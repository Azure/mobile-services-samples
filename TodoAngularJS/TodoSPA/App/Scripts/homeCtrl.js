'use strict';
angular.module('todoApp')
.controller('homeCtrl', ['$scope', '$location', 'Azureservice', function ($scope, $location, Azureservice) {
//    debugger;
    $scope.login = function () {
        Azureservice.login('aad');
        $scope.isAuthenticated = true;
    };
    $scope.logout = function () {
        Azureservice.logout();
        $scope.isAuthenticated = false;
    };
    $scope.isActive = function (viewLocation) {
        return viewLocation === $location.path();
    };
}]);