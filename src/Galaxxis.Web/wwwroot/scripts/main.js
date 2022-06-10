
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

//# sourceMappingURL=main.js.map
