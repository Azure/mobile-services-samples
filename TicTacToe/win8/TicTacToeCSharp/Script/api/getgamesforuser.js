exports.get = function (request, response) {
    var commandText = "SELECT games.id, User1, User2, Board, GameResult, " +
                "CurrentPlayerIndex, users1.UserName AS User1Name, " +
                "users2.UserName AS User2Name FROM games " +
                "JOIN users users1 ON (games.User1 = users1.id) " +
                "JOIN users users2 ON (games.User2 = users2.id) " +
                "WHERE games.GameResult = 0 " +
                "AND (games.User1 = ? OR games.User2 = ?)";
    console.log("getgamesforuser: SQL string is " + commandText);
    var userKey = request.param('UserKey');
    var params = [userKey, userKey];
    request.service.mssql.query(commandText, params, {
        success: function (results) {
            request.respond(200, results);
            return;
        },
        error: function (err) {
            console.log("Error in getgamesforuser : " + err)
            request.respond(500, "Error in getgamesforuser: " + err);
            return;
        }
    });

};