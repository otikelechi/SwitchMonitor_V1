(function() {
    'use strict';

    angular
        .module('App')
        .directive('nodeDirective', nodeDirective);

    nodeDirective.$inject = ['$window'];
    
    function nodeDirective($window) {
        // Usage:
        //     <nodeController></nodeController>
        // Creates:
        // 
        var directive = {
            link: link,
            restrict: 'EA',
            replace: true,
            templateUrl: "Scripts/ngApp/monitor/template/node.html",
            scope: {
                controller: "="
            }
        };
        return directive;

        function link(scope, element, attrs) {
        }
    }

})();