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
