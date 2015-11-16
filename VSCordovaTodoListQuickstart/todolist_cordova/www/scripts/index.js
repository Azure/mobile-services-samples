// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkID=397704
// To debug code on page load in Ripple or on Android devices/emulators: launch your app, set breakpoints, 
// and then run "window.location.reload()" in the JavaScript Console.
(function () {
    "use strict";

    document.addEventListener( 'deviceready', onDeviceReady.bind( this ), false );

    function onDeviceReady() {
        // Handle the Cordova pause and resume events
        document.addEventListener( 'pause', onPause.bind( this ), false );
        document.addEventListener( 'resume', onResume.bind( this ), false );
        
        var client = new WindowsAzure.MobileServiceClient('https://todolist-cordova.azure-mobile.net/', 'ZPBJZlWeNQAMBTzibQiPHAHIVpeSut47'),
        todoItemTable = client.getTable('todoitem');

        // Read current data and rebuild UI.
        // If you plan to generate complex UIs like this, consider using a JavaScript templating library.
        function refreshTodoItems() {
            $('#summary').html("Loading...");
            var query = todoItemTable.where({ complete: false });

            query.read().then(function (todoItems) {
                var listItems = $.map(todoItems, function (item) {
                    return $('<li>')
                        .attr('data-todoitem-id', item.id)
                        .append($('<button class="item-delete">Delete</button>'))
                        .append($('<input type="checkbox" class="item-complete">').prop('checked', item.complete))
                        .append($('<div>').append($('<input class="item-text">').val(item.text)));
                });

                $('#todo-items').empty().append(listItems).toggle(listItems.length > 0);
                $('#summary').html('<strong>' + todoItems.length + '</strong> item(s)');
            }, handleError);
        }

        function handleError(error) {
            var text = error + (error.request ? ' - ' + error.request.status : '');
            $('#errorlog').append($('<li>').text(text));
        }

        function getTodoItemId(formElement) {
            return $(formElement).closest('li').attr('data-todoitem-id');
        }

        // Handle insert
        $('#add-item').submit(function (evt) {
            var textbox = $('#new-item-text'),
                itemText = textbox.val();
            if (itemText !== '') {
                todoItemTable.insert({ text: itemText, complete: false }).then(refreshTodoItems, handleError);
            }
            textbox.val('').focus();
            evt.preventDefault();
        });

        $('#refresh').click(function (evt) {
            refreshTodoItems();
            evt.preventDefault();
        });

        // Handle update
        $(document.body).on('change', '.item-text', function () {
            var newText = $(this).val();
            todoItemTable.update({ id: getTodoItemId(this), text: newText }).then(null, handleError);
        });

        $(document.body).on('change', '.item-complete', function () {
            var isComplete = $(this).prop('checked');
            todoItemTable.update({ id: getTodoItemId(this), complete: isComplete }).then(refreshTodoItems, handleError);
        });

        // Handle delete
        $(document.body).on('click', '.item-delete', function () {
            todoItemTable.del({ id: getTodoItemId(this) }).then(refreshTodoItems, handleError);
        });

        // On initial load, start by fetching the current data
        function refreshAuthDisplay() {
            var isLoggedIn = client.currentUser !== null;
            $("#logged-in").toggle(isLoggedIn);
            $("#logged-out").toggle(!isLoggedIn);

            if (isLoggedIn) {
                $("#login-name").text(client.currentUser.userId);
                refreshTodoItems();
            }
        }

        function logIn() {
            client.login("google").then(refreshAuthDisplay, function (error) {
                alert(error);
            });
        }

        function logOut() {
            client.logout();
            refreshAuthDisplay();
            $('#summary').html('<strong>You must sign in to access data.</strong>');
        }

        function refreshAuthDisplay() {
            var isLoggedIn = client.currentUser !== null;
            $("#logged-in").toggle(isLoggedIn);
            $("#logged-out").toggle(!isLoggedIn);

            if (isLoggedIn) {
                $("#login-name").text(client.currentUser.userId);
                refreshTodoItems();
            }
        }

        function logIn() {
            client.login("facebook").then(refreshAuthDisplay, function (error) {
                alert(error);
            });
        }

        function logOut() {
            client.logout();
            refreshAuthDisplay();
            $('#summary').html('<strong>You must login to access data.</strong>');
        }

        // On page init, fetch the data and set up event handlers
        $(function () {
            refreshAuthDisplay();
            $('#summary').html('<strong>You must login to access data.</strong>');
            $("#logged-out button").click(logIn);
            $("#logged-in button").click(logOut);
        });
    };

    function onPause() {
        // TODO: This application has been suspended. Save application state here.
    };

    function onResume() {
        // TODO: This application has been reactivated. Restore application state here.
    };
} )();