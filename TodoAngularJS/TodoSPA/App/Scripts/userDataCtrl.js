'use strict';
angular.module('todoApp')
.controller('userDataCtrl', ['$scope','Azureservice',  function ($scope, Azureservice) {
	$scope.userInfo = Azureservice.getCurrentUser();
}]);