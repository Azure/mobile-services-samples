// See documentation at http://go.microsoft.com/fwlink/?LinkId=296704&clcid=0x409
function insert(item, user, request) {

    // The following code manages channels and should be retained in this script
    var ct = tables.getTable("channels");
    ct.where({ installationId: item.installationId }).read({
        success: function (results) {
            if (results.length > 0) {
                // we already have a record for this user/installation id - if the 
                // channel is different, update it otherwise just respond
                var existingItem = results[0];
                if (existingItem.channelUri !== item.channelUri) {
                    existingItem.channelUri = item.channelUri;
                    ct.update(existingItem, {
                        success: function () {
                            request.respond(200, existingItem);
                        }
                    });
                }
                else {
                    // no change necessary, just respond
                    request.respond(200, existingItem);
                }
            }
            else {
                // no matching installation, insert the record

                request.execute();
            }
        }
    }
    )

}