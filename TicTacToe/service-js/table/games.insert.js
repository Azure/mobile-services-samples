function insert(item, user, request) {
    var user1Name;
    // Query the users table to find out the username of the originator
    var userTable = tables.getTable("users")
    userTable.where({ UserId: user.userId }).read({
        success: function (results) {
            if (results.length == 0) {
                request.respond(500, "games/insert.js: User with UserId= " + user.userId + " not found.")
                return;
            }
            if (results.length == 1) {
                // Found a user
                user1Name = results[0].UserName;
                getUserIdAndSendNotification(request, user1Name, item.User2);
                request.execute();
                return;
            } else {
                // If UserId has more than one user, pick the first.
                user1Name = results[0].UserName;
                getUserIdAndSendNotification(request, user1Name, item.User2);
                request.execute();
                return;
            }
        }
    });
}

function getUserIdAndSendNotification(request, user1Name, user2) {
    var userTable = tables.getTable("users")
    userTable.where({ id: user2 }).read({
        success: function (results) {
            if (results.length == 0) {
                request.respond(500, "moves/insert.js: User with id= " + user2 + " not found.")
                return;
            }
            if (results.length == 1) {
                // Found a user
                var userId = results[0].UserId;
                sendNotification(request, user1Name, userId);
            } else {
                // If more than one user, pick the first.
                var userId = results[0].UserId;
                sendNotification(request, user1Name, userId);
            }
        }
    });
}

function sendNotification(request, user1Name, userId)
{
    // Notify user2 that a new game has been started.
    console.log("userId = " + userId)
    push.wns.sendBadge(userId, "alert", {
        success: function (pushResponse) {
            console.log("Sent badge push:", pushResponse);
        },
        error: function (err)
        { console.log("Badge push failed " + err) }
    });


    push.wns.sendToastText04(userId, {
        text1: user1Name + " wants to play TicTacToe!"
    }, {
        success: function (pushResponse) {
            console.log("Sent push:", pushResponse);
        },
        error: function (err)
        { console.warn("Push failed " + err) }
    });

}