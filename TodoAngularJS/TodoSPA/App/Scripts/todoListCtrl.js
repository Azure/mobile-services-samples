'use strict';
angular.module('todoApp')
.controller('todoListCtrl', ['$scope', '$location', 'Azureservice', function ($scope, $location, Azureservice){
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
            $scope.editInProgressTodo.ID = todo.id;
            $scope.editingInProgress = true;
        } else {
            $scope.editingInProgress = false;
        }
    };

    $scope.populate = function () {
        Azureservice.getAll(tableName).then(function (results) {
            $scope.todoList = results;
            $scope.loadingMessage = "";
        }).catch(function (err){
            $scope.loadingMessage = err.toString();
        });
    };
    $scope.delete = function (item) {
         Azureservice.del(tableName, item).then(function (results) {
            $scope.populate();
        }).catch(function (err){
            $scope.loadingMessage = err.toString();
        });
    };
    $scope.update = function (todo) {
        Azureservice.update(tableName, { 
            id: $scope.editInProgressTodo.ID,
            description: $scope.editInProgressTodo.Description 
        }).then(function (results) {
            $scope.populate();
            $scope.editSwitch(todo);
        }).catch(function (err){
            $scope.loadingMessage = err.toString();
        });;
    };
    $scope.add = function () {
        Azureservice.insert(tableName, {
            'description': $scope.newTodoCaption,
        }).then(function (results) {
            $scope.loadingMsg = "";
            $scope.newTodoCaption = "";
            $scope.populate();
        }).catch(function (err){
            $scope.loadingMessage = err.toString();
        });;
    };
}]);