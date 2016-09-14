(function () {
    'use strict';

    angular
        .module('App')
        .controller('monitorController', monitorController);

    controller.$inject = ['$scope']; 

    function monitorController($scope) {
        $scope.title = 'monitorController';

        activate();

        function activate() { }
    }
})();
