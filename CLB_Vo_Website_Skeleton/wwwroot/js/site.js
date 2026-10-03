// ============================================================
// CLB VŨ HỔ — Site Scripts
// ============================================================

console.log('%c🐯 CLB Vũ Hổ loaded', 'color:#f5c518;font-size:14px;font-weight:bold');

// ==================== 1. Header scroll effect ====================
const header = document.querySelector('.site-header');
if (header) {
    const onScroll = () => {
        header.classList.toggle('scrolled', window.scrollY > 60);
    };
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
}

// ==================== 2. Reveal on scroll ====================
const revealEls = document.querySelectorAll('.section, .card, .event-item, .member-card, .gallery-item, .stats > div');
revealEls.forEach(el => el.classList.add('reveal'));

const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry, i) => {
        if (entry.isIntersecting) {
            setTimeout(() => entry.target.classList.add('active'), i * 60);
            observer.unobserve(entry.target);
        }
    });
}, { threshold: 0.12, rootMargin: '0px 0px -80px 0px' });

revealEls.forEach(el => observer.observe(el));

// ==================== 3. Counter animation ====================
function animateCounter(el, target, duration = 1600) {
    let start = 0;
    const step = target / (duration / 16);
    const update = () => {
        start += step;
        if (start < target) {
            el.textContent = Math.floor(start);
            requestAnimationFrame(update);
        } else {
            el.textContent = target;
        }
    };
    requestAnimationFrame(update);
}

const statObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
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
                    const step = num / 100;
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
        }
    });
}, { threshold: 0.5 });

document.querySelectorAll('.stats > div').forEach(el => statObserver.observe(el));

// ==================== 4. Smooth scroll for hash links ====================
document.querySelectorAll('a[href*="#"]').forEach(link => {
    link.addEventListener('click', (e) => {
        const href = link.getAttribute('href');
        const hashIndex = href.indexOf('#');
        if (hashIndex === -1) return;

        const path = href.slice(0, hashIndex) || '/';
        const hash = href.slice(hashIndex);

        // Nếu click cùng trang
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

// ==================== 5. Confirm delete (admin) ====================
document.querySelectorAll('form[data-confirm]').forEach(form => {
    form.addEventListener('submit', (e) => {
        if (!confirm(form.dataset.confirm)) e.preventDefault();
    });
});