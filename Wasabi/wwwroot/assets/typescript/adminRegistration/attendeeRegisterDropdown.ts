document.addEventListener('DOMContentLoaded', function () {
    const dropdowns = document.querySelectorAll<HTMLElement>('.attendance-dropdown');

    dropdowns.forEach(dropdown => {
        const toggle = dropdown.querySelector<HTMLElement>('.attendance-dropdown__toggle');
        const menu = dropdown.querySelector<HTMLElement>('.attendance-dropdown__menu');

        if (!toggle || !menu) return;

        // Toggle dropdown when clicking the toggle button
        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            // Close all other dropdowns first
            document.querySelectorAll<HTMLElement>('.attendance-dropdown__menu').forEach(m => {
                if (m !== menu) m.style.display = 'none';
            });

            // Toggle current dropdown
            menu.style.display = menu.style.display === 'block' ? 'none' : 'block';
        });
    });

    // Close dropdowns when clicking outside
    document.addEventListener('click', function (e) {
        const target = e.target as HTMLElement;
        if (!target.closest('.attendance-dropdown')) {
            document.querySelectorAll<HTMLElement>('.attendance-dropdown__menu').forEach(menu => {
                menu.style.display = 'none';
            });
        }
    });
});

