using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using CustomAuthMobileService.DataObjects;
using CustomAuthMobileService.Models;


using System.Web.Http.OData.Query;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Mobile.Service.Security;

namespace CustomAuthMobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class TodoItemController : TableController<TodoItem>
    {
        // TODO: Uncomment to use Azure Table storage.
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            // TODO: Comment out for Azure Table storage.
            // SQL Database version using Entity Framework domain manager.
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request, Services);

            //// TODO: Uncomment to use Azure Table storage.
            //// Create a new Azure Storage domain manager using the stored 
            //// connection string and the name of the table exposed by the controller.
            //string connectionStringName = "StorageConnectionString";
            //var tableName = controllerContext.ControllerDescriptor.ControllerName.ToLowerInvariant();
            //DomainManager = new StorageDomainManager<TodoItem>(connectionStringName,
            //    tableName, Request, Services);
        }

        // TODO: Comment out for Azure Table storage.
        // GET tables/TodoItem
        public IQueryable<TodoItem> GetAllTodoItems()
        {
            return Query();
        }

        //// TODO: Uncomment to use Azure Table storage.
        //// GET tables/TodoItem
        //public Task<IEnumerable<TodoItem>> GetAllTodoItems(ODataQueryOptions options)
        //{
        //    // Call QueryAsync, passing the supplied query options.
        //    return DomainManager.QueryAsync(options);
        //}

        // TODO: Comment out for Azure Table storage.
        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TodoItem> GetTodoItem(string id)
        {

            return base.Lookup(id);
        }

        //// TODO: Uncomment to use Azure Table storage.
        //// GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        //public Task<SingleResult<TodoItem>> GetTodoItem(string id)
        //{
        //    return base.LookupAsync(id);
        //}

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
        {
            TodoItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTodoItem(string id)
        {
            return DeleteAsync(id);
        }
    }
}