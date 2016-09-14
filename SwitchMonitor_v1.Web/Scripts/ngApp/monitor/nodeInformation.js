(function() {

        console.log("hello info");
        angular.module("App").factory("nodeInformation", nodeInformation);


        function nodeInformation($modal) {
            var openModal = function(component) {
                var options = {
                    templateUrl: "/Scripts/ngApp/monitor/template/nodeInfo.html",
                    windowClass: "modal fade",
                    backdrop: true,
                    resolve: {
                        component: function() {
                            return component;
                        }
                    },
                    controller: function($scope, component) {
                        $scope.component = component;
                    }
                };
                $modal.open(options);
            };

            return {
                show: openModal
            };
        }

        console.log("in factory");
        // function getData() { }
    }
)();