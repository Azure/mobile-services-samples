(function () {
    "use strict";

    document.addEventListener('deviceready', onDeviceReady.bind(this), false);

    function onDeviceReady() {
        // Handle the Cordova pause and resume events
        document.addEventListener('pause', onPause.bind(this), false);
        document.addEventListener('resume', onResume.bind(this), false);

        // Initialize the Mobile Services client here.
        var client = new WindowsAzure.MobileServiceClient(
            'https://MY_MOBILE_SERVICE.azure-mobile.net/',
            'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX');

        // Get the existing TodoItem table for storage.
        var todoItemTable = client.getTable('TodoItem');

        // Read current data and rebuild UI.
        // If you plan to generate complex UIs like this, consider using a JavaScript templating library.
        function refreshTodoItems() {
            $('#summary').html("Loading...");
            var query = todoItemTable;//.where({ complete: false });

            // Execute the query and then generate the array list.
            query.read().then(function (todoItems) {
                var listItems = $.map(todoItems, function (item) {
                    var listItem = $('<li>')
                        .attr('data-todoitem-id', item.id)
                        .append($('<button class="item-delete">Delete</button>'))
                        .append($('<input type="checkbox" class="item-complete">').prop('checked', item.complete))
                        .append($('<div>').append($('<input class="item-text">').val(item.text)));

                    // Only add the image if the URL exists.
                    if (item.imageUri)
                    {                      
                        listItem.append($('<img>').attr('src', item.imageUri));
                    }
                    return listItem;
                });

                $('#todo-items').empty().append(listItems).toggle(listItems.length > 0);
                $('#summary').html('<strong>' + todoItems.length + '</strong> item(s)');

                var width = $('#todo-items').width();

                $('#todo-items img').css({
                    'max-width': width, 'height': 'auto'
                });

            }, handleError);
        }

        function handleError(error) {
            alert("error: " + JSON.string(error));
            var text = error + (error.request ? ' - ' + error.request.status : '');
            $('#errorlog').append($('<li>').text(text));
        }

        function getTodoItemId(formElement) {
            return $(formElement).closest('li').attr('data-todoitem-id');
        }

        /******************************************************
        // Begin upload images to Azure Mobile Services additions.
        *********************************************************/
        // This is the new item being inserted.
        var insertedItem;

        // This function is called to get the newly captured image
        // file and read it into an array buffer. 
        function readImage(capturedFile) {
            window.resolveLocalFileSystemURL("file://" + capturedFile.fullPath, function (fileEntry) {
                fileEntry.file(function (file) {
                    // We need a FileReader to read the captured file.
                    var reader = new FileReader();
                    reader.onloadend = readCompleted;
                    reader.onerror = fail;

                    // Read the captured file into a byte array.
                    // This function is not currently supported on Windows Phone.
                    reader.readAsArrayBuffer(file);
                }, fail);
            });
        }

        // This function gets called when the reader is done loading the image
        // and it is sent via an XMLHttpRequest to the Azure Storage Blob service.
        var readCompleted = function (evt) {
            if (evt.target.readyState == FileReader.DONE) {

                // The binary data is the result.
                var requestData = evt.target.result;

                // Build the request URI with the SAS, which gives us permissions to upload.
                var uriWithAccess = insertedItem.imageUri + "?" + insertedItem.sasQueryString;
                var xhr = new XMLHttpRequest();
                xhr.onerror = fail;
                xhr.onloadend = uploadCompleted;
                xhr.open("PUT", uriWithAccess);
                xhr.setRequestHeader('x-ms-blob-type', 'BlockBlob');
                xhr.setRequestHeader('x-ms-blob-content-type', 'image/jpeg');
                xhr.send(requestData);
            }
        }

        // This function is called when the XMLHttpRequest has a response.
        var uploadCompleted = function (r) {
            // Response code is 201 (Created) if success.
            if (r.currentTarget.status === 201) {

                console.debug("Upload complete.");

                // Refresh the UI with the latest image.
                refreshTodoItems();
            }
            else {
                alert("An error occurred during upload.");
            }
        }

        // Function that handles general errors.
        function fail(err) {
            alert("An error has occurred: " + JSON.stringify(err));
        }

        // Insert a new item, then also upload a captured image if we have one.
        var insertNewItemWithUpload = function (newItem, capturedFile) {
            // Do the insert so that we can get the SAS query string from Blob storage.
            todoItemTable.insert(newItem).then(function (item) {
                // If we have obtained an SAS, then upload the image to Blob storage.
                if (item.sasQueryString !== undefined) {

                    insertedItem = item;
                    readImage(capturedFile);
                }
            }, handleError).then(refreshTodoItems, handleError);
        }

        // Handle insert--this replaces the existing handler.
        $('#add-item').submit(function (evt) {
            var textbox = $('#new-item-text'),
                itemText = textbox.val();
            if (itemText !== '') {

                var newItem = { text: itemText, complete: false };
                // Do the capture before we do the insert. If user cancels, just continue.
                // Launch device camera application, allowing user to capture a single image.                
                navigator.device.capture.captureImage(function (mediaFiles) {
                    if (mediaFiles) {
                        // Set a reference to the captured file.
                        var capturedFile = mediaFiles[0];
                        console.debug("capturedFile object: " + JSON.stringify(capturedFile));

                        // Set the properties we need on the inserted item.
                        newItem.containerName = "todoitemimages";
                        newItem.resourceName = capturedFile.name;

                        // Insert the item and upload the blob.
                        insertNewItemWithUpload(newItem, capturedFile);
                    }

                }, function () {
                    // Insert the item but not the blob.
                    insertNewItemWithUpload(newItem, null);
                }, { limit: 1 });
            }
            textbox.val('').focus();
            evt.preventDefault();
        });
        /******************************************************
        // End upload images to Azure Mobile Services additions.
        *********************************************************/

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
        refreshTodoItems();

    };

    function onPause() {
        // TODO: This application has been suspended. Save application state here.
    };

    function onResume() {
        // TODO: This application has been reactivated. Restore application state here.
    };
})();