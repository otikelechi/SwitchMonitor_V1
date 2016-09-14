(function() {
    "use strict";


    function chartDirective($window) {
        // Usage:
        //     <chartDirective></chartDirective>
        // Creates:
        // 
        var directive = {
            link: link,
            restrict: "EA",
            template: '<div class="chartcontainer"><canvas class="doughnut" height="300"></canvas></div>',
            scope: {
                upnodes: "=",
                nodeLength: "="
            }
        };
        return directive;

        function link(scope, element, attrs) {
            var doughnut = $(element).find(".doughnut")[0];
            scope.$watch("upnodes", function(newVal, oldVal) {
                console.log(scope.doughnut);
                scope.doughnut.segments[0].value = isNaN(newVal) ? 50 : newVal;
                scope.doughnut.segments[1].value = isNaN(newVal) ? 50 : scope.nodeLength - newVal;
                scope.doughnut.update();
                scope.doughnut.render(1000, true);
            });

            var doughnutData = [
                {
                    value: 50,
                    color: themeprimary
                },
                {
                    value: 50,
                    color: themesecondary
                }
            ];
            scope.doughnut = new Chart(doughnut.getContext("2d")).Doughnut(doughnutData);
        }
    }

    angular.module("App").directive("chartDirective", chartDirective);

})();