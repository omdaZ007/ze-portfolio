/* =========================================================================
   ZE — Project DNA behaviour
   SVG spine drawing · node reveals · travelling particle · project switch
   ========================================================================= */
(function () {
    'use strict';

    var root = document.querySelector('[data-dna-root]');
    if (!root) return;

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (!reduceMotion) root.classList.add('js-anim');

    var projects = [];
    try {
        projects = JSON.parse(root.getAttribute('data-projects') || '[]');
    } catch (e) {
        projects = [];
    }
    if (!Array.isArray(projects) || projects.length === 0) return;

    var helix = root.querySelector('[data-dna-helix]');
    var spine = root.querySelector('[data-dna-spine]');
    var pathEl = root.querySelector('[data-dna-path]');
    var particle = root.querySelector('[data-dna-particle]');
    var halo = root.querySelector('[data-dna-halo]');
    var nodesWrap = root.querySelector('[data-dna-nodes]');
    var switchBtns = Array.prototype.slice.call(root.querySelectorAll('[data-dna-switch]'));

    var activeIndex = 0;
    var pathLength = 0;
    var nodeMarks = [];
    var particleRaf = null;
    var particleRunning = false;
    var drawn = false;
    var litKeys = {};

    /* ----------------------------- helpers -------------------------------- */
    function esc(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;')
            .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function fallbackImage() {
        return '/images/browser-window.png';
    }

    /* --------------------------- path building ----------------------------- */
    function buildPath() {
        if (!helix || !pathEl || !spine || !nodesWrap) return;

        var hb = helix.getBoundingClientRect();
        var w = hb.width;
        var h = hb.height;
        if (w < 10 || h < 10) return;

        spine.setAttribute('viewBox', '0 0 ' + w + ' ' + h);
        spine.setAttribute('width', String(w));
        spine.setAttribute('height', String(h));

        var nodes = Array.prototype.slice.call(nodesWrap.querySelectorAll('.dna-node'));
        if (!nodes.length) return;

        var firstDot = nodes[0].querySelector('.dna-node-dot');
        var spineX;
        if (firstDot) {
            var fd = firstDot.getBoundingClientRect();
            spineX = fd.left - hb.left + fd.width / 2;
        } else {
            spineX = w / 2;
        }

        var pad = 8;
        var ys = nodes.map(function (n) {
            var d = n.querySelector('.dna-node-dot');
            if (!d) return 0;
            var r = d.getBoundingClientRect();
            return r.top - hb.top + r.height / 2;
        });

        var startY = Math.max(pad, ys[0] - 14);
        var endY = Math.min(h - pad, ys[ys.length - 1] + 14);

        var d = 'M ' + spineX + ' ' + startY;
        var amp = Math.min(14, w * 0.03);
        for (var i = 0; i < ys.length; i++) {
            var y = ys[i];
            var prevY = i === 0 ? startY : ys[i - 1];
            var midY = (prevY + y) / 2;
            var dir = i % 2 === 0 ? 1 : -1;
            d += ' Q ' + (spineX + amp * dir) + ' ' + midY + ' ' + spineX + ' ' + y;
        }
        d += ' L ' + spineX + ' ' + endY;

        pathEl.setAttribute('d', d);
        pathLength = pathEl.getTotalLength();
        pathEl.style.strokeDasharray = String(pathLength);

        if (!drawn) {
            pathEl.style.strokeDashoffset = String(pathLength);
        } else {
            pathEl.style.strokeDashoffset = '0';
        }

        nodeMarks = nodes.map(function (n) {
            var d2 = n.querySelector('.dna-node-dot');
            var y = 0;
            if (d2) {
                var r = d2.getBoundingClientRect();
                y = r.top - hb.top + r.height / 2;
            }
            return { el: n, length: lengthAtY(y) };
        });
    }

    function lengthAtY(targetY) {
        if (!pathLength || !pathEl) return 0;
        var lo = 0;
        var hi = pathLength;
        for (var i = 0; i < 24; i++) {
            var mid = (lo + hi) / 2;
            var pt = pathEl.getPointAtLength(mid);
            if (pt.y < targetY) lo = mid;
            else hi = mid;
        }
        return (lo + hi) / 2;
    }

    /* ------------------------- particle animation --------------------------- */
    function stopParticle() {
        particleRunning = false;
        if (particleRaf) cancelAnimationFrame(particleRaf);
        particleRaf = null;
        if (particle) particle.setAttribute('opacity', '0');
        if (halo) halo.setAttribute('opacity', '0');
    }

    function startParticle() {
        if (reduceMotion || !pathEl || !pathLength) return;
        stopParticle();
        particleRunning = true;
        litKeys = {};

        var duration = Math.max(4200, Math.min(7000, pathLength * 4.2));
        var start = null;
        var prevT = 0;

        function frame(ts) {
            if (!particleRunning) return;
            if (start === null) start = ts;
            var elapsed = ts - start;
            var t = (elapsed % duration) / duration;

            // wrapped → clear pulse states so nodes can re-light next pass
            if (t < prevT) {
                litKeys = {};
                nodeMarks.forEach(function (m) {
                    m.el.classList.remove('is-pulse');
                });
            }
            prevT = t;

            var len = t * pathLength;
            var pt = pathEl.getPointAtLength(len);

            if (particle) {
                particle.setAttribute('cx', String(pt.x));
                particle.setAttribute('cy', String(pt.y));
                particle.setAttribute('opacity', '1');
            }
            if (halo) {
                halo.setAttribute('cx', String(pt.x));
                halo.setAttribute('cy', String(pt.y));
                halo.setAttribute('opacity', '1');
            }

            for (var i = 0; i < nodeMarks.length; i++) {
                if (litKeys[i]) continue;
                if (len >= nodeMarks[i].length) {
                    litKeys[i] = true;
                    var el = nodeMarks[i].el;
                    el.classList.add('is-lit', 'is-pulse');
                    (function (node) {
                        setTimeout(function () { node.classList.remove('is-pulse'); }, 950);
                    })(el);
                }
            }

            particleRaf = requestAnimationFrame(frame);
        }
        particleRaf = requestAnimationFrame(frame);
    }

    /* --------------------------- reveal sequence ---------------------------- */
    function playIntro() {
        if (drawn) return;
        drawn = true;

        if (reduceMotion) {
            root.classList.add('is-drawn', 'is-revealed');
            if (pathEl) pathEl.style.strokeDashoffset = '0';
            nodeMarks.forEach(function (m) { m.el.classList.add('is-lit'); });
            return;
        }

        buildPath();
        requestAnimationFrame(function () {
            root.classList.add('is-drawn');
            if (pathEl) pathEl.style.strokeDashoffset = '0';
        });

        setTimeout(function () {
            root.classList.add('is-revealed');
        }, 700);

        setTimeout(function () {
            startParticle();
        }, 1600);
    }

    /* ----------------------------- project switch --------------------------- */
    function renderNodes(project) {
        if (!nodesWrap) return;
        nodesWrap.innerHTML = (project.nodes || []).map(function (n) {
            var techs = (n.technologies || []).join(' • ');
            return '' +
                '<div class="dna-node" data-side="' + esc(n.side) + '" data-key="' + esc(n.key) + '" tabindex="0"' +
                ' aria-label="' + esc(n.label) + ': ' + esc(techs) + '">' +
                '<span class="dna-node-branch" aria-hidden="true"></span>' +
                '<span class="dna-node-dot" aria-hidden="true"><span class="dna-node-dot-core"></span></span>' +
                '<div class="dna-node-card">' +
                    '<span class="dna-node-icon"><i class="' + esc(n.icon) + '"></i></span>' +
                    '<span class="dna-node-label">' + esc(n.label) + '</span>' +
                    '<span class="dna-node-peek">' + esc(techs) + '</span>' +
                    '<div class="dna-node-panel" role="tooltip">' +
                        '<strong>' + esc(n.label) + '</strong>' +
                        '<p>' + esc(techs) + '</p>' +
                    '</div>' +
                '</div>' +
            '</div>';
        }).join('');
    }

    function switchProject(index) {
        if (index === activeIndex || !projects[index]) return;
        activeIndex = index;
        var p = projects[index];

        stopParticle();

        switchBtns.forEach(function (btn, i) {
            var on = i === index;
            btn.classList.toggle('is-active', on);
            btn.setAttribute('aria-selected', String(on));
        });

        var img = root.querySelector('[data-dna-image]');
        if (img) {
            img.classList.add('is-swapping');
            setTimeout(function () {
                img.src = p.imageUrl || fallbackImage();
                img.alt = p.title + ' — project preview';
                img.classList.remove('is-swapping');
            }, 220);
        }

        function setText(sel, val) {
            var el = root.querySelector(sel);
            if (el) el.textContent = val;
        }
        setText('[data-dna-title]', p.title);
        setText('[data-dna-desc]', p.shortDescription);
        setText('[data-dna-url]', 'ze.dev/' + p.slug);
        setText('[data-dna-slug]', '/' + p.slug);

        var caseLink = root.querySelector('[data-dna-case]');
        if (caseLink) caseLink.setAttribute('href', '/projects/' + p.slug);

        var live = root.querySelector('[data-dna-live]');
        if (live) {
            if (p.liveUrl) { live.setAttribute('href', p.liveUrl); live.hidden = false; }
            else live.hidden = true;
        }
        var gh = root.querySelector('[data-dna-github]');
        if (gh) {
            if (p.githubUrl) { gh.setAttribute('href', p.githubUrl); gh.hidden = false; }
            else gh.hidden = true;
        }

        renderNodes(p);
        requestAnimationFrame(function () {
            buildPath();
            if (drawn) {
                if (pathEl) pathEl.style.strokeDashoffset = '0';
                var nodes = nodesWrap.querySelectorAll('.dna-node');
                Array.prototype.forEach.call(nodes, function (n, i) {
                    setTimeout(function () { n.classList.add('is-lit'); }, 120 * i);
                });
                setTimeout(startParticle, 120 * nodes.length + 300);
            }
        });
    }

    switchBtns.forEach(function (btn) {
        btn.addEventListener('click', function () {
            switchProject(parseInt(btn.getAttribute('data-dna-switch'), 10) || 0);
        });
    });

    /* --------------------- touch: tap node to toggle panel ------------------ */
    if (nodesWrap) {
        nodesWrap.addEventListener('click', function (e) {
            var node = e.target.closest ? e.target.closest('.dna-node') : null;
            if (!node) return;
            var open = node.classList.contains('is-open');
            Array.prototype.forEach.call(nodesWrap.querySelectorAll('.dna-node.is-open'), function (n) {
                n.classList.remove('is-open');
            });
            if (!open) node.classList.add('is-open');
        });
    }

    document.addEventListener('click', function (e) {
        if (e.target.closest && e.target.closest('.dna-node')) return;
        if (!nodesWrap) return;
        Array.prototype.forEach.call(nodesWrap.querySelectorAll('.dna-node.is-open'), function (n) {
            n.classList.remove('is-open');
        });
    });

    /* ------------------------- viewport trigger ----------------------------- */
    if (reduceMotion) {
        playIntro();
    } else if ('IntersectionObserver' in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    playIntro();
                    io.unobserve(entry.target);
                }
            });
        }, { threshold: 0.22, rootMargin: '0px 0px -60px 0px' });
        io.observe(root);
    } else {
        playIntro();
    }

    /* --------------------------- resize handling ---------------------------- */
    var resizeTimer = null;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            var wasDrawn = drawn;
            buildPath();
            if (wasDrawn && pathEl) pathEl.style.strokeDashoffset = '0';
        }, 140);
    });

    document.addEventListener('visibilitychange', function () {
        if (document.hidden) stopParticle();
        else if (drawn && !reduceMotion) startParticle();
    });

    if (document.readyState === 'complete') buildPath();
    else window.addEventListener('load', buildPath);
})();
