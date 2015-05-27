'use strict';
angular.module('todoApp')
.factory('todoListSvc', ['$http', 'AzureMobileServiceClient', 'Azureservice', function ($http, AzureMobileServiceClient, Azureservice) {
    
    var tableName = 'TodoItem';
    
    return {
        authenticate: function(){
            
            
        },
        getItems : function(){
            return Azureservice.getAll(tableName);
        },
        getItem : function(id){
            return $http.get('/api/TodoList/' + id);
        },
        postItem : function(item){
            debugger;
            return Azureservice.insert(tableName, item);
        },
        putItem : function(item){
            debugger;
            return Azureservice.insert(tableName, item);
        },
        deleteItem : function(item){
           return Azureservice.del(tableName, item);
        }
    };
}]);