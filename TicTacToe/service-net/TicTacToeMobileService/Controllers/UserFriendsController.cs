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
    public class UserFriendsController : TableController<UserFriends>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            TicTacToeMobileServiceContext context = new TicTacToeMobileServiceContext();
            DomainManager = new EntityDomainManager<UserFriends>(context, Request, Services);
        }

        // GET tables/UserFriends
        public IQueryable<UserFriends> GetAllUserFriends()
        {
            return Query(); 
        }

        // GET tables/UserFriends/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<UserFriends> GetUserFriends(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/UserFriends/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<UserFriends> PatchUserFriends(string id, Delta<UserFriends> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/UserFriends/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostUserFriends(UserFriends item)
        {
            
            var existingUserFriendResult = this.Query().Where(userfriend =>
                item.User1Key == userfriend.User1Key && item.User2Key == userfriend.User2Key);
            if (existingUserFriendResult.Count() > 0)
            {
                return this.RedirectToRoute("Tables", new { id = item.Id });
            }
            else
            {
                UserFriends current = await InsertAsync(item);
                return CreatedAtRoute("Tables", new { id = current.Id }, current);
            }
        }

        // DELETE tables/UserFriends/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteUserFriends(string id)
        {
             return DeleteAsync(id);
        }

    }
}