/* =========================================================================
   ZE — PROJECT RADAR
   Database-driven orbit · hover slows + previews · click flies the node
   to the centre and expands into a detail modal. Mobile uses the timeline.
   ========================================================================= */
(function () {
    'use strict';

    var root = document.querySelector('[data-radar]');
    if (!root) return;

    var stage = root.querySelector('[data-radar-stage]');
    var orbitsEl = root.querySelector('[data-radar-orbits]');
    var preview = root.querySelector('[data-radar-preview]');
    var modal = root.querySelector('[data-radar-modal]');
    var nodes = Array.prototype.slice.call(root.querySelectorAll('[data-radar-node]'));

    if (!stage || !nodes.length) return;

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var mobileMq = window.matchMedia('(max-width: 768px)');
    if (mobileMq.matches) return; /* mobile renders the vertical timeline instead */

    /* ------------------------------- state -------------------------------- */
    var cx = 0, cy = 0;
    var rotation = 0;
    var speedFactor = 1;      /* lerps to 0 while a node is hovered */
    var baseSpeed = 0.16;     /* deg per frame @60fps — slow, satellite-like */
    var visible = false;
    var raf = null;
    var lastTs = 0;
    var hoverCount = 0;

    nodes.forEach(function (el) {
        el._st = {
            baseAngle: 0,
            radius: 0,
            curX: 0,
            curY: 0,
            hot: 0,            /* extra radius lerp */
            flying: false,
            returning: false,
            init: false
        };
    });

    /* ------------------------------- layout -------------------------------- */
    function layout() {
        var r = stage.getBoundingClientRect();
        var w = r.width;
        cx = w / 2;
        cy = r.height / 2;

        var twoRings = nodes.length > 8;
        var r1 = Math.min(w, r.height) * (twoRings ? 0.27 : 0.36);
        var r2 = Math.min(w, r.height) * 0.40;

        var ringCounts = [0, 0];
        nodes.forEach(function (el) {
            if (twoRings) ringCounts[String(el.style.getPropertyValue('--ring')).trim() === '1' ? 1 : 0]++;
        });

        var ringSeen = [0, 0];
        nodes.forEach(function (el, i) {
            var ring = twoRings ? (i % 2) : 0;
            var count = twoRings ? ringCounts[ring] : nodes.length;
            var offset = ring === 1 ? 45 : 0;
            var angle = offset + (ringSeen[ring] / Math.max(1, count)) * 360;
            ringSeen[ring]++;

            var st = el._st;
            st.baseAngle = angle;
            st.radius = ring === 1 ? r2 : r1;
            if (!st.init) {
                st.init = true;
                var pos = posFor(st, angle);
                st.curX = pos.x;
                st.curY = pos.y;
            }
        });
        render();
    }

    function posFor(st, angle) {
        var rad = angle * Math.PI / 180;
        var r = st.radius + st.hot;
        return { x: cx + Math.cos(rad) * r, y: cy + Math.sin(rad) * r };
    }

    function render() {
        nodes.forEach(function (el) {
            var st = el._st;
            var target;
            if (st.flying) {
                target = { x: cx, y: cy };
            } else {
                target = posFor(st, st.baseAngle + rotation);
            }
            var f = (st.flying || st.returning) ? 0.13 : 1;
            st.curX += (target.x - st.curX) * f;
            st.curY += (target.y - st.curY) * f;

            if (st.returning && !st.flying) {
                var dx = target.x - st.curX;
                var dy = target.y - st.curY;
                if (dx * dx + dy * dy < 9) st.returning = false;
            }
            el.style.transform =
                'translate(' + st.curX.toFixed(1) + 'px,' + st.curY.toFixed(1) + 'px) translate(-50%,-50%)';
        });
    }

    /* -------------------------------- loop --------------------------------- */
    function tick(ts) {
        raf = null;
        if (!visible || document.hidden) return;
        if (!lastTs) lastTs = ts;
        var dt = Math.min(64, ts - lastTs) / 16.67;
        lastTs = ts;

        var targetFactor = hoverCount > 0 || modalOpen ? 0 : 1;
        speedFactor += (targetFactor - speedFactor) * 0.07;
        if (!reduceMotion) rotation += baseSpeed * speedFactor * dt;

        nodes.forEach(function (el) {
            var st = el._st;
            var targetHot = el.classList.contains('is-hot') && !st.flying ? 20 : 0;
            st.hot += (targetHot - st.hot) * 0.12;
        });

        render();
        raf = requestAnimationFrame(tick);
    }

    function start() {
        if (raf || mobileMq.matches || reduceMotion) return;
        lastTs = 0;
        raf = requestAnimationFrame(tick);
    }
    function stop() {
        if (raf) cancelAnimationFrame(raf);
        raf = null;
        lastTs = 0;
    }

    /* ------------------------------ preview -------------------------------- */
    var previewTimer = null;
    var previewOpen = false;

    function data(el) {
        return {
            title: el.getAttribute('data-title') || '',
            desc: el.getAttribute('data-desc') || '',
            techs: (el.getAttribute('data-techs') || '').split('•').map(function (s) { return s.trim(); }).filter(Boolean),
            href: el.getAttribute('data-href') || '/projects',
            gh: el.getAttribute('data-gh') || '',
            live: el.getAttribute('data-live') || '',
            img: el.getAttribute('data-img') || '/images/browser-window.png'
        };
    }

    function fillChips(container, techs) {
        if (!container) return;
        container.innerHTML = '';
        techs.forEach(function (t) {
            var span = document.createElement('span');
            span.className = 'chip';
            span.textContent = t;
            container.appendChild(span);
        });
    }

    function showPreview(el) {
        if (!preview || mobileMq.matches) return;
        window.clearTimeout(previewTimer);
        var d = data(el);
        var img = preview.querySelector('[data-rp-img]');
        var title = preview.querySelector('[data-rp-title]');
        var desc = preview.querySelector('[data-rp-desc]');
        var href = preview.querySelector('[data-rp-href]');
        var gh = preview.querySelector('[data-rp-gh]');
        if (img) { img.src = d.img; img.alt = d.title + ' — preview'; }
        if (title) title.textContent = d.title;
        if (desc) desc.textContent = d.desc;
        if (href) href.setAttribute('href', d.href);
        if (gh) {
            if (d.gh) { gh.href = d.gh; gh.hidden = false; }
            else gh.hidden = true;
        }
        fillChips(preview.querySelector('[data-rp-techs]'), d.techs);

        if (!previewOpen) {
            preview.hidden = false;
            requestAnimationFrame(function () { preview.classList.add('is-open'); });
            previewOpen = true;
        }
    }

    function hidePreview(delay) {
        if (!preview) return;
        window.clearTimeout(previewTimer);
        previewTimer = window.setTimeout(function () {
            if (hoverCount > 0) return;
            preview.classList.remove('is-open');
            window.setTimeout(function () {
                if (!previewOpen) preview.hidden = true;
            }, 420);
            previewOpen = false;
        }, delay || 260);
    }

    /* -------------------------- hover / focus hooks ------------------------ */
    nodes.forEach(function (el) {
        function enter() {
            hoverCount++;
            el.classList.add('is-hot');
            showPreview(el);
        }
        function leave() {
            hoverCount = Math.max(0, hoverCount - 1);
            el.classList.remove('is-hot');
            hidePreview(200);
        }
        el.addEventListener('pointerenter', enter);
        el.addEventListener('pointerleave', leave);
        el.addEventListener('focus', enter);
        el.addEventListener('blur', leave);
    });

    if (preview) {
        preview.addEventListener('pointerenter', function () { window.clearTimeout(previewTimer); });
        preview.addEventListener('pointerleave', function () { if (hoverCount === 0) hidePreview(200); });
    }

    /* -------------------------------- modal -------------------------------- */
    var modalOpen = false;
    var flyingEl = null;
    var lastFocus = null;

    function openModal(el) {
        if (!modal || modalOpen) return;
        var d = data(el);
        var img = modal.querySelector('[data-rm-img]');
        var title = modal.querySelector('[data-rm-title]');
        var desc = modal.querySelector('[data-rm-desc]');
        var href = modal.querySelector('[data-rm-href]');
        var gh = modal.querySelector('[data-rm-gh]');
        var live = modal.querySelector('[data-rm-live]');
        if (img) { img.src = d.img; img.alt = d.title + ' — project preview'; }
        if (title) title.textContent = d.title;
        if (desc) desc.textContent = d.desc;
        if (href) href.setAttribute('href', d.href);
        if (gh) {
            if (d.gh) { gh.href = d.gh; gh.hidden = false; }
            else gh.hidden = true;
        }
        if (live) {
            if (d.live) { live.href = d.live; live.hidden = false; }
            else live.hidden = true;
        }
        fillChips(modal.querySelector('[data-rm-techs]'), d.techs);

        lastFocus = document.activeElement;
        modalOpen = true;
        modal.hidden = false;
        document.body.classList.add('radar-open');
        if (preview) { preview.classList.remove('is-open'); previewOpen = false; }
        var closeBtn = modal.querySelector('.radar-modal-x');
        if (closeBtn) { try { closeBtn.focus({ preventScroll: true }); } catch (e) { closeBtn.focus(); } }
    }

    function closeModal() {
        if (!modalOpen) return;
        modalOpen = false;
        modal.hidden = true;
        document.body.classList.remove('radar-open');
        if (flyingEl) {
            var st = flyingEl._st;
            st.flying = false;
            st.returning = true;
            flyingEl.classList.remove('is-flying', 'is-selected');
            flyingEl = null;
        }
        if (lastFocus && lastFocus.focus) { try { lastFocus.focus({ preventScroll: true }); } catch (e) { } }
    }

    if (modal) {
        Array.prototype.forEach.call(modal.querySelectorAll('[data-radar-close]'), function (el) {
            el.addEventListener('click', closeModal);
        });
    }
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && modalOpen) closeModal();
    });

    /* ------------------------- click → fly to centre ----------------------- */
    nodes.forEach(function (el) {
        el.addEventListener('click', function () {
            if (modalOpen) return;
            hidePreview(0);
            hoverCount = 0;
            el.classList.remove('is-hot');

            if (reduceMotion) {
                openModal(el);
                return;
            }

            el.classList.add('is-flying', 'is-selected');
            flyingEl = el;
            el._st.flying = true;

            var check = window.setInterval(function () {
                var st = el._st;
                var dx = cx - st.curX;
                var dy = cy - st.curY;
                if (!flyingEl || flyingEl !== el) { window.clearInterval(check); return; }
                if (dx * dx + dy * dy < 64) {
                    window.clearInterval(check);
                    st.flying = false;
                    openModal(el);
                }
            }, 60);
        });
    });

    /* ------------------------------ visibility ----------------------------- */
    if ('IntersectionObserver' in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                visible = entry.isIntersecting;
                if (visible) start();
                else stop();
            });
        }, { threshold: 0.12 });
        io.observe(root);
    } else {
        visible = true;
        start();
    }

    document.addEventListener('visibilitychange', function () {
        if (document.hidden) stop();
        else if (visible) start();
    });

    var resizeTimer = null;
    window.addEventListener('resize', function () {
        window.clearTimeout(resizeTimer);
        resizeTimer = window.setTimeout(layout, 150);
    });

    /* -------------------------------- boot --------------------------------- */
    layout();
    start();
})();
