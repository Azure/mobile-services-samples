namespace MultiChannelToDo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveVersion : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ToDoItems", "Version");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ToDoItems", "Version", c => c.Binary());
        }
    }
}
