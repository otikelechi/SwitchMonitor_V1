(function() {
    "use strict";


    angular.module("App").controller("monitorController", monitorController);
    monitorController.$inject = ["$scope", "$rootScope", "monitorService", "$interval", "allUpdates"];

    function monitorController($scope, $rootScope, monitorService, $interval, allUpdates) {
        monitorService.initialize();
        $scope.filter = "none";
        $scope.$on("hubConnected", function(e) {
            console.log("hubConnected event received");
            monitorService.getAllNodes();
        });

        $scope.$on("allNodes", function(e, nodes, updates, notUsed, statusMessage) {
            console.log("all nodes received");
            console.log("status received is " + statusMessage);
            $scope.nodes = nodes;
            console.log(nodes);
            $scope.updates = updates;
            debugger;
            if (statusMessage == null) {
                $scope.IsServerConnected = true;
                $scope.message = statusMessage;
                $scope.$apply();
            } else {
                $scope.IsServerConnected = false;
                $scope.message = statusMessage;
                $scope.$apply();
            }
            console.log("all updates received");
            $scope.notUsed = notUsed;
            //JSON.parse(nodes);
            console.log(updates + " occured @ " + new Date());
            var upCount = 0;
            for (var i = 0; i <= nodes.length - 1; i++) {

                if (nodes[i].IsConnected) {
                    upCount++;
                }
            }
            $scope.upNodes = upCount;

            $scope.$apply();
        });

        $interval(function() {
            $scope.date = new Date();
            //  $scope.$apply;
        }, 1000);

        $scope.changeState = function(filter) {
            if (filter === "none") {
                $scope.filter = "down";
                document.getElementById("btnShow").innerText = "Show All";
            } else {
                $scope.filter = "none";
                document.getElementById("btnShow").innerText = "Show Down Only";
            }
        };

        $scope.allUpdates = function(updates) {
            allUpdates.show(updates);
        };
    }
})();