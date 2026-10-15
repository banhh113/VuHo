// ============================================================
// CLB VŨ HỔ — HERO CINEMATIC
// Particle system + light beams + staggered text reveal
// ============================================================

(function () {
    'use strict';

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const isMobile = window.matchMedia('(hover: none), (max-width: 640px)').matches;

    // ============================================================
    // 1. PARTICLE SYSTEM (Canvas 2D)
    // ============================================================
    function initParticles() {
        const canvas = document.getElementById('heroParticles');
        if (!canvas) return;

        if (prefersReducedMotion) {
            canvas.style.display = 'none';
            return;
        }

        const ctx = canvas.getContext('2d');
        if (!ctx) return;

        // Cấu hình
        const COUNT = isMobile ? 12 : 30;
        const COLORS = [
            { r: 230, g: 57, b: 70, a: 0.55 },   // đỏ
            { r: 245, g: 197, b: 24, a: 0.45 },  // vàng
            { r: 255, g: 255, b: 255, a: 0.3 }   // trắng
        ];

        let W = 0, H = 0, DPR = 1;
        let particles = [];
        let rafId = null;
        let isVisible = true;

        function resize() {
            const rect = canvas.getBoundingClientRect();
            DPR = Math.min(window.devicePixelRatio || 1, 2);
            W = rect.width;
            H = rect.height;
            canvas.width = W * DPR;
            canvas.height = H * DPR;
            ctx.scale(DPR, DPR);
        }

        function makeParticle(initial = false) {
            const color = COLORS[Math.floor(Math.random() * COLORS.length)];
            return {
                x: Math.random() * W,
                y: initial ? Math.random() * H : H + 20,
                size: 1 + Math.random() * 2.5,
                speedY: -(0.15 + Math.random() * 0.5),
                speedX: (Math.random() - 0.5) * 0.3,
                drift: Math.random() * Math.PI * 2,
                driftSpeed: 0.005 + Math.random() * 0.015,
                opacity: 0,
                targetOpacity: color.a,
                fadeIn: true,
                color,
                blur: Math.random() > 0.7 ? 3 : 1
            };
        }

        function init() {
            particles = [];
            for (let i = 0; i < COUNT; i++) {
                particles.push(makeParticle(true));
                particles[i].opacity = particles[i].targetOpacity; // không fade in lần đầu
            }
        }

        function draw() {
            ctx.clearRect(0, 0, W, H);

            for (const p of particles) {
                // Cập nhật vị trí
                p.drift += p.driftSpeed;
                p.x += p.speedX + Math.sin(p.drift) * 0.3;
                p.y += p.speedY;

                // Fade
                if (p.fadeIn) {
                    p.opacity += 0.01;
                    if (p.opacity >= p.targetOpacity) {
                        p.opacity = p.targetOpacity;
                        p.fadeIn = false;
                    }
                } else if (p.y < H * 0.15) {
                    // Bắt đầu fade out khi gần đỉnh
                    p.opacity -= 0.005;
                }

                // Vẽ
                const c = p.color;
                ctx.beginPath();
                const gradient = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, p.size * 3);
                gradient.addColorStop(0, `rgba(${c.r}, ${c.g}, ${c.b}, ${p.opacity})`);
                gradient.addColorStop(1, `rgba(${c.r}, ${c.g}, ${c.b}, 0)`);
                ctx.fillStyle = gradient;
                ctx.arc(p.x, p.y, p.size * 3, 0, Math.PI * 2);
                ctx.fill();

                // Recycle khi ra khỏi màn hình
                if (p.y < -20 || p.opacity <= 0) {
                    Object.assign(p, makeParticle(false));
                }
            }
        }

        function loop() {
            if (!isVisible) {
                rafId = null;
                return;
            }
            draw();
            rafId = requestAnimationFrame(loop);
        }

        function start() {
            if (!rafId) rafId = requestAnimationFrame(loop);
        }

        function stop() {
            if (rafId) {
                cancelAnimationFrame(rafId);
                rafId = null;
            }
        }

        // Init
        resize();
        init();
        start();

        // Resize
        let resizeTimer;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(() => {
                ctx.setTransform(1, 0, 0, 1, 0, 0);
                resize();
                init();
            }, 200);
        });

        // Chỉ chạy khi hero trong viewport
        if ('IntersectionObserver' in window) {
            const io = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    isVisible = entry.isIntersecting;
                    if (isVisible) start();
                    else stop();
                });
            }, { threshold: 0 });
            io.observe(canvas);
        }
    }

    // ============================================================
    // 2. STAGGERED TEXT REVEAL
    // ============================================================
    function initTextReveal() {
        const items = document.querySelectorAll('.hero-reveal');
        if (!items.length) return;

        if (prefersReducedMotion || !('IntersectionObserver' in window)) {
            items.forEach(el => el.classList.add('hero-revealed'));
            return;
        }

        const io = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('hero-revealed');
                    io.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });

        items.forEach(el => io.observe(el));
    }

    // ============================================================
    // 3. TIGER BREATHING (subtle scale pulse)
    // ============================================================
    function initTigerBreathing() {
        const tiger = document.querySelector('.hero-media img');
        if (!tiger || prefersReducedMotion) return;

        // Đã có float animation từ CSS, thêm breathing riêng
        tiger.style.transformOrigin = 'center bottom';
    }

    // ============================================================
    // 4. PARALLAX LIGHT BEAMS
    // ============================================================
    function initBeamParallax() {
        const beams = document.querySelectorAll('.hero-light-beam');
        if (!beams.length || prefersReducedMotion) return;

        let ticking = false;
        function onScroll() {
            if (!ticking) {
                window.requestAnimationFrame(() => {
                    const y = window.scrollY;
                    beams.forEach((beam, i) => {
                        const speed = 0.15 + i * 0.08;
                        beam.style.transform = `translateY(${y * speed}px)`;
                    });
                    ticking = false;
                });
                ticking = true;
            }
        }

        window.addEventListener('scroll', onScroll, { passive: true });
    }

    // ============================================================
    // INIT
    // ============================================================
    function init() {
        initParticles();
        initTextReveal();
        initTigerBreathing();
        initBeamParallax();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();