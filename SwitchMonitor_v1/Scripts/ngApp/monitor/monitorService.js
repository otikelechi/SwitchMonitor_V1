(function () {
    'use strict';

    angular
        .module('App')
        .factory('monitorService', monitorService);

    monitorService.$inject = ['$http'];

    function monitorService($http) {
        var service = {
            getData: getData
        };

        return service;

        function getData() { }
    }
})();