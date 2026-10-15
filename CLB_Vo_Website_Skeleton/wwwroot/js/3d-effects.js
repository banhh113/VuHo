// ============================================================
// CLB VŨ HỔ — 3D EFFECTS (B + C + D + E)
// Vanilla JS — không thư viện ngoài
// Tự tắt trên mobile + prefers-reduced-motion
// ============================================================

(function () {
    'use strict';

    const supportsHover = window.matchMedia('(hover: hover) and (pointer: fine)').matches;
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    if (!supportsHover || prefersReducedMotion) {
        console.log('%c[3D Effects] Bỏ qua — mobile hoặc reduced-motion', 'color:#888');
        return;
    }

    console.log('%c[3D Effects] Đã kích hoạt', 'color:#f5c518;font-weight:bold');

    function rafThrottle(fn) {
        let ticking = false;
        return function (...args) {
            if (!ticking) {
                window.requestAnimationFrame(() => {
                    fn.apply(this, args);
                    ticking = false;
                });
                ticking = true;
            }
        };
    }

    // ========== TILT 3D ==========
    function attachTilt(el, options = {}) {
        if (!el) return;

        const maxTilt = options.maxTilt ?? 8;
        const scale = options.scale ?? 1.02;
        const perspective = options.perspective ?? 1000;
        const speed = options.speed ?? 0.35;
        const glareOpacity = options.glareOpacity ?? 0.15;

        el.style.transformStyle = 'preserve-3d';
        el.style.willChange = 'transform';

        let targetRotX = 0, targetRotY = 0;
        let currentRotX = 0, currentRotY = 0;
        let targetScale = 1, currentScale = 1;
        let rafId = null;
        let isHovering = false;

        let glare = el.querySelector('.tilt-glare');
        if (!glare && glareOpacity > 0) {
            glare = document.createElement('div');
            glare.className = 'tilt-glare';
            glare.style.cssText = `
                position: absolute;
                inset: 0;
                border-radius: inherit;
                pointer-events: none;
                opacity: 0;
                transition: opacity .3s ease;
                background: radial-gradient(circle at var(--glare-x, 50%) var(--glare-y, 50%),
                            rgba(255,255,255,${glareOpacity}) 0%, transparent 60%);
                z-index: 5;
            `;
            if (getComputedStyle(el).position === 'static') el.style.position = 'relative';
            el.appendChild(glare);
        }

        function tick() {
            currentRotX += (targetRotX - currentRotX) * speed;
            currentRotY += (targetRotY - currentRotY) * speed;
            currentScale += (targetScale - currentScale) * speed;

            el.style.transform = `perspective(${perspective}px) rotateX(${currentRotX.toFixed(2)}deg) rotateY(${currentRotY.toFixed(2)}deg) scale(${currentScale.toFixed(3)})`;

            const done = Math.abs(targetRotX - currentRotX) < 0.05 &&
                         Math.abs(targetRotY - currentRotY) < 0.05 &&
                         Math.abs(targetScale - currentScale) < 0.001;

            if (done && !isHovering) {
                el.style.transform = '';
                el.classList.remove('tilting');
                if (glare) glare.style.opacity = '0';
                rafId = null;
            } else {
                rafId = requestAnimationFrame(tick);
            }
        }

        function start() { if (!rafId) rafId = requestAnimationFrame(tick); }

        function onEnter() {
            isHovering = true;
            el.classList.add('tilting');
            targetScale = scale;
            if (glare) glare.style.opacity = '1';
            start();
        }

        function onMove(e) {
            const rect = el.getBoundingClientRect();
            const px = (e.clientX - rect.left) / rect.width;
            const py = (e.clientY - rect.top) / rect.height;

            targetRotY = (px - 0.5) * maxTilt * 2;
            targetRotX = -(py - 0.5) * maxTilt * 2;

            if (glare) {
                glare.style.setProperty('--glare-x', (px * 100) + '%');
                glare.style.setProperty('--glare-y', (py * 100) + '%');
            }
            start();
        }

        function onLeave() {
            isHovering = false;
            targetRotX = 0;
            targetRotY = 0;
            targetScale = 1;
            start();
        }

        el.addEventListener('mouseenter', onEnter);
        el.addEventListener('mousemove', rafThrottle(onMove));
        el.addEventListener('mouseleave', onLeave);
    }

    // ========== SPOTLIGHT ==========
    function attachSpotlight(el) {
        if (!el) return;
        el.classList.add('spotlight');
        const onMove = (e) => {
            const rect = el.getBoundingClientRect();
            el.style.setProperty('--mouse-x', ((e.clientX - rect.left) / rect.width * 100) + '%');
            el.style.setProperty('--mouse-y', ((e.clientY - rect.top) / rect.height * 100) + '%');
        };
        el.addEventListener('mousemove', rafThrottle(onMove));
    }

    // ========== MAGNETIC ==========
    function attachMagnetic(el, options = {}) {
        if (!el) return;
        const strength = options.strength ?? 0.25;
        const onMove = (e) => {
            const rect = el.getBoundingClientRect();
            const x = e.clientX - (rect.left + rect.width / 2);
            const y = e.clientY - (rect.top + rect.height / 2);
            el.style.transform = `translate(${(x * strength).toFixed(1)}px, ${(y * strength).toFixed(1)}px)`;
            el.style.transition = 'transform .1s ease-out';
        };
        const onLeave = () => {
            el.style.transform = '';
            el.style.transition = 'transform .4s cubic-bezier(.22, 1, .36, 1)';
        };
        el.addEventListener('mousemove', rafThrottle(onMove));
        el.addEventListener('mouseleave', onLeave);
    }

    // ========== STATS 3D REVEAL ==========
    function initStats3D() {
        const items = document.querySelectorAll('.stats > div');
        if (!items.length) return;
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry, i) => {
                if (entry.isIntersecting) {
                    setTimeout(() => entry.target.classList.add('stats-3d-enter'), i * 120);
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.3 });
        items.forEach(el => observer.observe(el));
    }

    // ============================================================
    // APPLY ALL
    // ============================================================
    function init() {
        // PHASE B — Hero + Why
        const heroMedia = document.querySelector('.hero-media');
        if (heroMedia) {
            attachTilt(heroMedia, { maxTilt: 6, scale: 1.03, perspective: 1200, speed: 0.28, glareOpacity: 0 });
        }

        document.querySelectorAll('.why-card').forEach((card) => {
            attachTilt(card, { maxTilt: 10, scale: 1.03, perspective: 900, speed: 0.3, glareOpacity: 0.08 });
            attachSpotlight(card);
        });

        const heroCta = document.querySelector('.hero-actions .btn-primary');
        if (heroCta) attachMagnetic(heroCta, { strength: 0.15 });

        // PHASE C — Sport + Member + Tournament
        document.querySelectorAll('.sport-card').forEach((card) => {
            attachTilt(card, { maxTilt: 8, scale: 1.02, perspective: 1000, speed: 0.3, glareOpacity: 0 });
        });
        document.querySelectorAll('.member-link').forEach((link) => {
            attachTilt(link, { maxTilt: 6, scale: 1.02, perspective: 1200, speed: 0.25, glareOpacity: 0 });
        });
        document.querySelectorAll('.tournament-card').forEach((card) => {
            attachTilt(card, { maxTilt: 6, scale: 1.02, perspective: 1200, speed: 0.3, glareOpacity: 0.05 });
        });
        document.querySelectorAll('.top-achievement-card').forEach((card) => {
            attachTilt(card, { maxTilt: 4, scale: 1.01, perspective: 1400, speed: 0.3, glareOpacity: 0 });
        });

        // PHASE D — Stats + Gallery + Contact
        initStats3D();

        document.querySelectorAll('.gallery-card').forEach((card) => {
            attachTilt(card, { maxTilt: 8, scale: 1.03, perspective: 1000, speed: 0.3, glareOpacity: 0.1 });
        });
        document.querySelectorAll('.gallery-hero-img').forEach((el) => {
            attachTilt(el, { maxTilt: 5, scale: 1.02, perspective: 1400, speed: 0.28, glareOpacity: 0 });
        });
        document.querySelectorAll('.contact-card').forEach((card) => {
            attachMagnetic(card, { strength: 0.1 });
        });
        document.querySelectorAll('.contact-cta .btn, .schedule-col .btn-dark, .center-actions .btn').forEach((btn) => {
            attachMagnetic(btn, { strength: 0.15 });
        });

        // PHASE E — Admin
        document.querySelectorAll('.admin-body .stat-card').forEach((card) => {
            attachTilt(card, { maxTilt: 4, scale: 1.01, perspective: 1500, speed: 0.35, glareOpacity: 0 });
        });
        document.querySelectorAll('.admin-body .quick-action').forEach((el) => {
            attachTilt(el, { maxTilt: 3, scale: 1.01, perspective: 1500, speed: 0.4, glareOpacity: 0 });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    window.CLBVo3D = { attachTilt, attachSpotlight, attachMagnetic };
})();