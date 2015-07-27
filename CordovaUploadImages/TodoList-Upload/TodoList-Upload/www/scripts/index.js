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
        var todoItemTable = client.getTable('zumoTable');
        var isSrcCamera = true;

        // Read current data and rebuild UI.
        // If you plan to generate complex UIs like this, consider using a JavaScript templating library.
        function refreshTodoItems() {
            $('#summary').html("Loading...");
            var query = todoItemTable; //.where({ complete: false });

            // Execute the query and then generate the array list.
            query.read().then(function (todoItems) {
                var listItems = $.map(todoItems, function (item) {
                    var listItem = $('<li>')
                        .attr('data-todoitem-id', item.id)
                        .append($('<button class="item-delete">Delete</button>'))
                        .append($('<input type="checkbox" class="item-complete">').prop('checked', item.complete))
                        .append($('<div>').append($('<input class="item-text">').val(item.text)));

                    // Only add the image if the URL exists.
                    if (item.imageUri) {
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
            $('#output-text').html("error: " + JSON.string(error));
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

            // Get the URL of the image on the local device.
            var localFileSytemUrl = capturedFile.fullPath;
            if (device.platform == 'iOS') {
                // We need the file:/ prefix on an iOS device.
                localFileSytemUrl = "file://" + localFileSytemUrl;
            }

            if (device.platform == 'Android' && capturedFile.nativeURL) {
                // On Android, File Picker needed the nativeURL.
                localFileSytemUrl = capturedFile.nativeURL;
            }

            window.resolveLocalFileSystemURL(localFileSytemUrl, function (fileEntry) {
                fileEntry.file(function (file) {
                    // We need a FileReader to read the captured file.
                    var reader = new FileReader();
                    reader.onloadend = readCompleted;
                    reader.onerror = fail;

                    // Read the captured file into a byte array.
                    // This function is not currently supported on Windows Phone.
                    reader.readAsArrayBuffer(file);
                }, fail);
            }, function fileError(error) {
                console.debug("Unable to resolve file URL: " + error, "app");
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

                $('#output-text').html("Upload complete.");

                // Refresh the UI with the latest image.
                refreshTodoItems();
            }
            else {
                if (r.currentTarget.status === 403) {
                    // alert("You may need to use a unique filename.");
                    $('#output-text').html("403. On WinPhone, you might need a unique filename...");
                }
                else {
                    $('#output-text').html("An error occurred during upload.");
                }
            }
        }

        // Function that handles general errors.
        function fail(err) {
            $('#output-text').html("An error has occurred: " + JSON.stringify(err));
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
            }, handleError).then(function () {
                $('#output-text').html("Table updated...");
            }, handleError);
        }

        // Handle insert--this replaces the existing handler.
        $('#add-item').submit(function (evt) {
            var textbox = $('#new-item-text'),
                itemText = textbox.val();
            if (itemText !== '') {

                var newItem = { text: itemText, complete: false };
                // Do the capture before we do the insert. If user cancels, just continue.
                // Launch device camera application, allowing user to capture a single image. 
                var srcType;

                if (isSrcCamera == false) {
                    srcType = Camera.PictureSourceType.SAVEDPHOTOALBUM;
                }
                else {
                    srcType = Camera.PictureSourceType.CAMERA;
                }

                // For WinPhone, we need to use
                // our own unique filename due to bug in FileReader.
                if (device.platform == "windows") {
                    storeFilename(itemText);
                }
                // Use the Camera plugin instead of MediaCapture plugin to
                // add support for using the FilePicker.
                navigator.camera.getPicture(function cameraSuccess(imageUri) {

                    window.resolveLocalFileSystemURL(imageUri, function success(entry) {

                        // Set a reference to the captured file.
                        var capturedFile = entry;
                        console.debug("capturedFile object: " + JSON.stringify(capturedFile));
                        // The container name in Azure will be created
                        // if it's not already present.
                        newItem.containerName = "pngimages";
                        // Set the properties we need on the inserted item, using the device UUID
                        // to avoid collisions on the server with images from other devices.
                        newItem.resourceName = device.uuid.concat("-", capturedFile.name);
                        insertNewItemWithUpload(newItem, capturedFile);

                    }, function fail(entry) {
                        console.log("no file: " + entry.name);
                    });

                }, function cameraError(error) {
                    insertNewItemWithUpload(newItem, null);
                    console.debug("Unable to obtain picture: " + error, "app");

                }, {
                    quality: 50,
                    destinationType: Camera.DestinationType.FILE_URI,
                    // destinationType: Camera.DestinationType.DATA_URL,
                    // Dynamically set the picture source
                    sourceType: srcType,
                    //sourceType: Camera.PictureSourceType.CAMERA,
                    encodingType: Camera.EncodingType.JPEG,
                    mediaType: Camera.MediaType.PICTURE,
                    allowEdit: true,
                    correctOrientation: true  //Corrects Android orientation quirks
                });
            }

            textbox.val('').focus();
            evt.preventDefault();
        });
        /******************************************************
        // End upload images to Azure Mobile Services additions.
        *********************************************************/

        // readAsArrayBuffer() does not support the default
        // filenames generated by Windows Phone Camera app.
        // We can, however, provide a unique filename to work around this.
        function storeFilename(fileName) {
            window.winphone81FileName = fileName;
        }

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

        // Handle changing the picture source.
        $('#img-source').on('click', function (evt) {
            isSrcCamera = $(this).prop('checked');
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
