'use strict';
angular.module('todoApp')
.controller('homeCtrl', ['$scope', '$location', 'Azureservice', function ($scope, $location, Azureservice) {
//    debugger;
    $scope.login = function () {
        Azureservice.login('aad');
    };
    $scope.logout = function () {
        Azureservice.logOut();
    };
    $scope.isActive = function (viewLocation) {
        return viewLocation === $location.path();
    };
}]);