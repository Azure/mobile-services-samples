'use strict';
var apiPath = "https://donnam-multichannel.azurewebsites.net";
multiChannelToDoApp
    .factory('toDoService', ['$http', function ($http) {
        return {

            getItems: function () {
                return $http.get(apiPath + '/api/ToDoItems');
            },

            add: function (id, task) {
                return $http.post(apiPath + '/api/ToDoItems', { "Id": id + 1, "Text": task, "Complete": false });
            },

            complete: function (item) {
                return $http.put(apiPath + '/api/ToDoItems/' + item.Id, { "Id": item.Id, "Text": item.Text, "Complete": true });
            }
        }
    }]);