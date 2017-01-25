﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MultiChannelToDo.Models
{
    public class MultiChannelToDoContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public MultiChannelToDoContext() : base("name=MultiChannelToDoContext")
        {
        }

        public System.Data.Entity.DbSet<MultiChannelToDo.Models.ToDoItem> ToDoItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("mobile_service_name"); // change schema to the name of your mobile service,
                                                                  // replacing dashes with underscore
                                                                  // mobile-service-name ==> mobile_service_name
            base.OnModelCreating(modelBuilder);
        }
    }
}
