namespace MultiChannelToDo.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Models;

    internal sealed class Configuration : DbMigrationsConfiguration<MultiChannelToDo.Models.MultiChannelToDoContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(MultiChannelToDo.Models.MultiChannelToDoContext context)
        {
            context.ToDoItems.AddOrUpdate(
                i => i.Text,
                new ToDoItem { Id = "1", Text = "MultiChannel Demo", Complete = false, CreatedAt = DateTimeOffset.Now, UpdatedAt = DateTimeOffset.Now },
                new ToDoItem { Id= "2", Text = "Micro Service Demo", Complete=false, CreatedAt = DateTimeOffset.Now, UpdatedAt=DateTimeOffset.Now }
            );
        }
    }
}
