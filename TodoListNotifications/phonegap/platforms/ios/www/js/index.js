/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
 
// Set these global variables to the settings for your
// Azure Mobile Service. 
var MOBILE_SERVICE_URL = '<mobile_service_url>';
var MOBILE_SERVICE_APP_KEY = '<mobile_service_application_key>';

// Numeric part of the project ID assigned by the Google API console.
var GCM_SENDER_ID = '<gcm_sender_id>';

// Define the MobileServiceClient as a global variable.
var mobileClient;

var app = {
    // Application Constructor
    initialize: function() {
        this.bindEvents();
    },
    // Bind Event Listeners
    //
    // Bind any events that are required on startup. Common events are:
    // 'load', 'deviceready', 'offline', and 'online'.
    bindEvents: function() {
        document.addEventListener('deviceready', this.onDeviceReady, false);
    },
    // deviceready Event Handler
    //
    // The scope of 'this' is the event. In order to call the 'receivedEvent'
    // function, we must explicitly call 'app.receivedEvent(...);'
    onDeviceReady: function() {
        
		// Define the Mobile Services client.
        mobileClient = new WindowsAzure.MobileServiceClient(MOBILE_SERVICE_URL, MOBILE_SERVICE_APP_KEY);
        todoItemTable = mobileClient.getTable('TodoItem');
	    
        // #region notification-registration			
        // Define the PushPlugin.
		var pushNotification = window.plugins.pushNotification;
		
		// Platform-specific registrations.
        if ( device.platform == 'android' || device.platform == 'Android' ){
			// Register with GCM for Android apps.
            pushNotification.register(
               app.successHandler, app.errorHandler,
               { 
				"senderID": GCM_SENDER_ID, 
				"ecb": "app.onNotificationGCM" 
				});
        } else if (device.platform === 'iOS') {
            // Register with APNS for iOS apps.
            pushNotification.register(
                app.tokenHandler,
                app.errorHandler, {                    
                    "ecb": "app.onNotificationAPN"
                });
        }
		else if(device.platform === "Win32NT"){
			// Register with MPNS for WP8 apps.
			pushNotification.register(
				app.channelHandler,
				app.errorHandler,
				{
					"channelName": "MyPushChannel",
					"ecb": "app.onNotificationWP8",
					"uccb": "app.channelHandler",
					"errcb": "app.ErrorHandler"
			});
		}
        // #endregion notifications-registration
		
        // #region todolist-quickstart
        // Read current data and rebuild UI.
        // If you plan to generate complex UIs like this, consider using a JavaScript templating library.
        function refreshTodoItems() {
            $('#summary').html("Loading...");
            var query = todoItemTable.where({ complete: false });

            query.read().then(function(todoItems) {
                var listItems = $.map(todoItems, function(item) {
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
        $('#add-item').submit(function(evt) {
            var textbox = $('#new-item-text'),
                itemText = textbox.val();
            if (itemText !== '') {
                todoItemTable.insert({ text: itemText, complete: false }).then(refreshTodoItems, handleError);
            }
            textbox.val('').focus();
            evt.preventDefault();
        });

        $('#refresh').click(function(evt) {
            refreshTodoItems();
            evt.preventDefault();
        });

        // Handle update
        $(document.body).on('change', '.item-text', function() {
            var newText = $(this).val();
            todoItemTable.update({ id: getTodoItemId(this), text: newText }).then(null, handleError);
        });

        $(document.body).on('change', '.item-complete', function() {
            var isComplete = $(this).prop('checked');
            todoItemTable.update({ id: getTodoItemId(this), complete: isComplete }).then(refreshTodoItems, handleError);
        });

        // Handle delete
        $(document.body).on('click', '.item-delete', function () {
            todoItemTable.del({ id: getTodoItemId(this) }).then(refreshTodoItems, handleError);
        });

        // On initial load, start by fetching the current data
        refreshTodoItems();
		
        app.receivedEvent('deviceready');
        // #endregion todolist-quickstart
    },

    // #region notification-callbacks
    // Callbacks from PushPlugin
    onNotificationGCM: function (e) {
        switch (e.event) {
            case 'registered':
                // Handle the registration.
                if (e.regid.length > 0) {
                    console.log("gcm id " + e.regid);

                    if (mobileClient) {

                        // Call the integrated Notification Hub client.
                        var hub = new NotificationHub(mobileClient);

                        // Template registration.
                        var template = "{ \"data\" : {\"message\":\"$(message)\"}}";

                        // (gcmRegId, ["tag1","tag2"], templateName, templateBody)
                        hub.gcm.register(e.regid, null, "myTemplate", template).done(function () {
                            alert("Registered with hub!");
                        }).fail(function (error) {
                            alert("Failed registering with hub: " + error);
                        });
                    }
                }
                break;

            case 'message':
			
				if (e.foreground)
				{
					// Handle the received notification when the app is running
					// and display the alert message. 
					alert(e.payload.message);
				}
                break;

            case 'error':
                alert('GCM error: ' + e.message);
                break;

            default:
                alert('An unknown GCM event has occurred');
                break;
        }
    },

    // Handle the token from APNS and create a new hub registration.
    tokenHandler: function (result) {
        console.log('device token = ' + result);

        if (mobileClient) {

            // Call the integrated Notification Hub client.
			var hub = new NotificationHub(mobileClient);

            // This is a template registration.
            var template = "{\"aps\":{\"alert\":\"$(message)\"}}";

            // (deviceId, ["tag1","tag2"], templateName, templateBody, expiration)
            hub.apns.register(result, null, "myTemplate", template, null).done(function () {
                alert("Registered with hub!");
            }).fail(function (error) {
                alert("Failed registering with hub: " + error);
            });
        }
    },

    // Handle the notification when the iOS app is running.
    onNotificationAPN: function (event) {
        console.log("event.alert " + event.alert);

        // Display the alert message in an alert.
        alert(event.alert);
			
        // Do some even fancier stuff here if we requested them in the registration.    
        // if (event.sound)
        // {
        //	   // Play a custom sound file.
        //     var snd = new Media(event.sound);
        //     snd.play();
        // }
        //     
        // if (event.badge)
        // {
        //	   // Display a badge number.
        //     pushNotification.setApplicationIconBadgeNumber(successHandler, errorHandler, event.badge);
        // }
    },
		
    // Handle the channel URI from MPNS and create a new hub registration. 
    channelHandler: function(result) {
        if (result.uri !== "")
        {
            console.log('channel URI = ' + result.uri);

            if (mobileClient) {

                // Call the integrated Notification Hub client.

                // Define the Notification Hubs client.
                var hub = new NotificationHub(mobileClient);

                // This is a template registration.
                var template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<wp:Notification xmlns:wp=\"WPNotification\">" +
                        "<wp:Toast>" +
                            "<wp:Text1>$(message)</wp:Text1>" +
                        "</wp:Toast>" +
                    "</wp:Notification>";
               
                // (channelUri, ["tag1","tag2"] , templateName, templateBody)
                hub.mpns.register(result.uri, null, "myTemplate", template).done(function () {
                    alert("Registered with hub!");
                }).fail(function (error) {
                    alert("Failed registering with hub: " + error);
                });
            }
        }
        else{
            console.log('channel URI could not be obtained!');
        }
    },
		
    // Handle the notification when the WP8 app is running.
    onNotificationWP8: function(event){
        if (event.jsonContent)
        {
            // Display the alert message in an alert.
            alert(event.jsonContent['wp:Text1']);
        }
    },
    // #endregion notification-callbacks

	successHandler: function (result) {
        console.log("callback success, result = " + result);
    },

    errorHandler: function (error) {
        alert(error);
    },
};
