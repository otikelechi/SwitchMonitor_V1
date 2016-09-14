(function () {

    function monitorService($rootScope, $timeout, $interval) {

        var switchMonitor = null;
        var interval = 1;
        var _initialize = function () {
            console.log("in service");

            switchMonitor = $.connection.switchMonitorHub;

            //start hub
            $.connection.hub.logging = true;
            $timeout(function () {
                $.connection.hub.start().done(function () {
                    console.log("server connected");
                    $rootScope.$broadcast('hubConnected');
                });
            }, 200);
            $.connection.hub.connectionSlow(function () {
                console.log('We are currently experiencing difficulties with the connection.');
            });
            $.connection.hub.error(function (error) {
                console.log('SignalR error: ' + error + ' occured @ ' + new Date());
            });
            $.connection.hub.disconnected(function () {
                $timeout(function () {
                    $.connection.hub.start().done(function () {
                        console.log("server connected");
                        $rootScope.$broadcast('hubConnected');
                    });
                }, 200);
            });

            //client recieves all nodes
            switchMonitor.client.getAllNodes = function (nodes, updates,notUsed,statusMessage) {
                var nodeJson = JSON.parse(nodes);
                var updatesJson = JSON.parse(updates);
                var notUsedJson = JSON.parse(notUsed);
                $rootScope.$broadcast('allNodes', nodeJson, updatesJson,notUsedJson,statusMessage);
            }

            $rootScope.$on('allNodes', function (e, nodes, updates) {
                $timeout(function () { _getAllNodes(); }, interval);
            });
            
        }

        var _getAllNodes = function () {
            console.log('getting nodes');
            switchMonitor.server.getAllNodes().fail(function (error) {
                console.log('Switch Monitor error: ' + error + ' occured @ ' + new Date());
            });
        }


        return {
            initialize: _initialize,
            getAllNodes: _getAllNodes
        };
       
    }
    angular.module('App').factory('monitorService', monitorService);
})();