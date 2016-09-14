(function() {

    angular.module('App').directive('updateDirective', updateDirective);
 
    function updateDirective () {
        var directive = {
            restrict: 'E',
            templateUrl: "/Scripts/ngApp/monitor/template/update.html",
            scope: {
                component: "="
            },
            controller: function ($scope) {

                }
        }
        console.log('in update directive');
        return directive;
        };
    })();