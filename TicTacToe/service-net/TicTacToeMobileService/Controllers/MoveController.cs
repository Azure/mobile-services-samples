using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using TicTacToeMobileService.DataObjects;
using TicTacToeMobileService.Models;

namespace TicTacToeMobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class MovesController : TableController<Move>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            TicTacToeMobileServiceContext context = new TicTacToeMobileServiceContext();
            DomainManager = new EntityDomainManager<Move>(context, Request, Services);
        }

        // GET tables/Moves
        public IQueryable<Move> GetAllMove()
        {
            return Query(); 
        }

        // GET tables/Moves/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Move> GetMove(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Moves/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Move> PatchMove(string id, Delta<Move> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Moves/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostMove(Move item)
        {
            // Find the user and game
            var dbContext = new TicTacToeMobileServiceContext();
            var userResult = dbContext.Users.Where(user => user.Id == item.UserId);
            User user1 = userResult.First();
            var gameResult = dbContext.Games.Where(game => game.Id == item.GameId);
            Game game1 = gameResult.First();

            // Get the second user.
            string user2Id = (game1.User1 == user1.Id ? game1.User2 : game1.User1);
            var userResult2 = dbContext.Users.Where(user => user.Id == user2Id);
            User user2 = userResult2.First();

            Move current = await InsertAsync(item);
            string pushText = "TicTacToe: ";

            if (item.GameResult == 0) // Game is not over.
            {
                WindowsPushMessage badgeMessage = new WindowsPushMessage();
                badgeMessage.XmlPayload = @"<?xml version=""1.0"" encoding=""utf-8""?>" +
                                     @"<badge value=""alert""/>";

                try
                {
                    var tags = new string[] { user2.UserId };
                    await Services.Push.SendAsync(badgeMessage, tags);
                }
                catch (System.Exception ex)
                {
                    Services.Log.Error(ex.Message, null, "Push.SendAsync Error");
                }

                pushText = pushText + user1.UserName + " played and now it's your turn!";
            }
            else if (item.GameResult == 1) // Game is over, a player won.
            {
                pushText = pushText + user1.UserName + " played and the game is over. " + item.Winner + " won!";
            }
            else if (item.GameResult == 2) // Game ended in a draw.
            {
                pushText = pushText + user1.UserName + " played and the game is over! It's a tie.";
            }

            WindowsPushMessage toastMessage = new WindowsPushMessage();
            toastMessage.XmlPayload = @"<?xml version=""1.0"" encoding=""utf-8""?>" +
                                 @"<toast><visual><binding template=""ToastText01"">" +
                                 @"<text id=""1"">" + pushText + @"</text>" +
                                 @"</binding></visual></toast>";
            try
            {
                var tags = new string[] { user2.UserId };
                var result = await Services.Push.SendAsync(toastMessage, tags);
                Services.Log.Info(result.State.ToString());
            }
            catch (System.Exception ex)
            {
                Services.Log.Error(ex.Message, null, "Push.SendAsync Error");
            }
            
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Moves/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteMove(string id)
        {
             return DeleteAsync(id);
        }

    }
}