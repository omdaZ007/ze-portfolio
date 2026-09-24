/* =========================================================================
   ZE — BUILD MODE
   Cinematic intro timeline (GSAP):
   IDEA → WIREFRAME → DESIGN → CODE → TEST → LAUNCH → WE BUILD IDEAS.
   Plays once per session · skippable · disabled for reduced motion / no GSAP
   ========================================================================= */
(function () {
    'use strict';

    var root = document.querySelector('[data-build-mode]');
    if (!root) return;

    var docEl = document.documentElement;
    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    var seen = false;
    try { seen = !!sessionStorage.getItem('ze-build-mode'); } catch (e) { }

    var finished = false;
    var locked = false;

    function markSeen() {
        try { sessionStorage.setItem('ze-build-mode', '1'); } catch (e) { }
    }

    function setContentInert(on) {
        var targets = document.querySelectorAll('#main-content, .site-navbar, #site-footer, .site-footer');
        Array.prototype.forEach.call(targets, function (el) {
            try { el.inert = on; } catch (e) { }
        });
    }

    function lock() {
        if (locked) return;
        locked = true;
        docEl.classList.add('bm-lock');
        setContentInert(true);
    }

    function unlock() {
        if (!locked) return;
        locked = false;
        docEl.classList.remove('bm-lock');
        setContentInert(false);
    }

    /* immediate dismissal (seen / reduced motion / no GSAP) */
    function dismissNow() {
        if (finished) return;
        finished = true;
        markSeen();
        docEl.classList.remove('bm-armed', 'bm-lock');
        docEl.classList.add('bm-done');
        setContentInert(false);
        if (root.parentNode) root.parentNode.removeChild(root);
    }

    if (reduceMotion || seen || !window.gsap) {
        dismissNow();
        return;
    }

    /* ---------------------------- sequence end --------------------------- */
    function finishSequence() {
        if (finished) return;
        finished = true;
        markSeen();
        docEl.classList.remove('bm-lock');
        setContentInert(false);
        root.classList.add('is-exiting');
        docEl.classList.remove('bm-armed');
        window.setTimeout(function () {
            docEl.classList.add('bm-done');
            if (root.parentNode) root.parentNode.removeChild(root);
        }, 900);
    }

    function skip() {
        if (finished) return;
        try { tl.kill(); } catch (e) { }
        finishSequence();
    }

    /* ------------------------------- setup -------------------------------- */
    var order = ['idea', 'wire', 'design', 'code', 'test', 'launch'];
    var phases = {};
    order.forEach(function (key) {
        phases[key] = root.querySelector('[data-phase="' + key + '"]');
    });
    var steps = Array.prototype.slice.call(root.querySelectorAll('.bm-steps li'));
    var lines = Array.prototype.slice.call(root.querySelectorAll('.bm-line'));
    var caret = root.querySelector('[data-bm-caret]');

    function setStep(key) {
        var idx = order.indexOf(key);
        steps.forEach(function (li, i) {
            li.classList.toggle('is-active', i === idx);
            li.classList.toggle('is-done', i < idx);
        });
    }

    function caretOn() {
        if (!caret) return;
        caret.classList.add('is-on');
        caret.style.opacity = '';
    }
    function caretOff() {
        if (!caret) return;
        caret.classList.remove('is-on');
        caret.style.opacity = '0';
    }
    function caretPlace(line) {
        if (!caret || !line) return;
        line.appendChild(caret);
    }

    var skipBtn = root.querySelector('[data-bm-skip]');
    if (skipBtn) skipBtn.addEventListener('click', skip);
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && !finished) skip();
    });

    lock();
    if (skipBtn) { try { skipBtn.focus({ preventScroll: true }); } catch (e) { skipBtn.focus(); } }

    var tl = window.gsap.timeline({ paused: true, onComplete: finishSequence });
    var g = window.gsap;

    /* ------------------------------ 1 · IDEA ------------------------------ */
    tl.call(function () { setStep('idea'); })
        .set(phases.idea, { autoAlpha: 1 })
        .to('.bm-idea-dot', { opacity: 1, scale: 1, duration: 0.55, ease: 'power2.out' })
        .to('.bm-idea-word', { clipPath: 'inset(0 0% 0 0)', duration: 0.85, ease: 'power2.inOut' }, '-=0.12')
        .to('.bm-idea-sketch path', { strokeDashoffset: 0, duration: 0.7, stagger: 0.2, ease: 'power1.inOut' }, '-=0.5')
        .to(phases.idea, { autoAlpha: 0, y: -26, duration: 0.42, ease: 'power2.in' }, '+=0.4');

    /* ---------------------------- 2 · WIREFRAME --------------------------- */
    tl.call(function () { setStep('wire'); })
        .set(phases.wire, { autoAlpha: 1 })
        .to(phases.wire.querySelectorAll('rect'), {
            strokeDashoffset: 0,
            duration: 0.65,
            stagger: 0.04,
            ease: 'power1.inOut'
        })
        .to(phases.wire, { autoAlpha: 0, duration: 0.35, ease: 'power2.in' }, '+=0.5');

    /* ----------------------------- 3 · DESIGN ----------------------------- */
    tl.call(function () { setStep('design'); })
        .set(phases.design, { autoAlpha: 1 })
        .from('.bm-d-frame', { scale: 0.94, opacity: 0, duration: 0.5, ease: 'power3.out' })
        .from('.bm-d-frame > *', { opacity: 0, y: 14, duration: 0.4, stagger: 0.09, ease: 'power3.out' }, '-=0.22')
        .from('.bm-d-cards .bm-d-card', { opacity: 0, y: 16, duration: 0.4, stagger: 0.1, ease: 'power3.out' }, '-=0.35')
        .to(phases.design, { autoAlpha: 0, duration: 0.3, ease: 'power2.in' }, '+=0.55');

    /* ------------------------------ 4 · CODE ------------------------------ */
    tl.call(function () { setStep('code'); })
        .set(phases.code, { autoAlpha: 1 })
        .from('.bm-code-card', { y: 24, opacity: 0, scale: 0.97, duration: 0.45, ease: 'power3.out' })
        .call(caretOn);

    lines.forEach(function (line) {
        tl.call(function () { caretPlace(line); })
            .to(line, { clipPath: 'inset(0 0% 0 0)', duration: 0.26, ease: 'none' }, '+=0.05');
    });

    tl.call(caretOff)
        .to(phases.code, { autoAlpha: 0, duration: 0.3, ease: 'power2.in' }, '+=0.55');

    /* ------------------------------- 5 · TEST ----------------------------- */
    var tracks = Array.prototype.slice.call(phases.test.querySelectorAll('.bm-track'));
    var frags = Array.prototype.slice.call(phases.test.querySelectorAll('.bm-frag'));

    tl.call(function () { setStep('test'); })
        .set(phases.test, { autoAlpha: 1 })
        .to(tracks, { strokeDashoffset: 0, duration: 0.6, stagger: 0.14, ease: 'power1.inOut' });

    /* fragments glide along the glowing tracks (SVG user units → stage px) */
    var scale = phases.test.getBoundingClientRect().width / 480 || 1;
    tracks.forEach(function (path, i) {
        var frag = frags[i];
        if (!frag) return;
        var len = path.getTotalLength();
        var proxy = { t: 0 };
        tl.call(function () { g.set(frag, { opacity: 1 }); })
            .to(proxy, {
                t: 1,
                duration: 1.2,
                ease: 'power1.inOut',
                onUpdate: function () {
                    var pt = path.getPointAtLength(proxy.t * len);
                    frag.style.transform =
                        'translate(' + (pt.x * scale) + 'px,' + (pt.y * scale) + 'px) translate(-50%,-50%)';
                },
                onComplete: function () { g.set(frag, { opacity: 0 }); }
            }, i === 0 ? '>' : '<' + (0.16));
    });

    tl.to('.bm-check', { opacity: 1, y: 0, duration: 0.4, stagger: 0.16, ease: 'power3.out' }, '-=0.4')
        .to(phases.test, { autoAlpha: 0, duration: 0.35, ease: 'power2.in' }, '+=0.55');

    /* ------------------------------ 6 · LAUNCH ---------------------------- */
    tl.call(function () { setStep('launch'); })
        .set(phases.launch, { autoAlpha: 1 })
        .from('.bm-browser', { scale: 0.86, y: 30, opacity: 0, duration: 0.6, ease: 'power3.out' })
        .to('.bm-browser-sheen', { xPercent: 260, opacity: 1, duration: 0.95, ease: 'power2.inOut' }, '-=0.25')
        .set('.bm-browser-sheen', { opacity: 0 })
        .to('.bm-browser', { scale: 3.4, opacity: 0, duration: 0.9, ease: 'power3.in' }, '+=0.4')
        .to('.bm-hud', { opacity: 0, duration: 0.5, ease: 'power2.out' }, '<0.15')

        /* final message */
        .call(function () { setStepDoneAll(); })
        .set('.bm-final', { autoAlpha: 1 })
        .from('.bm-slogan', { y: 36, opacity: 0, duration: 0.7, ease: 'power3.out' })
        .from('.bm-welcome', { y: 18, opacity: 0, duration: 0.55, ease: 'power3.out' }, '-=0.25')
        .to({}, { duration: 1.15 })
        .call(finishSequence);

    function setStepDoneAll() {
        steps.forEach(function (li) {
            li.classList.remove('is-active');
            li.classList.add('is-done');
        });
    }

    /* progress bar spans the whole timeline */
    tl.to('.bm-fill', { scaleX: 1, duration: tl.duration(), ease: 'none' }, 0);

    /* pause when tab is hidden */
    document.addEventListener('visibilitychange', function () {
        if (finished) return;
        if (document.hidden) tl.pause();
        else tl.resume();
    });

    tl.play(0);
})();
