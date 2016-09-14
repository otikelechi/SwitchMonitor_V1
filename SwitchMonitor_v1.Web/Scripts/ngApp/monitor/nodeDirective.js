(function() {

    function nodeDirective() {
        var directive = {
            restrict: "EA",
            replace: true,
            templateUrl: "/Scripts/ngApp/monitor/template/node.html",
            scope: {
                component: "="
            },
            controller: function($scope, nodeInformation) {

                $scope.showDetail = function() {
                    console.log($scope.component);
                    nodeInformation.show($scope.component);
                };
            }
        };
        return directive;


    }

    console.log("in directive");
    angular.module("App").directive("nodeDirective", nodeDirective);
})();