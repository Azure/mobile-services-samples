function insert(item, user, request) {


    var ct = tables.getTable("userchannels");
    ct.where({ ChannelId: item.ChannelId, UserKey: item.UserKey }).read({
        success: function (results) {
            if (results.length > 0) {
                // we already have a record for this user and channel id
                request.respond(200, item);
            }
            else {
                // no matching record, insert it
                request.execute();
            }
        }
    }
    )

}