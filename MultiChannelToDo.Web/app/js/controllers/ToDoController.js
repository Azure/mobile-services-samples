'use strict';
multiChannelToDoApp
    .controller('ToDoController', ['$scope', 'toDoService', function ($scope, toDoService) {

    toDoService.getItems().success(function (data) {
        $scope.items = data;
    });

    $scope.add = function (text) {
        toDoService.add(text);
    };

    $scope.complete = function (id) {
        toDoService.complete(id)
    };

}]);