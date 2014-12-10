function insert(item, user, request) {
    // Execute the request and send notifications.
    request.execute({
        success: function () {
            // Create a template-based payload.
            var payload = '{ "message" : "New item added: ' + item.text + '" }';

            // Write the default response and send a template notification
            // to all registered devices on all platforms.            
            push.send(null, payload, {
                success: function (pushResponse) {
                    console.log("Sent push:", pushResponse);
                    // Send the default response.
                    request.respond();
                },
                error: function (pushResponse) {
                    console.log("Error Sending push:", pushResponse);
                    // Send the an error response.
                    request.respond(500, { error: pushResponse });
                }
            });
        }
    });
}