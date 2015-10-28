var GCM_SENDER_ID = 'GCM_SENDER_ID'; //Replace with your own ID
var mobileServiceClient;
var pushNotification;

// Create the Azure client register for notifications, 
// replace with URL and app key of your mobile service.
document.addEventListener('deviceready', function () {
    mobileServiceClient = new WindowsAzure.MobileServiceClient(
                    'MOBILE_SERVICE_URL',
                    'MOBILE_SERVICE_APP_KEY');

    // Create a new PushNotification and start registration with the PNS.
    pushNotification = PushNotification.init({
        "android": { "senderID": GCM_SENDER_ID },
        "ios": { "alert": "true", "badge": "false", "sound": "false" }
    });

    // Handle the registration event.
    pushNotification.on('registration', function (data) {
        // Get the native platform of the device.
        var platform = device.platform;
        // Get the handle returned during registration.
        var handle = data.registrationId;
        // Set the device-specific message template.
        if (platform == 'android' || platform == 'Android') {
            // Template registration.
            var template = '{ "data" : {"message":"$(message)"}}';
            // Register for notifications.
            mobileServiceClient.push.gcm.registerTemplate(handle,
                'myTemplate', template, null)
                .done(registrationSuccess, registrationFailure);
        } else if (device.platform === 'iOS') {
            // Template registration.
            var template = '{"aps": {"alert": "$(message)"}}';
            // Register for notifications.            
            mobileServiceClient.push.apns.registerTemplate(handle,
                'myTemplate', template, null)
                .done(registrationSuccess, registrationFailure);
        }
    });

    // Handles the notification received event.
    pushNotification.on('notification', function (data) {
        // Display the alert message in an alert.
        alert(data.message);
        // Reload the items list.
        app.Storage.getData();
    });

    // Handles an error event.
    pushNotification.on('error', function (e) {
        // Display the error message in an alert.
        alert(e.message);
    });
});

var registrationSuccess = function () {
    alert('Registered with Azure!');
}

var registrationFailure = function (error) {
    alert('Failed registering with Azure: ' + error);
}