using MultiChannelToDo.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.OData;

namespace MultiChannelToDo.Controllers
{
    [EnableCors("*", "*", "*", SupportsCredentials = false)]
    public class ToDoItemsController : ApiController
    {
        private MultiChannelToDoContext db = new MultiChannelToDoContext();

        // GET: api/ToDoItems
        [EnableQuery]
        public IQueryable<TodoItem> GetToDoItems()
        {
            return db.ToDoItems;
        }

        // GET: api/ToDoItems/5
        [EnableQuery]
        [ResponseType(typeof(TodoItem))]
        public async Task<IHttpActionResult> GetToDoItem(string id)
        {
            TodoItem toDoItem = await db.ToDoItems.FindAsync(id);
            if (toDoItem == null)
            {
                return NotFound();
            }

            return Ok(toDoItem);
        }

        // PUT: api/ToDoItems/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutToDoItem(string id, TodoItem toDoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != toDoItem.Id)
            {
                return BadRequest();
            }

            db.Entry(toDoItem).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToDoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ToDoItems
        [ResponseType(typeof(TodoItem))]
        public async Task<IHttpActionResult> PostToDoItem(TodoItem toDoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ToDoItems.Add(toDoItem);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ToDoItemExists(toDoItem.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = toDoItem.Id }, toDoItem);
        }

        // DELETE: api/ToDoItems/5
        [ResponseType(typeof(TodoItem))]
        public async Task<IHttpActionResult> DeleteToDoItem(string id)
        {
            TodoItem toDoItem = await db.ToDoItems.FindAsync(id);
            if (toDoItem == null)
            {
                return NotFound();
            }

            db.ToDoItems.Remove(toDoItem);
            await db.SaveChangesAsync();

            return Ok(toDoItem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ToDoItemExists(string id)
        {
            return db.ToDoItems.Count(e => e.Id == id) > 0;
        }
    }
}