using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using TicTacToeMobileService.DataObjects;
using TicTacToeMobileService.Models;

namespace TicTacToeMobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class GetGamesForUserController : ApiController
    {
        public ApiServices Services { get; set; }

        // GET api/GetGames
        public IQueryable<GameInfo> Get()
        {
            string commandText = "SELECT games.id, User1, User2, Board, GameResult, " +
                "CurrentPlayerIndex, users1.UserName AS User1Name, " +
                "users2.UserName AS User2Name FROM games " +
                "JOIN users users1 ON (games.User1 = users1.id) " +
                "JOIN users users2 ON (games.User2 = users2.id) " +
                "WHERE games.GameResult = 0 " +
                "AND (games.User1 = @p0 OR games.User2 = @p1)";
            TicTacToeMobileServiceContext dataContext = new Models.TicTacToeMobileServiceContext();
            string userKey = this.Request.GetQueryNameValuePairs()
                                 .Where(kvp => kvp.Key == "UserKey").First().Value;
            Services.Log.Info("GetGamesForUser: userkey = " + userKey);
            
            var query = dataContext.Database.SqlQuery<GameInfo>(
                commandText, userKey, userKey);
            
            var games = query.ToList();
            Services.Log.Info("GetGamesForUser: count = " + games.Count());
            return games.AsQueryable<GameInfo>();
        }

    }
}
