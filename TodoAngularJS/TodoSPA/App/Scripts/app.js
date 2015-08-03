'use strict';
angular.module('todoApp', ['ngRoute', 'azure-mobile-service.module']).constant('AzureMobileServiceClient', {
    API_URL : 'yourmobileservice',
    API_KEY : 'appkey',
  })
.config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
    
    $routeProvider.when("/Login", {
        controller: "loginCtrl",
        templateUrl: "/App/Views/Login.html",
    }).when("/Home", {
        controller: "homeCtrl",
        templateUrl: "/App/Views/Home.html",
    }).when("/TodoList", {
        controller: "todoListCtrl",
        templateUrl: "/App/Views/TodoList.html",
        requireADLogin: true,
    }).when("/UserData", {
        controller: "userDataCtrl",
        templateUrl: "/App/Views/UserData.html",
    }).otherwise({ redirectTo: "/Home" });  
}]);
