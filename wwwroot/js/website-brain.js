/* =========================================================================
   ZE — WEBSITE BRAIN
   Travelling signal on the circuit · node activation · info panel swap ·
   cursor proximity glow. Pauses entirely when off-screen.
   ========================================================================= */
(function () {
    'use strict';

    var root = document.querySelector('[data-brain]');
    if (!root) return;

    var stage = root.querySelector('[data-brain-stage]');
    var circuit = root.querySelector('[data-brain-circuit]');
    var signal = root.querySelector('[data-brain-signal]');
    var halo = root.querySelector('[data-brain-halo]');
    var panel = root.querySelector('[data-brain-panel]');
    if (!stage) return;

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var nodes = Array.prototype.slice.call(root.querySelectorAll('[data-brain-node]'));
    if (!nodes.length) return;

    var panelLabel = root.querySelector('[data-brain-panel-label]');
    var panelDesc = root.querySelector('[data-brain-panel-desc]');
    var panelChips = root.querySelector('[data-brain-panel-chips]');
    var panelIcon = root.querySelector('[data-brain-panel-icon]');

    var activeIndex = -1;
    var visible = false;
    var hovering = false;
    var swapTimer = null;

    /* ------------------------------ activate ------------------------------ */
    function activate(index) {
        if (index < 0 || index >= nodes.length || index === activeIndex) return;
        activeIndex = index;
        var node = nodes[index];

        nodes.forEach(function (n, i) {
            n.classList.toggle('is-active', i === index);
            n.setAttribute('aria-pressed', String(i === index));
        });

        if (!panel) return;
        panel.classList.add('is-swapping');
        window.clearTimeout(swapTimer);
        swapTimer = window.setTimeout(function () {
            if (panelLabel) panelLabel.textContent = node.getAttribute('data-label') || '';
            if (panelDesc) panelDesc.textContent = node.getAttribute('data-desc') || '';
            if (panelIcon) panelIcon.className = node.getAttribute('data-icon') || 'fa-solid fa-brain';
            if (panelChips) {
                var techs = (node.getAttribute('data-techs') || '').split('•')
                    .map(function (s) { return s.trim(); })
                    .filter(Boolean);
                panelChips.innerHTML = techs.map(function (t) {
                    return '<span class="chip"></span>';
                }).join('');
                var chips = panelChips.children;
                for (var i = 0; i < chips.length; i++) chips[i].textContent = techs[i];
            }
            panel.classList.remove('is-swapping');
        }, 190);
    }

    nodes.forEach(function (node, i) {
        node.addEventListener('click', function () { activate(i); });
        node.addEventListener('focus', function () { activate(i); });
    });

    /* --------------------------- node pixel points ------------------------ */
    function nodePoints() {
        var sr = stage.getBoundingClientRect();
        return nodes.map(function (n) {
            var core = n.querySelector('.brain-node-core') || n;
            var r = core.getBoundingClientRect();
            return { x: r.left - sr.left + r.width / 2, y: r.top - sr.top + r.height / 2 };
        });
    }

    /* ------------------------------- proximity ---------------------------- */
    var pointer = { x: -9999, y: -9999 };
    var proximityRaf = null;

    function applyProximity() {
        proximityRaf = null;
        var pts = nodePoints();
        for (var i = 0; i < pts.length; i++) {
            var dx = pts[i].x - pointer.x;
            var dy = pts[i].y - pointer.y;
            var dist = Math.sqrt(dx * dx + dy * dy);
            var near = Math.max(0, 1 - dist / 190);
            nodes[i].style.setProperty('--near', near.toFixed(3));
        }
    }

    if (window.matchMedia('(pointer: fine)').matches && !reduceMotion) {
        stage.addEventListener('pointermove', function (e) {
            var r = stage.getBoundingClientRect();
            pointer.x = e.clientX - r.left;
            pointer.y = e.clientY - r.top;
            if (!proximityRaf) proximityRaf = requestAnimationFrame(applyProximity);
        }, { passive: true });

        stage.addEventListener('pointerleave', function () {
            pointer.x = pointer.y = -9999;
            nodes.forEach(function (n) { n.style.setProperty('--near', '0'); });
        }, { passive: true });

        stage.addEventListener('pointerenter', function () { hovering = true; });
        stage.addEventListener('pointerleave', function () { hovering = false; });
    }

    /* ---------------------------- travelling signal ------------------------ */
    var signalRaf = null;
    var lastTs = 0;
    var progress = 0;
    var lap = 0;
    var lit = {};
    var LOOP_MS = 9500;

    var netVisible = true;
    function checkNet() {
        var net = root.querySelector('.brain-net');
        netVisible = !!net && window.getComputedStyle(net).display !== 'none' &&
            window.innerWidth > 860;
    }

    function tick(ts) {
        signalRaf = null;
        if (!visible || reduceMotion || !netVisible) return;
        if (!lastTs) lastTs = ts;
        var dt = Math.min(64, ts - lastTs);
        lastTs = ts;

        progress += dt / LOOP_MS;
        if (progress >= 1) {
            progress -= 1;
            lap++;
            lit = {};
        }

        if (!circuit || !circuit.getTotalLength) return;
        var len = circuit.getTotalLength();
        var pt = circuit.getPointAtLength(progress * len);

        if (signal) {
            signal.setAttribute('cx', String(pt.x));
            signal.setAttribute('cy', String(pt.y));
            signal.setAttribute('opacity', '1');
        }
        if (halo) {
            halo.setAttribute('cx', String(pt.x));
            halo.setAttribute('cy', String(pt.y));
            halo.setAttribute('opacity', '1');
        }

        /* convert user units → stage px to test node proximity */
        var sr = stage.getBoundingClientRect();
        var sx = pt.x * (sr.width / 1000);
        var sy = pt.y * (sr.height / 560);
        var pts = nodePoints();
        for (var i = 0; i < pts.length; i++) {
            if (lit[i]) continue;
            var dx = pts[i].x - sx;
            var dy = pts[i].y - sy;
            if (dx * dx + dy * dy < 46 * 46) {
                lit[i] = true;
                activate(i);
            }
        }

        signalRaf = requestAnimationFrame(tick);
    }

    function startSignal() {
        if (reduceMotion || signalRaf || !visible) return;
        checkNet();
        if (!netVisible) return;
        lastTs = 0;
        signalRaf = requestAnimationFrame(tick);
    }

    function stopSignal() {
        if (signalRaf) cancelAnimationFrame(signalRaf);
        signalRaf = null;
        lastTs = 0;
        if (signal) signal.setAttribute('opacity', '0');
        if (halo) halo.setAttribute('opacity', '0');
    }

    /* ------------------------------ auto cycle ---------------------------- */
    var cycleTimer = null;

    function startCycle() {
        if (reduceMotion || cycleTimer) return;
        cycleTimer = window.setInterval(function () {
            if (!visible || hovering || document.hidden) return;
            var next = (activeIndex + 1) % nodes.length;
            activate(next);
        }, 4400);
    }

    function stopCycle() {
        window.clearInterval(cycleTimer);
        cycleTimer = null;
    }

    /* ------------------------------- visibility ---------------------------- */
    if ('IntersectionObserver' in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                visible = entry.isIntersecting;
                if (visible) {
                    startSignal();
                    startCycle();
                } else {
                    stopSignal();
                }
            });
        }, { threshold: 0.2 });
        io.observe(root);
    } else {
        visible = true;
        startSignal();
        startCycle();
    }

    document.addEventListener('visibilitychange', function () {
        if (document.hidden) stopSignal();
        else startSignal();
    });

    window.addEventListener('resize', function () {
        checkNet();
        if (netVisible && visible && !signalRaf && !reduceMotion) startSignal();
    });

    /* ------------------------------ boot ---------------------------------- */
    activate(0);
})();
