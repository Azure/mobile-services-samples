using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Web.Http;
using TicTacToeMobileService.DataObjects;
using TicTacToeMobileService.Models;
using Microsoft.WindowsAzure.Mobile.Service;

namespace TicTacToeMobileService
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            // config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            Database.SetInitializer(new TicTacToeMobileServiceInitializer(ServiceSettingsDictionary.GetSchemaName()));
        }
    }

    public class TicTacToeMobileServiceInitializer : ClearDatabaseSchemaIfModelChanges<TicTacToeMobileServiceContext>
    {

  
        public TicTacToeMobileServiceInitializer(string schemaName)
            : base()
        {

        }

        protected override void Seed(TicTacToeMobileServiceContext context)
        {
            List<User> users = new List<User>
        {
            new User { Id = "1", UserName = "User1" },
            new User { Id = "2", UserName = "User2" }
        };

            foreach (User user in users)
            {
                context.Set<User>().Add(user);
            }

            base.Seed(context);
        }
    }

}

