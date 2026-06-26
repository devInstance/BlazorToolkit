// Connectivity bridge for BlazorToolkit offline support.
// Shipped as _content/DevInstance.BlazorToolkit/js/blazortoolkit-connectivity.js.
// Wires window online/offline events to a .NET object reference so managed code
// learns about connectivity changes. Called via IJSRuntime as
// window.blazortoolkit.connectivity.*
(function () {
    "use strict";
    var dotNetRef = null;

    function notify() {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync("OnConnectivityChangedFromJs", navigator.onLine);
        }
    }

    function initialize(ref) {
        dotNetRef = ref;
        window.addEventListener("online", notify);
        window.addEventListener("offline", notify);
        return navigator.onLine;
    }

    window.blazortoolkit = window.blazortoolkit || {};
    window.blazortoolkit.connectivity = {
        initialize: initialize
    };
})();
