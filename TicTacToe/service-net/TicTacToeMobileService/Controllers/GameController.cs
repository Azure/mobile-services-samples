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
    public class GamesController : TableController<Game>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            TicTacToeMobileServiceContext context = new TicTacToeMobileServiceContext();
            DomainManager = new EntityDomainManager<Game>(context, Request, Services);
        }

        // GET tables/Games
        public IQueryable<Game> GetAllGame()
        {
            return Query(); 
        }

        // GET tables/Games/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Game> GetGame(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Games/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Game> PatchGame(string id, Delta<Game> patch)
        {
             Services.Log.Info("games/update called. item.CurrentPlayerIndex = " + patch.GetEntity().CurrentPlayerIndex);
             return UpdateAsync(id, patch);
        }

        // POST tables/Games/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostGame(Game item)
        {
            // Send a push notification to the opponent.
            var dbContext = new TicTacToeMobileServiceContext();
            var user1Result = from user in dbContext.Users
                              where user.Id == item.User1
                              select user;
            User user1  = user1Result.First();

            var user2Result = from user in dbContext.Users
                              where user.Id == item.User2
                              select user;
            User user2 = user2Result.First();
            Game current = await InsertAsync(item);
            WindowsPushMessage message = new WindowsPushMessage();
            message.XmlPayload = @"<?xml version=""1.0"" encoding=""utf-8""?>" +
                                 @"<toast><visual><binding template=""ToastText04"">" +
                                 @"<text id=""1"">" + user1.UserName + " wants to play TicTacToe!" + @"</text>" +
                                 @"</binding></visual></toast>";
            try
            {
                string[] tags = { user2.UserId };
                
                var result = await Services.Push.SendAsync(message, tags);
                Services.Log.Info(result.State.ToString());
            }
            catch (System.Exception ex)
            {
                Services.Log.Error(ex.Message, null, "Push.SendAsync Error");
            }

            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Games/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteGame(string id)
        {
             return DeleteAsync(id);
        }

    }
}