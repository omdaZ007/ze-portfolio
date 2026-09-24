/* =========================================================================
   ZE — DIGITAL HEARTBEAT
   Particle-line pulse generated from a seed (per-project rhythm).
   Visual representation only — no measured metrics are claimed.
   Pauses when off-screen or when the tab is hidden.
   ========================================================================= */
(function () {
    'use strict';

    var root = document.querySelector('[data-heartbeat]');
    if (!root) return;

    var line = root.querySelector('[data-hb-line]');
    var glow = root.querySelector('[data-hb-glow]');
    var headPath = root.querySelector('[data-hb-headpath]');
    var head = root.querySelector('[data-hb-head]');
    var word = root.querySelector('[data-hb-word]');
    var metrics = Array.prototype.slice.call(root.querySelectorAll('[data-hb-metric]'));

    if (!line) return;

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var seed = parseInt(root.getAttribute('data-seed'), 10) || 11;
    var words = (root.getAttribute('data-words') || 'Performance|Responsive|Fast|Stable|Alive')
        .split('|').map(function (s) { return s.trim(); }).filter(Boolean);

    /* deterministic hash from seed + index */
    function rnd(n) {
        var x = Math.sin(n * 127.1 + seed * 311.7) * 43758.5453;
        return x - Math.floor(x);
    }

    /* --------------------------- waveform generation ----------------------- */
    var VB_W = 900;
    var BASE_Y = 130;
    var spikeLens = [];
    var cycleDuration = 6200;

    function buildWave() {
        var smooth = rnd(1) > 0.55;
        var cycles = smooth ? 3 + Math.floor(rnd(2) * 2) : 5 + Math.floor(rnd(3) * 2);
        var d = 'M 8 ' + BASE_Y;
        var spikes = [];

        for (var i = 0; i < cycles; i++) {
            var sx = 60 + ((i + 0.5) / cycles) * (VB_W - 120);
            var amp = (smooth ? 34 : 52) + rnd(i + 10) * (smooth ? 20 : 40);
            var dip = 10 + rnd(i + 20) * 14;

            if (smooth) {
                /* rounded pulse — smooth rhythm */
                d += ' L ' + (sx - 42) + ' ' + BASE_Y;
                d += ' Q ' + (sx - 14) + ' ' + (BASE_Y - amp) + ' ' + sx + ' ' + BASE_Y;
                d += ' Q ' + (sx + 14) + ' ' + (BASE_Y + dip * 0.4) + ' ' + (sx + 42) + ' ' + BASE_Y;
            } else {
                /* sharp technology heartbeat */
                d += ' L ' + (sx - 40) + ' ' + BASE_Y;
                d += ' L ' + (sx - 20) + ' ' + (BASE_Y + dip * 0.5);
                d += ' L ' + (sx - 8) + ' ' + (BASE_Y - amp);
                d += ' L ' + (sx + 4) + ' ' + (BASE_Y + amp * 0.42);
                d += ' L ' + (sx + 14) + ' ' + (BASE_Y - amp * 0.34);
                d += ' L ' + (sx + 26) + ' ' + BASE_Y;
                d += ' L ' + (sx + 44) + ' ' + BASE_Y;
            }
            spikes.push(sx);
        }

        d += ' L ' + (VB_W - 8) + ' ' + BASE_Y;

        line.setAttribute('d', d);
        if (glow) glow.setAttribute('d', d);
        if (headPath) headPath.setAttribute('d', d);
        spikeLens = spikes;
        cycleDuration = smooth ? 7600 : 5600;
    }

    buildWave();

    /* ------------------------------- pulse --------------------------------- */
    var wordIndex = 0;
    var metricIndex = 0;

    function pulse() {
        if (word && words.length) {
            word.textContent = words[wordIndex % words.length];
            wordIndex++;
            word.classList.remove('is-flash');
            void word.offsetWidth; /* restart animation */
            word.classList.add('is-flash');
        }
        if (metrics.length) {
            metrics.forEach(function (m) { m.classList.remove('is-hot'); });
            var m = metrics[metricIndex % metrics.length];
            metricIndex++;
            m.classList.add('is-hot');
            window.setTimeout(function () { m.classList.remove('is-hot'); }, 1400);
        }
    }

    /* ------------------------------ animation ------------------------------ */
    var raf = null;
    var lastTs = 0;
    var flowOffset = 0;
    var travelT = 0;
    var prevLen = 0;
    var visible = false;
    var totalLen = 0;
    var DASH_PERIOD = 9; /* dasharray 0.5 + gap 8.5 */

    function measureLen() {
        if (headPath && headPath.getTotalLength) totalLen = headPath.getTotalLength();
    }

    function tick(ts) {
        raf = null;
        if (!visible || document.hidden || reduceMotion) return;
        if (!lastTs) lastTs = ts;
        var dt = Math.min(64, ts - lastTs);
        lastTs = ts;

        /* particle flow along the line */
        flowOffset = (flowOffset + dt * 0.028) % DASH_PERIOD;
        line.style.strokeDashoffset = String(-flowOffset);

        /* travelling head */
        if (head && headPath && totalLen) {
            travelT += dt / cycleDuration;
            if (travelT >= 1) travelT -= 1;
            var len = travelT * totalLen;
            var pt = headPath.getPointAtLength(len);
            head.setAttribute('cx', String(pt.x));
            head.setAttribute('cy', String(pt.y));
            head.setAttribute('opacity', '1');

            /* crossing a spike (or wrapping) → pulse */
            var crossed = false;
            for (var i = 0; i < spikeLens.length; i++) {
                var sl = spikeLens[i] / VB_W * totalLen;
                if ((prevLen < sl && len >= sl) || (len < prevLen && sl > len && sl > prevLen)) {
                    crossed = true;
                    break;
                }
            }
            prevLen = len;
            if (crossed) pulse();
        }

        raf = requestAnimationFrame(tick);
    }

    function start() {
        if (raf || reduceMotion || !visible) return;
        if (!totalLen) measureLen();
        lastTs = 0;
        raf = requestAnimationFrame(tick);
    }

    function stop() {
        if (raf) cancelAnimationFrame(raf);
        raf = null;
        lastTs = 0;
        if (head) head.setAttribute('opacity', '0');
    }

    /* ------------------------------ visibility ------------------------------ */
    if ('IntersectionObserver' in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                visible = entry.isIntersecting;
                if (visible) start();
                else stop();
            });
        }, { threshold: 0.25 });
        io.observe(root);
    } else {
        visible = true;
        start();
    }

    document.addEventListener('visibilitychange', function () {
        if (document.hidden) stop();
        else start();
    });

    window.addEventListener('resize', function () {
        var prev = totalLen;
        measureLen();
        if (totalLen !== prev && visible && !raf) start();
    });

    /* -------------------------------- boot ---------------------------------- */
    if (reduceMotion) {
        /* static waveform + persistent first word */
        if (word && words.length) {
            word.textContent = words[0];
            word.classList.add('is-flash');
        }
        if (metrics[0]) metrics[0].classList.add('is-hot');
    } else {
        measureLen();
        if (visible) start();
    }
})();
