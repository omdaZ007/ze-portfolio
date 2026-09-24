/* =========================================================================
   ZE — DIGITAL CITY
   Scroll-driven camera pan through the districts (desktop).
   Uses GSAP ScrollTrigger when available (smoothed scrub), otherwise a
   lightweight scroll listener with lerp. Mobile uses the static city map.
   ========================================================================= */
(function () {
    'use strict';

    var root = document.querySelector('[data-city]');
    if (!root) return;

    var viewport = root.querySelector('[data-city-viewport]');
    var world = root.querySelector('[data-city-world]');
    var mid = root.querySelector('[data-city-mid]');
    var far = root.querySelector('[data-city-far]');
    var near = root.querySelector('[data-city-near]');
    var hudLabel = root.querySelector('[data-city-hud]');
    var buildings = Array.prototype.slice.call(root.querySelectorAll('[data-city-building]'));

    if (!viewport || !world || !mid) return;

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var mobileMq = window.matchMedia('(max-width: 860px)');

    var maxX = 0;
    var currentX = 0;
    var targetX = 0;
    var currentBuilding = -1;
    var usingScrollTrigger = false;
    var raf = null;

    /* ------------------------------- metrics -------------------------------- */
    function measure() {
        if (mobileMq.matches) return;
        maxX = Math.max(0, world.offsetWidth - viewport.offsetWidth);
    }

    function applyCamera(x) {
        if (mid) mid.style.transform = 'translate3d(' + (-x) + 'px,0,0)';
        if (far) far.style.transform = 'translate3d(' + (-x * 0.45) + 'px,0,0)';
        if (near) near.style.transform = 'translate3d(' + (-x * 1.18) + 'px,0,0)';
        updateHud(x);
    }

    /* --------------------------- current district -------------------------- */
    function updateHud(x) {
        if (!buildings.length) return;
        var camCenter = x + viewport.offsetWidth / 2;
        var best = 0;
        var bestDist = Infinity;
        for (var i = 0; i < buildings.length; i++) {
            var b = buildings[i];
            var bc = b.offsetLeft + b.offsetWidth / 2;
            var d = Math.abs(bc - camCenter);
            if (d < bestDist) { bestDist = d; best = i; }
        }
        if (best !== currentBuilding) {
            currentBuilding = best;
            for (var j = 0; j < buildings.length; j++) {
                buildings[j].classList.toggle('is-current', j === best);
            }
            if (hudLabel) hudLabel.textContent = buildings[best].getAttribute('data-name') || '';
        }
    }

    /* --------------------------- fallback scrolling ------------------------- */
    function onScroll() {
        if (mobileMq.matches || usingScrollTrigger) return;
        var rect = root.getBoundingClientRect();
        var travel = rect.height - window.innerHeight;
        var p = travel > 0 ? Math.min(1, Math.max(0, -rect.top / travel)) : 0;
        targetX = p * maxX;
        if (!raf) raf = requestAnimationFrame(lerpTick);
    }

    function lerpTick() {
        raf = null;
        var f = reduceMotion ? 1 : 0.1;
        currentX += (targetX - currentX) * f;
        if (Math.abs(targetX - currentX) < 0.4) currentX = targetX;
        applyCamera(currentX);
        if (currentX !== targetX) raf = requestAnimationFrame(lerpTick);
    }

    /* ------------------------------ GSAP scrub ------------------------------ */
    function initScrollTrigger() {
        window.gsap.registerPlugin(window.ScrollTrigger);
        var proxy = { p: 0 };
        window.gsap.to(proxy, {
            p: 1,
            ease: 'none',
            scrollTrigger: {
                trigger: root,
                start: 'top top',
                end: 'bottom bottom',
                scrub: reduceMotion ? true : 0.7,
                invalidateOnRefresh: true
            },
            onUpdate: function () {
                currentX = targetX = proxy.p * maxX;
                applyCamera(currentX);
            }
        });
        usingScrollTrigger = true;
    }

    /* -------------------------------- boot ----------------------------------- */
    function boot() {
        if (mobileMq.matches) {
            stopAll();
            return;
        }
        measure();
        currentX = targetX = 0;
        applyCamera(0);

        if (!usingScrollTrigger && window.gsap && window.ScrollTrigger) {
            initScrollTrigger();
        } else if (!usingScrollTrigger) {
            window.addEventListener('scroll', onScroll, { passive: true });
            onScroll();
        }
    }

    function stopAll() {
        if (raf) cancelAnimationFrame(raf);
        raf = null;
        if (mid) mid.style.transform = '';
        if (far) far.style.transform = '';
        if (near) near.style.transform = '';
    }

    var resizeTimer = null;
    window.addEventListener('resize', function () {
        window.clearTimeout(resizeTimer);
        resizeTimer = window.setTimeout(function () {
            if (mobileMq.matches) { stopAll(); return; }
            measure();
            if (!usingScrollTrigger && window.gsap && window.ScrollTrigger) initScrollTrigger();
            else if (!usingScrollTrigger) onScroll();
            if (usingScrollTrigger && window.ScrollTrigger) window.ScrollTrigger.refresh();
        }, 160);
    });

    if (document.readyState === 'complete') boot();
    else window.addEventListener('load', boot);
})();
