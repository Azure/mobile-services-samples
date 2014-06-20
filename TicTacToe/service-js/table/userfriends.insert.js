function insert(item, user, request) {

    // Determine if the friend was already added.
    
    var ct = tables.getTable("userfriends");
    ct.where({ user1Key: item.User1Key, user2Key: item.User2Key }).read({
        success: function (results) {
            if (results.length > 0) {
                // we already have a record for this friendship
                var existingItem = results[0];
                request.respond(200, existingItem);
            }
            else
                request.execute();
        }
    })
}