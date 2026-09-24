/* =========================================================================
   ZE — public site behaviour
   Navbar transitions · mobile menu · scroll reveal · parallax · particles
   ========================================================================= */
(function () {
    'use strict';

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    /* ----------------------------- Navbar ------------------------------- */
    var navbar = document.getElementById('navbar');
    function onScroll() {
        if (!navbar) return;
        navbar.classList.toggle('scrolled', window.scrollY > 24);
    }
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();

    /* -------------------------- Mobile menu ----------------------------- */
    var toggle = document.getElementById('nav-toggle');
    var menu = document.getElementById('nav-menu');
    var backdrop = document.getElementById('nav-backdrop');

    function closeMenu() {
        if (!menu) return;
        menu.classList.remove('open');
        backdrop && backdrop.classList.remove('show');
        toggle && toggle.setAttribute('aria-expanded', 'false');
        document.body.style.overflow = '';
    }

    if (toggle && menu) {
        toggle.addEventListener('click', function () {
            var open = menu.classList.toggle('open');
            backdrop && backdrop.classList.toggle('show', open);
            toggle.setAttribute('aria-expanded', String(open));
            document.body.style.overflow = open ? 'hidden' : '';
        });
        backdrop && backdrop.addEventListener('click', closeMenu);
        menu.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', closeMenu);
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') closeMenu();
        });
    }

    /* -------------------------- Scroll reveal --------------------------- */
    var revealItems = document.querySelectorAll('[data-reveal]');
    if (reduceMotion || !('IntersectionObserver' in window)) {
        revealItems.forEach(function (el) { el.classList.add('is-visible'); });
    } else {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

        revealItems.forEach(function (el, index) {
            el.style.transitionDelay = Math.min(index % 4, 3) * 70 + 'ms';
            observer.observe(el);
        });
    }

    /* ---------------------------- Parallax ------------------------------ */
    var parallax = document.querySelector('[data-parallax]');
    if (parallax && !reduceMotion && window.matchMedia('(pointer: fine)').matches) {
        var layers = parallax.querySelectorAll('.hero-logo-3d, .hero-card, .hero-island, .hero-plant');
        window.addEventListener('mousemove', function (e) {
            var cx = window.innerWidth / 2;
            var cy = window.innerHeight / 2;
            var dx = (e.clientX - cx) / cx;
            var dy = (e.clientY - cy) / cy;
            layers.forEach(function (layer, i) {
                var depth = (i % 3 + 1) * 6;
                layer.style.translate = (dx * depth).toFixed(2) + 'px ' + (dy * depth).toFixed(2) + 'px';
            });
        }, { passive: true });
    }

    /* ---------------------------- Particles ----------------------------- */
    var canvas = document.getElementById('hero-particles');
    if (canvas && !reduceMotion) {
        var ctx = canvas.getContext('2d');
        var particles = [];
        var width, height, raf;

        function resize() {
            width = canvas.width = canvas.offsetWidth;
            height = canvas.height = canvas.offsetHeight;
        }

        function create() {
            particles = [];
            var count = Math.min(70, Math.floor(width / 22));
            for (var i = 0; i < count; i++) {
                particles.push({
                    x: Math.random() * width,
                    y: Math.random() * height,
                    r: Math.random() * 1.8 + 0.4,
                    vx: (Math.random() - 0.5) * 0.22,
                    vy: (Math.random() - 0.5) * 0.22,
                    gold: Math.random() > 0.55
                });
            }
        }

        function tick() {
            ctx.clearRect(0, 0, width, height);
            for (var i = 0; i < particles.length; i++) {
                var p = particles[i];
                p.x += p.vx;
                p.y += p.vy;
                if (p.x < 0 || p.x > width) p.vx *= -1;
                if (p.y < 0 || p.y > height) p.vy *= -1;
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
                ctx.fillStyle = p.gold ? 'rgba(245,185,66,0.55)' : 'rgba(56,167,255,0.5)';
                ctx.fill();
            }
            raf = requestAnimationFrame(tick);
        }

        resize();
        create();
        tick();
        window.addEventListener('resize', function () {
            cancelAnimationFrame(raf);
            resize();
            create();
            tick();
        });
    }

    /* ------------------------ Active nav link --------------------------- */
    var sections = ['home', 'about', 'services', 'skills', 'projects', 'dna', 'contact', 'founders']
        .map(function (id) { return document.getElementById(id); })
        .filter(Boolean);

    if (sections.length && 'IntersectionObserver' in window) {
        var navLinks = Array.prototype.slice.call(document.querySelectorAll('.nav-menu .nav-link'));
        var spy = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                var id = entry.target.id;
                navLinks.forEach(function (link) {
                    var target = (link.getAttribute('href') || '').replace('/#', '#').replace(/^\/$/, '#home');
                    var active = target === '#' + id;
                    link.classList.toggle('active', active);
                });
            });
        }, { rootMargin: '-45% 0px -50% 0px' });

        sections.forEach(function (section) { spy.observe(section); });
    }
})();
