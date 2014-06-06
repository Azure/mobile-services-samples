(function () {

    //#region Constants
    var API_VERSION = "?api-version=2013-10";

    // #endregion

    var parseConnectionString = function (hub, connectionString) {
        var parts = connectionString.split(';');
        if (parts.length !== 3) {
            throw "Error parsing connection string";
        }

        parts.forEach(function (part) {
            if (part.indexOf('Endpoint') === 0) {
                hub.endpoint = 'https' + part.substring(11);
            } else if (part.indexOf('SharedAccessKeyName') === 0) {
                hub.sasKeyName = part.substring(20);
            } else if (part.indexOf('SharedAccessKey') === 0) {
                hub.sasKeyValue = part.substring(16);
            }
        });
    };

    NotificationHub = function (hubPath, connectionString) {
        console.log("building hub");

        // parse connection string
        parseConnectionString(this, connectionString);
        this.hubPath = hubPath;

        this.gcm = new gcm(this);
        this.apns = new apns(this);
		this.mpns = new mpns(this);
    };

    var getSelfSignedToken = function (targetUri, sharedKey, ruleId, expiresInMins) {
        targetUri = encodeURIComponent(targetUri.toLowerCase()).toLowerCase();

        // Set expiration in seconds
        var expireOnDate = new Date();
        expireOnDate.setMinutes(expireOnDate.getMinutes() + expiresInMins);
        var expires = Date.UTC(expireOnDate.getUTCFullYear(), expireOnDate
            .getUTCMonth(), expireOnDate.getUTCDate(), expireOnDate
            .getUTCHours(), expireOnDate.getUTCMinutes(), expireOnDate
            .getUTCSeconds()) / 1000;
        var tosign = targetUri + '\n' + expires;

        var signature = CryptoJS.HmacSHA256(tosign, sharedKey);
        var base64signature = signature.toString(CryptoJS.enc.Base64);
        var base64UriEncoded = encodeURIComponent(base64signature);

        // construct authorization string
        var token =
            "SharedAccessSignature sr=" + targetUri + "&sig="
            + base64UriEncoded + "&se=" + expires + "&skn=" + ruleId;

        // console.log("signature:" + token);
        return token;
    };

    // #region Local storage
    // object: {location: '', deviceToken: ''}
    var storeInContainer = function (key, object) {
        window.localStorage.setItem(key, JSON.stringify(object));
    };

    var deleteFromContainer = function (key) {
        window.localStorage.removeItem(key);
    };

    var getFromContainer = function (key) {
        console.log("get from container: " + window.localStorage.getItem(key));

        if (typeof window.localStorage.getItem(key) === 'string') {
            return JSON.parse(window.localStorage.getItem(key));
        }
        return undefined;
    };
    // #endregion End of local storage

    // #region Payload
    var gcmNativePayload = '<?xml version="1.0" encoding="utf-8"?><entry xmlns="http://www.w3.org/2005/Atom"><content type="application/xml"><GcmRegistrationDescription xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/netservices/2010/10/servicebus/connect">{0}<GcmRegistrationId>{1}</GcmRegistrationId></GcmRegistrationDescription></content></entry>';
    var gcmTemplatePayload = '<?xml version="1.0" encoding="utf-8"?><entry xmlns="http://www.w3.org/2005/Atom"><content type="application/xml"><GcmTemplateRegistrationDescription xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/netservices/2010/10/servicebus/connect">{0}<GcmRegistrationId>{1}</GcmRegistrationId><BodyTemplate><![CDATA[{2}]]></BodyTemplate></GcmTemplateRegistrationDescription></content></entry>';
    var apnsNativePayload = '<?xml version="1.0" encoding="utf-8"?><entry xmlns="http://www.w3.org/2005/Atom"><content type="application/xml"><AppleRegistrationDescription xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/netservices/2010/10/servicebus/connect">{0}<DeviceToken>{1}</DeviceToken></AppleRegistrationDescription></content></entry>';
	var apnsTemplatePayload = '<?xml version="1.0" encoding="utf-8"?><entry xmlns="http://www.w3.org/2005/Atom"><content type="application/xml"><AppleTemplateRegistrationDescription xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/netservices/2010/10/servicebus/connect">{0}<DeviceToken>{1}</DeviceToken><BodyTemplate><![CDATA[{2}]]></BodyTemplate></AppleTemplateRegistrationDescription></content></entry>';
    var mpnsNativePayload = '<?xml version="1.0" encoding="utf-8"?><entry xmlns="http://www.w3.org/2005/Atom"><content type="application/xml"><MpnsRegistrationDescription xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/netservices/2010/10/servicebus/connect">{0}<ChannelUri>{1}</ChannelUri></MpnsRegistrationDescription></content></entry>';
	var mpnsTemplatePayload = '<?xml version="1.0" encoding="utf-8"?><entry xmlns="http://www.w3.org/2005/Atom"><content type="application/xml"><MpnsTemplateRegistrationDescription xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/netservices/2010/10/servicebus/connect">{0}<ChannelUri>{1}</ChannelUri><BodyTemplate><![CDATA[{2}]]></BodyTemplate><MpnsHeaders><MpnsHeader><Header>X-WindowsPhone-Target</Header><Value>toast</Value></MpnsHeader><MpnsHeader><Header>X-NotificationClass</Header><Value>2</Value></MpnsHeader></MpnsHeaders></MpnsTemplateRegistrationDescription></content></entry>';
    var buildCreatePayload = function (registration) {
        /* expecting: tags, deviceToken */

        var nativePayload;
        var templatePayload;

        console.log("registration: " + JSON.stringify(registration));
 
        switch (registration.platform) {
            case 'gcm':
                nativePayload = gcmNativePayload;
                templatePayload = gcmTemplatePayload;
                break;
            case 'apns':
                nativePayload = apnsNativePayload;
				templatePayload = apnsTemplatePayload;
                break;
			case 'mpns':
                nativePayload = mpnsNativePayload;
				templatePayload = mpnsTemplatePayload
                break;
        }

        var registrationPayload;

        // if templateBody != undefined use template
        if (typeof registration.templateBody !== 'undefined') {
            registrationPayload =
                templatePayload.replace('{1}', registration.deviceToken)
                    .replace('{2}', registration.templateBody);

            var tagstring = '';
            if (typeof registration.tags === 'object') {
                tagstring = '<Tags>' + registration.tags.join(',') + '</Tags>';
            }
            registrationPayload = registrationPayload.replace('{0}', tagstring);
        } else // native
        {
            registrationPayload = nativePayload.replace('{1}', registration.deviceToken);
            var tagstring = '';
            if (typeof registration.tags === 'object') {
                tagstring = '<Tags>' + registration.tags.join(',') + '</Tags>';
            }
            registrationPayload = registrationPayload.replace('{0}', tagstring);
        }

		console.log("registration payload: " + registrationPayload);
        return registrationPayload;
    };

    // #endregion Payload

    // #region CRUD ops

    // Create registration ID
    var createRegistrationId = function (hub) {
        var serverUrl = hub.endpoint + hub.hubPath + "/registrationIDs" + API_VERSION;

        var token = getSelfSignedToken(serverUrl, hub.sasKeyValue, hub.sasKeyName, 60);

        var deferred = $.Deferred();
        $.ajax({
            type: "POST",
            url: serverUrl,
            headers: {
                "Authorization": token
            },
        }).done(function (data, status, response) {
            var location = response.getResponseHeader("Location");
            console.log('create registration ID success: ' + location);

            var regex = /\S+\/registrationIDs\/([^?]+).*/;
            var regId = regex.exec(location)[1];
            console.log("regId: " + regId);
            deferred.resolve(regId);

        }).fail(function (response, status, error) {
            console.log("Error: " + error);
            deferred.reject("Error: " + error);
        });

        return deferred.promise();
    };

    // CREATE
    var createRegistration = function (hub, registration) {
        return createRegistrationId(hub).then(function (regId) {
            return updateRegistration(hub, regId, registration).then(function (location) {
                return regId;
            });
        });
    };

    // UPDATE
    var updateRegistration = function (hub, regId, registration) {
        /* expecting: registration.regId, registration.deviceToken */

        var registrationPayload = buildCreatePayload(registration);

        var serverUrl = hub.endpoint + hub.hubPath + "/registrations/" + regId + API_VERSION;

        console.log('serverUrl:' + serverUrl);
        console.log('payload:' + registrationPayload);

        var token = getSelfSignedToken(serverUrl, hub.sasKeyValue, hub.sasKeyName, 60);

        var deferred = $.Deferred();
        $.ajax({
            type: "PUT",
            url: serverUrl,
            headers: {
                "Content-Type": "application/atom+xml",
                "Authorization": token,
                // "If-Match:": '*'
            },
            data: registrationPayload
        }).done(function (data, status, response) {
            console.log("update registration data: " + JSON.stringify(data));
            console.log("update registration data: " + JSON.stringify(status));
            var location = response.getResponseHeader("Content-Location");
            deferred.resolve(location);
        }).fail(function (response, status, error) {
            console.log("Error: " + error);
            console.log("Response: " + JSON.stringify(response));
            console.log("Status: " + status);
            deferred.reject("Error: " + error);
        });

        return deferred.promise();
    };

    // DELETE
    var deleteRegistration = function (hub, regId) {
        var serverUrl = hub.endpoint + hub.hubPath + "/registrations/" + regId + API_VERSION;

        console.log('url:' + serverUrl);
        console.log('registration ID:' + regId);

        var token = getSelfSignedToken(serverUrl, hub.sasKeyValue, hub.sasKeyName, 60);

        var deferred = $.Deferred();
        $.ajax({
            type: "DELETE",
            url: serverUrl,
            headers: {
                "Content-Type": "application/atom+xml",
                "Authorization": token,
                "If-Match:": '*'
            },
        }).done(function (data, status, response) {
            console.log("Deleted registration");
            deferred.resolve();
        }).fail(function (response, status, error) {
            console.log("Error: " + error);
            deferred.reject("Error: " + error);
        });

        return deferred.promise();
    };

    // #endregion CRUD ops

    var register = function (hub, registration) {
        var tileKey = 'application';
        if (typeof registration.tileId !== 'undefined') {
            tileKey = registration.tileId;
        }

        // create key
        var regKey = hub.endpoint + hub.hubPath + '/' + tileKey + '/' + registration.name;

        // if key exists
        var deferred = $.Deferred();
        var regInfo = getFromContainer(regKey);
        if (typeof regInfo !== 'undefined') {
            // update registration
            console.log("stored registration found, updating existing registration for ID: " + regInfo.regId);

            updateRegistration(hub, regInfo.regId, registration).then(
                function (location) {
                    deferred.resolve(location);
                }, function (error) {
                    // if not exists / recreate
                    if (error === '404' || error === '410') {
                        deferred.resolve(createRegistration(hub, registration));
                    } else {
                        deferred.reject(error);
                    }
                }).then(function (location) {
                    // update regInfo with new location
                    regInfo.location = location;
                    regInfo.deviceToken = registration.deviceToken;
                    storeInContainer(regKey, regInfo);

                    deferred.resolve(location);
                }, function (error) {
                    deferred.reject(error);
                });
        } else {
            // create new
            return createRegistration(hub, registration).then(
                function (regId) {
                    regInfo = {};
                    regInfo.regId = regId;
                    regInfo.location = location;
                    regInfo.deviceToken = registration.deviceToken;

                    storeInContainer(regKey, regInfo);
                    getFromContainer(regKey);

                    deferred.resolve(location);
                }, function (error) {
                    deferred.reject(error);
                });
        }

        return deferred.promise();
    };

    var unregister = function (hub, registration) {
        // create key
        var tileKey = 'application';
        if (typeof registration.tileId !== 'undefined') {
            tileKey = registration.tileId;
        }

        // create key
        var regKey = hub.endpoint + hub.hubPath + '/' + tileKey + '/' + registration.name;

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
        register: function (gcmRegId, tags) {
            return this.hub._register(this.hub, {
                platform: "gcm",
                deviceToken: gcmRegId,
                tags: tags,
                name: '$native'
            });
        },

        registerTemplate: function (gcmRegId, templateName, templateBody, tags) {
            return this.hub._register(this.hub, {
                platform: "gcm",
                deviceToken: gcmRegId,
                tags: tags,
                name: templateName,
                templateBody: templateBody
            });
        },

        unregister: function (gcmRegId, tags) {
            return this.hub._unregister(this.hub, {
                platform: "gcm",
                deviceToken: gcmRegId,
                tags: tags,
                name: '$native'
            });
        },
    };

    var apns = function (hub) {
        this.hub = hub;
    };

    apns.prototype = {
        register: function (deviceToken, tags) {
            return this.hub._register(this.hub, {
                platform: "apns",
                deviceToken: deviceToken,
                tags: tags,
                name: '$native'
            });
        },

        registerTemplate: function (deviceToken, templateName, templateBody, tags) {
            return this.hub._register(this.hub, {
                platform: "apns",
                deviceToken: deviceToken,
                tags: tags,
                name: templateName,
                templateBody: templateBody
            });
        },

        unregister: function (deviceToken, tags) {
            return this.hub._unregister(this.hub, {
                platform: "apns",
                handle: deviceToken,
                tags: tags,
                name: '$native'
            });
        }
    };
	
	 var mpns = function (hub) {
        this.hub = hub;
    };

    mpns.prototype = {
        register: function (channel, tags) {
            return this.hub._register(this.hub, {
                platform: "mpns",
                deviceToken: channel,
                tags: tags,
                name: '$native'
            });
        },

        registerTemplate: function (channel, templateName, templateBody, tags) {
            return this.hub._register(this.hub, {
                platform: "mpns",
                deviceToken: channel,
                tags: tags,
                name: templateName,
                templateBody: templateBody
            });
        },

        unregister: function (channel, tags) {
            return this.hub._unregister(this.hub, {
                platform: "mpns",
                handle: channel,
                tags: tags,
                name: '$native'
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

}());
