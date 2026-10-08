// ============================================================
// CLB VŨ HỔ — Site Scripts
// ============================================================

console.log('%c🐯 CLB Vũ Hổ loaded', 'color:#e63946;font-size:14px;font-weight:bold');

// ==================== 1. Header scroll effect ====================
(function () {
    const header = document.querySelector('.site-header');
    const scrollTop = document.getElementById('scrollTop');
    if (!header) return;

    const onScroll = () => {
        const y = window.scrollY;
        header.classList.toggle('scrolled', y > 40);
        if (scrollTop) scrollTop.classList.toggle('visible', y > 400);
    };

    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
})();

// ==================== 2. Mobile menu toggle ====================
(function () {
    const toggle = document.getElementById('menuToggle');
    const menu = document.getElementById('mobileMenu');
    if (!toggle || !menu) return;

    toggle.addEventListener('click', () => {
        toggle.classList.toggle('active');
        menu.classList.toggle('open');
    });

    // Đóng khi click link
    menu.querySelectorAll('a').forEach(a => {
        a.addEventListener('click', () => {
            toggle.classList.remove('active');
            menu.classList.remove('open');
        });
    });
})();

// ==================== 3. Scroll to top ====================
(function () {
    const btn = document.getElementById('scrollTop');
    if (!btn) return;
    btn.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
})();

// ==================== 4. Reveal on scroll ====================
(function () {
    const els = document.querySelectorAll('.reveal');
    if (!els.length || !('IntersectionObserver' in window)) {
        els.forEach(el => el.classList.add('active'));
        return;
    }

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, i) => {
            if (entry.isIntersecting) {
                setTimeout(() => entry.target.classList.add('active'), i * 60);
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12, rootMargin: '0px 0px -60px 0px' });

    els.forEach(el => observer.observe(el));
})();

// ==================== 5. Counter animation ====================
(function () {
    if (!('IntersectionObserver' in window)) return;

    const statObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (!entry.isIntersecting) return;
            const strong = entry.target.querySelector('strong');
            if (strong && !strong.dataset.animated) {
                const text = strong.textContent;
                const match = text.match(/(\d+)/);
                if (match) {
                    const num = parseInt(match[1], 10);
                    const prefix = text.slice(0, text.indexOf(match[1]));
                    const suffix = text.slice(text.indexOf(match[1]) + match[1].length);
                    strong.dataset.animated = '1';
                    strong.textContent = prefix + '0' + suffix;
                    let start = 0;
                    const step = num / 60;
                    const tick = () => {
                        start += step;
                        if (start < num) {
                            strong.textContent = prefix + Math.floor(start) + suffix;
                            requestAnimationFrame(tick);
                        } else {
                            strong.textContent = prefix + num + suffix;
                        }
                    };
                    requestAnimationFrame(tick);
                }
            }
            statObserver.unobserve(entry.target);
        });
    }, { threshold: 0.5 });

    document.querySelectorAll('.stats > div').forEach(el => statObserver.observe(el));
})();

// ==================== 6. Smooth scroll for hash links ====================
(function () {
    document.querySelectorAll('a[href*="#"]').forEach(link => {
        link.addEventListener('click', (e) => {
            const href = link.getAttribute('href');
            if (!href) return;
            const hashIndex = href.indexOf('#');
            if (hashIndex === -1) return;

            const path = href.slice(0, hashIndex) || '/';
            const hash = href.slice(hashIndex);

            if (path === '/' || path === '' || path === window.location.pathname) {
                const target = document.querySelector(hash);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    history.replaceState(null, '', hash);
                }
            }
        });
    });
})();

// ==================== 7. Admin sidebar active state ====================
(function () {
    const items = document.querySelectorAll('.admin-nav-item');
    if (!items.length) return;

    const path = window.location.pathname.toLowerCase().replace(/\/$/, '') || '/';
    items.forEach(i => i.classList.remove('active'));

    let bestMatch = null;
    let bestLen = 0;

    items.forEach(item => {
        const href = item.getAttribute('href');
        if (!href || href === '#') return;

        const clean = href.toLowerCase().replace(/\/$/, '') || '/';
        const isMatch = path === clean || path.startsWith(clean + '/');

        if (isMatch && clean.length > bestLen) {
            bestMatch = item;
            bestLen = clean.length;
        }
    });

    if (bestMatch) bestMatch.classList.add('active');
})();

// ==================== 8. GLightbox ====================
document.addEventListener('DOMContentLoaded', function () {
    if (typeof GLightbox !== 'undefined') {
        GLightbox({
            selector: '.glightbox',
            touchNavigation: true,
            loop: true,
            zoomable: true,
            draggable: true,
            openEffect: 'zoom',
            closeEffect: 'fade',
            slideEffect: 'slide',
            moreLength: 60
        });
    }
});

// ==================== 9. Confirm delete (admin) ====================
document.querySelectorAll('form[data-confirm]').forEach(form => {
    form.addEventListener('submit', (e) => {
        if (!confirm(form.dataset.confirm)) e.preventDefault();
    });
});