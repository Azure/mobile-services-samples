function insert(item, user, request) {

    var opponentId = request.parameters.OpponentUserId;
    var user1Name;
    // Query the users table to find out the username of the originator
    var userTable = tables.getTable("users")
    userTable.where({ UserId: user.userId }).read({
        success: function (results) {
            if (results.length == 0) {
                request.respond(500, "moves/insert.js: User with UserId= " + user.userId + " not found.")
                return;
            }
            if (results.length == 1) {
                // Found a user
                user1Name = results[0].UserName;
                getUserIdAndSendNotification(request, user1Name, opponentId, item);
                request.execute();
                return;
            } else {
                // If UserId has more than one user, pick the first.
                user1Name = results[0].UserName;
                getUserIdAndSendNotification(request, user1Name, opponentId, item);
                request.execute();
                return;
            }
        }
    });
}

function getUserIdAndSendNotification(request, user1Name, user2, item) {
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
                sendNotification(request, user1Name, userId, item);
            } else {
                // If more than one user, pick the first.
                var userId = results[0].UserId;
                sendNotification(request, user1Name, userId, item);
            }
        }
    });
}

function sendNotification(request, user1Name, userId, item) {

    console.log("userId = " + userId)
    // Notify the other user that it's their turn, or that the game is over.
    var pushText = "";
    if (item.GameResult == 0) {
        pushText = " played and now it's your turn!"
        push.wns.sendBadge(userId, "alert", {
            success: function (pushResponse) {
                console.log("Sent badge push:", pushResponse);
            },
            error: function (err)
            { console.log("Badge push failed " + err) }
        });
    }
    else if (item.GameResult == 1) {
        pushText = " played and the game is over! " + item.Winner + " wins!"
    }
    else if (item.GameResult == 2) {
        pushText = " played and the game is over! It's a tie."
    }



    push.wns.sendToastText01(userId, {
        text1: "TicTacToe: " + user1Name + pushText
    }, {
        success: function (pushResponse) {
            console.log("Sent push:", pushResponse);
        },
        error: function (err) {
            console.log("Error pushing notification " + err)
        }
    });

}