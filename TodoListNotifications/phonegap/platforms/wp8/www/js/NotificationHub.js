(function () {

    NotificationHub = function (mobileClient) {

        // Set the client to use to register for push.
        this.mobileClient = mobileClient;
        
        this.gcm = new gcm(this);
        this.apns = new apns(this);
        this.mpns = new mpns(this);
    };

    // #region CRUD ops
    // Create registration ID--payload on upsert.
    var createRegistrationId = function (hub) {

        // Variables needed to make the request to the mobile service.
        var method = "POST";
        var uriFragment = "/push/registrationids";

        var deferred = $.Deferred();

        // Send a Notification Hubs registration request to the Mobile Service.
        hub.mobileClient._request(method, uriFragment, null, null, null,
            function (error, response) {
            if (error) {
                console.log("Error: " + error);
                deferred.reject("Error: " + error);
            } else {

                // Get the unique registration ID from the Location header.
                var location = response.getResponseHeader("Location");            
                var regex = /\S+\/registrations\/([^?]+).*/;
                var regId = regex.exec(location)[1];

                // Return the registration ID.
                deferred.resolve(regId);
            }
        });
        return deferred.promise();
    };

    // Update an existing registration--includes payload.
    var updateRegistration = function (hub, regId, registration) {

        // Variables needed to make the request to the mobile service.
        var registrationPayload = buildCreatePayload(registration);
        var method = "PUT";
        var uriFragment = "/push/registrations/" + regId;

        var deferred = $.Deferred();

        // Send a Notification Hubs registration update to the Mobile Service.
        hub.mobileClient._request(method, uriFragment, registrationPayload, null,
            null, function (error, response) {
            if (error) {
                console.log("Error: " + error);
                deferred.reject("Error: " + error);
            } else {
                console.log("Updated registration: " + regId);
                deferred.resolve();
            }
        });
        return deferred.promise();
    };

    var deleteRegistration = function (hub, regId) {

        var method = "DELETE";
        var uriFragment = "/push/registrations/" + regId;

        var deferred = $.Deferred();

        // Send a Notification Hubs registration update to the Mobile Service.
        hub.mobileClient._request(method, uriFragment, null, null,
            null, function (error, response) {                
                if (error) {
                    console.log("Error: " + error);
                    deferred.reject("Error: " + error);
                } else {
                    console.log("Deleted registration");
                    deferred.resolve();
                }
            });
        return deferred.promise();
    };

    // Create registration as an insert + update (POST + PUT).
    var createRegistration = function (hub, registration) {
        return createRegistrationId(hub).then(function (regId) {
            return updateRegistration(hub, regId, registration).then(function () {
                return regId;
            });
        });
    };

    // #endregion CRUD ops

    // #region helper functions
    var buildRegKey = function (hub, registration) {

        var tileKey = 'application';
        if (typeof registration.tileId !== 'undefined') {
            tileKey = registration.tileId;
        }

        // create key
        var regKey = hub.mobileClient.applicationUrl + tileKey;
        if (registration.templateName) {
            regKey = regKey + '/' + registration.templateName;
        }
        return regKey;
    };

    var buildCreatePayload = function (registration) {

        // The payload matches the registration object, so all 
        // we need to do is add any extra info not part of the registration.
        var payload = registration;

        // Add static headers required by MPNS template
        if (payload.platform === 'mpns' && payload.templateBody) {

            // Add the MPNS headers required for a template registration.
            payload.headers = { "X-WindowsPhone-Target": "toast", "X-NotificationClass": "2" };
        }

        // Return the payload as a string.
        return JSON.stringify(payload);
    };
    // #endregion helper functions

    // #region local storage
    // object: {regId: '', deviceToken: ''}
    var storeInContainer = function (key, object) {
        localStorage.setItem(key, JSON.stringify(object));
    };

    var deleteFromContainer = function (key) {
        localStorage.removeItem(key);
    };

    var getFromContainer = function (key) {
 
        if (typeof localStorage.getItem(key) === 'string') {
            return JSON.parse(localStorage.getItem(key));
        }
        return undefined;
    };
    // #endregion local storage

    // #region public API
    var register = function (hub, registration) {

        // Build the key used to store info in local storage,
        // then get the stored registration--if it exists.
        var regKey = buildRegKey(hub, registration);
        var regInfo = getFromContainer(regKey);

        var retries = 0;
        var deferred = $.Deferred();

        if (typeof regInfo !== 'undefined') {

            // If we find stored registration, then this should be 
            // an update of the existing registration.
            updateRegistration(hub, regInfo.regId, registration).then(
                function () {
                    deferred.resolve();
                }, function (error) {
                    // if not exists / recreate
                    if ((error === '404' || error === '410') && retries < 2 ) {
                        deferred.resolve(createRegistration(hub, registration));
                    } else {
                        deferred.reject(error);
                    }
                }).then(function () {
                    // update regInfo with new location
                    //regInfo.location = location;
                    // Store the latest device token.
                    regInfo.deviceToken = registration.deviceToken;
                    storeInContainer(regKey, regInfo);

                    deferred.resolve();
                }, function (error) {
                    deferred.reject(error);
                });
        } else {
            // create new registration.
            return createRegistration(hub, registration).then(
                function (regId) {
                    // Store registration info with the returned ID.
                    regInfo = {};
                    regInfo.regId = regId;
                    regInfo.deviceToken = registration.deviceToken;

                    storeInContainer(regKey, regInfo);
                    getFromContainer(regKey);

                    deferred.resolve();
                }, function (error) {
                    deferred.reject(error);
                });
        }
        return deferred.promise();
    };

    var unregister = function (hub, registration) {

        var regKey = buildRegKey(hub, registration);

        var deferred = $.Deferred();
        var regInfo = getFromContainer(regKey);
        if (typeof regInfo === 'undefined') {
            deferred.resolve();
        }

        // delete registration
        deleteRegistration(hub, regInfo.regId).then(function () {
            deleteFromContainer(regKey);
            deferred.resolve();
            }, function (error) {
            // not an error
            if (error === 'Not Found') {
                deleteFromContainer(regKey);
                deferred.resolve();
            }
            deferred.reject(error);
            });
        return deferred;
    };

    var gcm = function (hub) {
        this.hub = hub;
    };

    gcm.prototype = {

        register: function (gcmRegId, tags, templateName, templateBody) {
            return this.hub._register(this.hub, {
                platform: "gcm",
                deviceId: gcmRegId,
                tags: tags,
                templateName: templateName,
                templateBody: templateBody
            });
        },

        unregister: function (gcmRegId, tags, templateName) {
            return this.hub._unregister(this.hub, {
                platform: "gcm",
                deviceId: gcmRegId,
                tags: tags,
                templateName: templateName
            });
        },
    };

    var apns = function (hub) {
        this.hub = hub;
    };

    apns.prototype = {

        register: function (deviceToken, tags, templateName, templateBody, expiration) {
            return this.hub._register(this.hub, {
                platform: "apns",
                deviceId: deviceToken,
                tags: tags,
                templateName: templateName,
                templateBody: templateBody,
                expiration: expiration
            });
        },

        unregister: function (deviceToken, tags, templateName) {
            return this.hub._unregister(this.hub, {
                platform: "apns",
                deviceId: deviceToken,
                tags: tags,
                templateName: templateName
            });
        }
    };
	
    var mpns = function (hub) {
        this.hub = hub;
    };

	 mpns.prototype = {

	     register: function (channel, tags, templateName, templateBody) {
	         return this.hub._register(this.hub, {
	             platform: "mpns",
	             deviceId: channel,
	             tags: tags,
	             templateName: templateName,
	             templateBody: templateBody
	         });
	     },

	     unregister: function (channel, tags, templateName) {
	         return this.hub._unregister(this.hub, {
	             platform: "mpns",
	             deviceId: channel,
	             tags: tags,
	             templateName: templateName
	         });
	     }
	 };

    NotificationHub.prototype = {        
        _register: register,
        _unregister: unregister,

        gcm: this.gcm,
        apns: this.apns,
		mpns: this.mpns
    };
    // #endregion public API
}());
