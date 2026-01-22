// Fix footer position
function fixFooterPosition() {
    const body = document.body;
    const html = document.documentElement;
    const footer = document.querySelector('footer');

    if (!footer) return;

    const bodyHeight = Math.max(
        body.scrollHeight,
        body.offsetHeight,
        html.clientHeight,
        html.scrollHeight,
        html.offsetHeight
    );

    const windowHeight = window.innerHeight;

    if (bodyHeight < windowHeight) {
        footer.classList.add('fixed-bottom');
    } else {
        footer.classList.remove('fixed-bottom');
        footer.style.position = 'relative';
    }
}

// Run on page load and resize
document.addEventListener('DOMContentLoaded', fixFooterPosition);
window.addEventListener('resize', fixFooterPosition);