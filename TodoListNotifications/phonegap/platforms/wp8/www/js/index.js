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
// Azure Mobile Service and Azure Notification Hub. 
var HUB_NAME = '<notification_hub_name>';
var HUB_ENDPOINT = '<shared_access_connection_string>';
var MOBILE_SERVICE_URL = '<mobile_service_url>';
var MOBILE_SERVICE_APP_KEY = '<mobile_service_application_key>';

// Numeric part of the project ID assigned by the Google API console.
var GCM_SENDER_ID = '<gcm_sender_id>';

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
        var client = new WindowsAzure.MobileServiceClient(MOBILE_SERVICE_URL, MOBILE_SERVICE_APP_KEY);
        todoItemTable = client.getTable('TodoItem');
	    
        // #region notification-registration			
        // Define the PushPlugin.
		var pushNotification = window.plugins.pushNotification;
		
		// Platform-specific registrations.
        if ( device.platform == 'android' || device.platform == 'Android' 
				|| device.platform == "amazon-fireos" ){
            console.log('Android registration');
			
			// Register with GCM for Android apps.
            pushNotification.register(
               app.successHandler, app.errorHandler,
               { 
				"senderID": GCM_SENDER_ID, 
				"ecb": "app.onNotificationGCM" 
				});
        } else if (device.platform === 'iOS') {
            console.log('iOS registration');

			// Register with APNS for iOS apps.
            pushNotification.register(
                app.tokenHandler,
                app.errorHandler, {
                    "badge": "true",
                    "sound": "true",
                    "alert": "true",
                    "ecb": "app.onNotificationAPN"
                });
        }
		else if(device.platform === "Win32NT"){
			console.log('WP8 registration');
			
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

                    var hub = new NotificationHub(HUB_NAME, HUB_ENDPOINT);
                    
					// Template registration.
					var template = "{ \"data\" : {\"message\":\"$(message)\"}}";
					
					hub.gcm.registerTemplate(e.regid, "myTemplate", template).done(function () {
                        alert("Registered with hub!");
                    }).fail(function (error) {
                        alert("Failed registering with hub: " + error);
                    });
					
					// // Native registration.
					// hub.gcm.register(e.regid).done(function () {
                        // alert("Registered with hub!");
                    // }).fail(function (error) {
                        // alert("Failed registering with hub: " + error);
                    // });
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

        // Define the Notification Hubs client.
        var hub = new NotificationHub(HUB_NAME, HUB_ENDPOINT);

		// This is a template registration.
		var template = "{\"aps\":{\"alert\":\"$(message)\"}}";
		
        hub.apns.registerTemplate(result, "myTemplate", template).done(function () {	
            alert("Registered with hub!");
        }).fail(function (error) {
            alert("Failed registering with hub: " + error);
        });
		
		// // Native registration.
		// hub.apns.register(result).done(function () {	
            // alert("Registered with hub!");
        // }).fail(function (error) {
            // alert("Failed registering with hub: " + error);
        // });
    },

    // Handle the notification when the iOS app is running.
    onNotificationAPN: function (event) {
        console.log("event.alert " + event.alert);

        // Display the alert message in an alert.
        alert(event.alert);
			
        // Do some even fancier stuff here...    
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
					
            // Define the Notification Hubs client.
            var hub = new NotificationHub(HUB_NAME, HUB_ENDPOINT);
					
            // This is a template registration.
            var template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<wp:Notification xmlns:wp=\"WPNotification\">" +
					"<wp:Toast>" +
						"<wp:Text1>$(message)</wp:Text1>" +
					"</wp:Toast>" +
				"</wp:Notification>";

            console.log("template: " + template);

            hub.mpns.registerTemplate(result.uri, "myTemplate", template).done(function () {	
            alert("Registered with hub!");
            }).fail(function (error) {
            alert("Failed registering with hub: " + error);
            });			
			
			// // Native registration.	
            // hub.mpns.register(result.uri).done(function () {	
                // alert("Registered with hub!");
            // }).fail(function (error) {
                // alert("Failed registering with hub: " + error);
            // });			
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
