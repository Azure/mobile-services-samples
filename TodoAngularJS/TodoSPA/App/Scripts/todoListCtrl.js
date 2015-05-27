'use strict';
angular.module('todoApp')
.controller('todoListCtrl', ['$scope', '$location', 'todoListSvc', 'Azureservice', function ($scope, $location, todoListSvc, Azureservice){
    var tableName = "todoItem";
    
    $scope.error = "";
    $scope.loadingMessage = "Loading...";
    $scope.todoList = null;
    $scope.editingInProgress = false;
    $scope.newTodoCaption = "";
    $scope.isAuthenticated = false;

    $scope.editInProgressTodo = {
        Description: "",
        ID: 0
    };

    $scope.editSwitch = function (todo) {
        todo.edit = !todo.edit;
        if (todo.edit) {
            $scope.editInProgressTodo.Description = todo.Description;
            $scope.editInProgressTodo.ID = todo.ID;
            $scope.editingInProgress = true;
        } else {
            $scope.editingInProgress = false;
        }
    };

    $scope.populate = function () {
        Azureservice.getAll(tableName).then(function (results) {
            $scope.todoList = results;
            $scope.loadingMessage = "";
        });
    };
    $scope.delete = function (item) {
         Azureservice.del(tableName, item).then(function (results) {
            $scope.populate();
        });
    };
    $scope.update = function (todo) {
         Azureservice.update(tableName, todo).then(function (results) {
            $scope.populate();
            $scope.editSwitch(todo);
        });
    };
    $scope.add = function () {
        todoListSvc.putItem({
            'Description': $scope.newTodoCaption,
        }).then(function (results) {
            $scope.loadingMsg = "";
            $scope.newTodoCaption = "";
            $scope.populate();
        }).error(function (err) {
            $scope.error = err;
            $scope.loadingMsg = "";
        })
    };
}]);