document.addEventListener('DOMContentLoaded', () => {
    const carousel = document.querySelector('.carousel') as HTMLElement;
    const btnPrevious = document.querySelector('.btn-previous') as HTMLElement;
    const btnNext = document.querySelector('.btn--next') as HTMLElement;

    if (!carousel || !btnPrevious || !btnNext) return;

    const cards = carousel.querySelectorAll('.container-card');
    if (cards.length === 0) return;

    let currentIndex = 0;
    let isAnimating = false;

    // Initialize carousel position
    updateCarouselPosition(false);

    btnPrevious.addEventListener('click', () => {
        if (isAnimating) return;
        navigateCarousel('previous');
    });

    btnNext.addEventListener('click', () => {
        if (isAnimating) return;
        navigateCarousel('next');
    });

    function navigateCarousel(direction: 'previous' | 'next') {
        isAnimating = true;

        if (direction === 'previous') {
            currentIndex = (currentIndex - 1 + cards.length) % cards.length;
        } else {
            currentIndex = (currentIndex + 1) % cards.length;
        }

        updateCarouselPosition(true);

        setTimeout(() => {
            isAnimating = false;
        }, 500);
    }

    function updateCarouselPosition(animate: boolean) {
        if (!animate) {
            carousel.style.transition = 'none';
        } else {
            carousel.style.transition = 'transform 0.5s ease';
        }

        carousel.style.transform = `translateX(-${currentIndex * 100}%)`;

        if (!animate) {
            // Force reflow to make sure the 'none' transition is applied
            carousel.offsetHeight;
        }
    }
});
