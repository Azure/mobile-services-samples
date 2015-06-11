'use strict';
angular.module('todoApp')
.controller('homeCtrl', ['$scope', '$location', 'Azureservice', function ($scope, $location, Azureservice) {
    
    $scope.userInfo = Azureservice.getCurrentUser();
    $scope.isAuthenticated = $scope.userInfo !== null; 
    $scope.loginWithProvider = function (selectedProvider) {
        Azureservice.login(selectedProvider).then(function(){
            $scope.userInfo = Azureservice.getCurrentUser();
            $scope.isAuthenticated = $scope.userInfo !== null;
        });
    };
    
    $scope.logout = function () {
        Azureservice.logout();
        $scope.isAuthenticated = false;
    };
    $scope.isActive = function (viewLocation) {
        return viewLocation === $location.path();
    };
}]);