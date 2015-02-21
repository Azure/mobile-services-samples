namespace MultiChannelToDo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialState : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ToDoItems",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Text = c.String(),
                        Complete = c.Boolean(nullable: false),
                        CreatedAt = c.DateTimeOffset(precision: 7),
                        UpdatedAt = c.DateTimeOffset(precision: 7),
                        Version = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ToDoItems");
        }
    }
}
