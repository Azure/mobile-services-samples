using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using blog20141010.DataObjects;
using blog20141010.Models;
using System.Web.Http.OData.Query;
using System.Collections.Generic;

namespace blog20141010.Controllers
{
    public class PersonController : TableController<Person>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            var tableName = controllerContext.ControllerDescriptor.ControllerName.ToLowerInvariant();
            var connStringName = "My_StorageConnectionString";
            DomainManager = new StorageDomainManager<Person>(connStringName, tableName, Request, Services);
        }

        // GET tables/Person
        public Task<IEnumerable<Person>> GetAllPerson(ODataQueryOptions queryOptions)
        {
            return base.QueryAsync(queryOptions); 
        }

        // GET tables/Person/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<SingleResult<Person>> GetPerson(string id)
        {
            return base.LookupAsync(id);
        }

        // PATCH tables/Person/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Person> PatchPerson(string id, Delta<Person> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Person/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostPerson(Person item)
        {
            Person current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Person/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeletePerson(string id)
        {
             return DeleteAsync(id);
        }

    }
}