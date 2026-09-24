/* =========================================================
   ZE Admin — interactions
   ========================================================= */
(function () {
    "use strict";

    var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    /* ---------- sidebar ---------- */
    var sidebar = document.getElementById("adminSidebar");
    var burger = document.getElementById("adminSidebarToggle");
    var collapseBtn = document.getElementById("adminSidebarCollapse");
    var backdrop = document.getElementById("adminSidebarBackdrop");
    var STORAGE_KEY = "zeAdminSidebarCollapsed";
    var desktopMq = window.matchMedia("(min-width: 901px)");

    function isDesktop() { return desktopMq.matches; }

    function applyBackdrop(show) {
        if (!backdrop) return;
        backdrop.classList.toggle("show", !!show);
    }

    if (sidebar) {
        try {
            if (localStorage.getItem(STORAGE_KEY) === "1" && isDesktop()) {
                sidebar.classList.add("collapsed");
            }
        } catch (e) { /* ignore */ }
    }

    function setCollapsed(collapsed) {
        if (!sidebar) return;
        sidebar.classList.toggle("collapsed", collapsed);
        try {
            localStorage.setItem(STORAGE_KEY, collapsed ? "1" : "0");
        } catch (e) { /* ignore */ }
        if (collapseBtn) {
            collapseBtn.setAttribute("aria-label", collapsed ? "Expand sidebar" : "Collapse sidebar");
            collapseBtn.setAttribute("title", collapsed ? "Expand sidebar" : "Collapse sidebar");
        }
    }

    if (collapseBtn && sidebar) {
        collapseBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            if (!isDesktop()) return;
            setCollapsed(!sidebar.classList.contains("collapsed"));
        });
    }

    if (burger && sidebar) {
        burger.addEventListener("click", function () {
            if (isDesktop()) {
                setCollapsed(!sidebar.classList.contains("collapsed"));
            } else {
                var open = sidebar.classList.toggle("open");
                applyBackdrop(open);
            }
        });
        document.addEventListener("click", function (e) {
            if (isDesktop()) return;
            if (!sidebar.classList.contains("open")) return;
            if (sidebar.contains(e.target) || burger.contains(e.target)) return;
            sidebar.classList.remove("open");
            applyBackdrop(false);
        });
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && !isDesktop() && sidebar.classList.contains("open")) {
                sidebar.classList.remove("open");
                applyBackdrop(false);
            }
        });
    }

    if (backdrop) {
        backdrop.addEventListener("click", function () {
            sidebar && sidebar.classList.remove("open");
            applyBackdrop(false);
        });
    }

    desktopMq.addEventListener("change", function (e) {
        if (e.matches) {
            sidebar && sidebar.classList.remove("open");
            applyBackdrop(false);
            try {
                if (localStorage.getItem(STORAGE_KEY) === "1") {
                    sidebar && sidebar.classList.add("collapsed");
                }
            } catch (err) { /* ignore */ }
        } else {
            sidebar && sidebar.classList.remove("collapsed");
        }
    });

    /* ---------- auto-dismiss alerts ---------- */
    document.querySelectorAll("[data-auto-dismiss]").forEach(function (alert) {
        var closeBtn = alert.querySelector(".admin-alert-close");
        if (closeBtn) closeBtn.addEventListener("click", function () { alert.remove(); });
        window.setTimeout(function () {
            if (!alert.isConnected) return;
            alert.style.transition = "opacity .4s ease, transform .4s ease";
            alert.style.opacity = "0";
            alert.style.transform = "translateY(-6px)";
            window.setTimeout(function () { alert.remove(); }, 420);
        }, 6000);
    });

    /* ---------- confirm delete modal ---------- */
    var modal = document.getElementById("confirmDeleteModal");
    var confirmForm = document.getElementById("confirmDeleteForm");
    var confirmMessage = document.getElementById("confirmDeleteMessage");

    if (modal && confirmForm) {
        var openModal = function (url, message) {
            confirmForm.setAttribute("action", url);
            if (confirmMessage) {
                confirmMessage.textContent = message ||
                    "Are you sure you want to delete this item? This action cannot be undone.";
            }
            modal.hidden = false;
            document.body.style.overflow = "hidden";
            var submit = confirmForm.querySelector("button[type=submit]");
            if (submit) submit.focus();
        };

        var closeModal = function () {
            modal.hidden = true;
            document.body.style.overflow = "";
        };

        document.addEventListener("click", function (e) {
            var trigger = e.target.closest("[data-confirm-href]");
            if (trigger) {
                e.preventDefault();
                openModal(trigger.getAttribute("data-confirm-href"),
                    trigger.getAttribute("data-confirm-message"));
                return;
            }
            if (e.target.closest("[data-confirm-cancel]")) closeModal();
        });

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && !modal.hidden) closeModal();
        });
    }

    /* ---------- image preview (create/edit forms) ---------- */
    document.querySelectorAll("input[type=file][data-preview]").forEach(function (input) {
        input.addEventListener("change", function () {
            var selector = input.getAttribute("data-preview");
            var target = selector ? document.querySelector(selector) : null;
            if (!target) return;
            var file = input.files && input.files[0];
            if (!file) return;
            if (!/^image\/(png|jpe?g|webp)$/i.test(file.type)) {
                window.alert("Please choose a JPG, PNG, or WEBP image.");
                input.value = "";
                return;
            }
            if (file.size > 5 * 1024 * 1024) {
                window.alert("Image must be 5 MB or smaller.");
                input.value = "";
                return;
            }
            var url = URL.createObjectURL(file);
            if (target.tagName === "IMG") {
                target.src = url;
            } else {
                target.innerHTML = "";
                var img = document.createElement("img");
                img.src = url;
                img.alt = "Preview";
                target.appendChild(img);
            }
        });
    });

    /* ---------- password visibility (login) ---------- */
    document.querySelectorAll(".admin-password-toggle").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var wrap = btn.closest(".admin-password-wrap");
            var input = wrap && wrap.querySelector("input");
            if (!input) return;
            var show = input.type === "password";
            input.type = show ? "text" : "password";
            var icon = btn.querySelector("i");
            if (icon) icon.className = show ? "fa-solid fa-eye-slash" : "fa-solid fa-eye";
        });
    });

    /* ---------- search form: reset page on submit ---------- */
    document.querySelectorAll("form[data-reset-page]").forEach(function (form) {
        form.addEventListener("submit", function () {
            var pageInput = form.querySelector("input[name=page]");
            if (pageInput) pageInput.value = "1";
        });
    });

    /* ---------- smooth section jump from sidebar (no-op guard) ---------- */
    if (!reduceMotion) {
        document.querySelectorAll(".admin-stat").forEach(function (card, i) {
            card.style.opacity = "0";
            card.style.transform = "translateY(8px)";
            card.style.transition = "opacity .4s ease " + (i * 40) + "ms, transform .4s ease " + (i * 40) + "ms";
            requestAnimationFrame(function () {
                card.style.opacity = "1";
                card.style.transform = "";
            });
        });
    }
})();
