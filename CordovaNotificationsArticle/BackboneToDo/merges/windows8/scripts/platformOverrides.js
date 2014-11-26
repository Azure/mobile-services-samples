(function () {
    var scriptElem = document.createElement('script');
    scriptElem.setAttribute('src', 'scripts/safeHtmlPolyfill.js');
    if (document.body) {
    	document.body.appendChild(scriptElem);
    } else {
    	document.head.appendChild(scriptElem);
    }
}());