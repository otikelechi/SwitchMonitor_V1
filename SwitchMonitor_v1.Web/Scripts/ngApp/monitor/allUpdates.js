(function () {

    angular.module('App').factory('allUpdates', allUpdates);


    function allUpdates($modal) {
        var openModal = function (component) {
            var options = {
                templateUrl: '/Scripts/ngApp/monitor/template/allUpdates.html',
                windowClass: 'modal fade',
                backdrop: true,
                resolve: {
                    component: function () {
                        return component;
                    }
                },
                controller: function ($scope, component) {
                    $scope.component = component;
                }
            };
            $modal.open(options);
        };

        return {
            show: openModal
        };
    }
}
)();
