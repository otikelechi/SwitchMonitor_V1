(function() {
    "use strict";

    angular
        .module("app")
        .factory("updateMonitorService", updateMonitorService);

    updateMonitorService.$inject = ["$http"];

    function updateMonitorService($http) {
        var service = {
            getData: getData
        };

        return service;

        function getData() {}
    }
})();