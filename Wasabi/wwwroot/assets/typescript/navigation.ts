// Update in navigation.ts
document.getElementById('hamburger-menu').addEventListener('click', function () {
    document.getElementById('mobile-nav').classList.toggle('active');
    document.getElementById('hamburger-menu').classList.toggle('active');

    // Toggle body scroll prevention
    document.body.classList.toggle('nav-open');
});