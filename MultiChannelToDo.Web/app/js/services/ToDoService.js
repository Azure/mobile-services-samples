'use strict';
multiChannelToDoApp
    .factory('toDoService', ['$http', function ($http) {
        return {

            getItems: function () {
                return $http.get('https://multichannel-saas.azurewebsites.net/api/ToDoItems');
            },

            add: function (id, task) {
                return $http.post('https://multichannel-saas.azurewebsites.net/api/ToDoItems', { "Id": id + 1, "Text": task, "Complete": false });
            },

            complete: function (item) {
                return $http.put('https://multichannel-saas.azurewebsites.net/api/ToDoItems/' + item.Id, { "Id": item.Id, "Text": item.Text, "Complete": true });
            }
        }
    }]);