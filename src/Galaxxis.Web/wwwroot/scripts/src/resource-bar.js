/* #region Resource bar: live counting ------------------------------------ */
// The server renders every resource amount as of page load, together with its net change per second
// (data-amount, data-rate-per-second; see Views/Shared/_ResourceBar.cshtml). This script counts on from
// there, so the bar shows the current amounts without reloading the page.
// It is a display estimate only: amounts never fall below zero, but the throttling that starts when a
// supply resource runs out is not modelled. The next page load shows the exact values again.
(() => {
    const formatter = new Intl.NumberFormat('de-DE', { maximumFractionDigits: 0 });

    function startResourceBar() {
        const values = document.querySelectorAll('[data-rate-per-second]');
        if (values.length === 0) {
            return;
        }

        const loadedAt = performance.now();
        setInterval(() => {
            const seconds = (performance.now() - loadedAt) / 1000;
            values.forEach(element => {
                const amount = parseFloat(element.dataset.amount) + parseFloat(element.dataset.ratePerSecond) * seconds;
                element.textContent = formatter.format(Math.floor(Math.max(0, amount)));
            });
        }, 1000);
    }

    document.addEventListener('DOMContentLoaded', startResourceBar);
})();
/* #endregion -------------------------------------------------------------- */
