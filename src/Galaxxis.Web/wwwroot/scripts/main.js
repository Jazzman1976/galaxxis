/* #region Player commands --------------------------------------------------- */
// Forms marked with data-command-form send a player command (e.g. "upgrade the ore mine") without
// leaving the page. A command is only recorded by the server; the engine decides on it within about a
// second. So the script:
//   1. posts the form (including its antiforgery token) and reads the JSON answer,
//   2. shows an immediate refusal (e.g. not enough resources) right away,
//   3. otherwise asks the status URL once per second until the engine has decided,
//   4. shows a rejection, or reloads the page to show the accepted command.
// Messages appear in the element marked with data-command-message.
// Without JavaScript the forms still work as plain posts (the server then redirects back with a message).
(() => {
    const pollIntervalMs = 1000;
    const maxPolls = 30;
    const jsonHeaders = { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' };

    const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

    function showMessage(text) {
        const message = document.querySelector('[data-command-message]');
        if (message) {
            message.textContent = text;
        }
    }

    function setButtonsEnabled(form, enabled) {
        form.querySelectorAll('button').forEach(button => button.disabled = !enabled);
    }

    async function waitForDecision(statusUrl) {
        for (let poll = 0; poll < maxPolls; poll++) {
            await delay(pollIntervalMs);
            const response = await fetch(statusUrl, { headers: jsonHeaders });
            if (!response.ok) {
                continue;
            }

            const status = await response.json();
            if (status.isDecided) {
                return status;
            }
        }

        return null;
    }

    async function sendCommand(form) {
        setButtonsEnabled(form, false);
        showMessage('Befehl wird übermittelt …');

        try {
            const response = await fetch(form.action, { method: 'POST', body: new FormData(form), headers: jsonHeaders });
            if (!response.ok) {
                showMessage('Der Befehl konnte nicht übermittelt werden. Bitte lade die Seite neu.');
                return;
            }

            const result = await response.json();
            if (result.outcome !== 'Recorded') {
                showMessage(result.message || 'Der Befehl wurde abgelehnt.');
                return;
            }

            showMessage('Befehl erteilt, warte auf Bestätigung …');
            const status = await waitForDecision(result.statusUrl);

            if (status === null) {
                showMessage('Der Befehl ist erteilt, die Bestätigung dauert aber länger als üblich (ist das Universum pausiert?).');
                return;
            }

            if (status.state === 'Rejected' || status.state === 'Failed') {
                showMessage('Abgelehnt: ' + (status.error || 'unbekannter Grund'));
                return;
            }

            window.location.reload();
        } catch (error) {
            showMessage('Der Befehl konnte nicht übermittelt werden: ' + error);
        } finally {
            setButtonsEnabled(form, true);
        }
    }

    document.addEventListener('submit', event => {
        const form = event.target.closest('form[data-command-form]');
        if (!form) {
            return;
        }

        event.preventDefault();
        sendCommand(form);
    });

    // Countdowns (Views/Shared/_Countdown.cshtml) reload the page shortly after they end, when the
    // engine has completed the order.
    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-reload-after-seconds]').forEach(element => {
            const seconds = parseFloat(element.dataset.reloadAfterSeconds);
            setTimeout(() => window.location.reload(), (seconds + 2) * 1000);
        });
    });
})();
/* #endregion -------------------------------------------------------------- */


const content = document.getElementById('content');

/* #region Timer Uhren ----------------------------------------------------- */
function StarteUhren() {
    const uhren = content.querySelectorAll('.clock');
    for (var i = 0; i < uhren.length; i++) {
        var uhr = uhren[i];
        var stunden = uhr.dataset.stunden;
        var minuten = uhr.dataset.minuten;
        var sekunden = uhr.dataset.sekunden;
        uhr.innerHTML = 
            '<span>' + ('0' + stunden).slice(-2) + ':</span>' +
            '<span>' + ('0' + minuten).slice(-2) + ':</span>' +
            '<span>' + ('0' + sekunden).slice(-2) + '</span>';
    }
    const tick = setInterval(() => {
        for (var i = 0; i < uhren.length; i++) {
            var uhr = uhren[i];
            var stunden = uhr.dataset.stunden;
            var minuten = uhr.dataset.minuten;
            var sekunden = uhr.dataset.sekunden;
            if (stunden + minuten + sekunden == 0) {
                continue;
            }
            sekunden = sekunden - 1;
            if (sekunden < 0) {
                 minuten = minuten - 1;
                 sekunden = 59;
            }
            if (minuten < 0) {
                stunden = stunden - 1;
                minuten = 59;
            }
            if (stunden < 0) {
                stunden = 0;
                minuten = 0;
                sekunden = 0;
            }
            uhr.innerHTML = 
                '<span>' + ('0' + stunden).slice(-2) + ':</span>' +
                '<span>' + ('0' + minuten).slice(-2) + ':</span>' +
                '<span>' + ('0' + sekunden).slice(-2) + '</span>';
            uhr.dataset.stunden = stunden;
            uhr.dataset.minuten = minuten;
            uhr.dataset.sekunden = sekunden;
        }        
    }, 1000);
}
/* #endregion -------------------------------------------------------------- */

/* #region on document ready ----------------------------------------------- */
// on document ready
document.addEventListener("DOMContentLoaded", function(event) { 
    const container = document.querySelector('#layer-interface');
    const ps = new PerfectScrollbar(container);
    StarteUhren();    
});
/* #endregion */

/* #region click handler --------------------------------------------------- */
// click event handler
content.addEventListener('click', function(event) {

    // prev button click
    if(event.target.classList.contains('location-selection__prev')) {        
        var selectId = event.target.dataset.for;
        if(selectId) {            
            var options = document.getElementById(selectId).querySelectorAll('option');
            for(var j = 0; j < options.length; j++) {
                if (options[j].selected) {
                    if (j-1 < 0) {
                        options[options.length-1].selected = true;                    
                    } else {
                        options[j-1].selected = true;
                    }
                    break;
                }
            }
        }
    }

    // next button click
    if(event.target.classList.contains('location-selection__next')) {
        var selectId = event.target.dataset.for;
        if(selectId) {            
            var options = document.getElementById(selectId).querySelectorAll('option');
            for(var j = 0; j < options.length; j++) {
                if (options[j].selected) {
                    if (options.length > j+1) {
                        options[j+1].selected = true;                    
                    } else {
                        options[0].selected = true;
                    }
                    break;
                }
            }
        }
    }
});
/* #endregion */

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

//# sourceMappingURL=main.js.map
