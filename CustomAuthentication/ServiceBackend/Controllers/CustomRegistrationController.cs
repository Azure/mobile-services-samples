using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;

using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Text.RegularExpressions;
using CustomAuthMobileService.DataObjects;
using CustomAuthMobileService.Models;

using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;

//using SendGrid;

namespace CustomAuthMobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomRegistrationController : ApiController
    {
        public ApiServices Services { get; set; }
        
        //// TODO: Uncomment to use Azure Table storage.
        //private CloudTableClient tableClient;
        //private CloudTable accountTable;
        //protected override void Initialize(System.Web.Http.Controllers.HttpControllerContext controllerContext)
        //{
        //    base.Initialize(controllerContext);
        //    // Parse the Storage account connection string.
        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
        //        ConfigurationManager.ConnectionStrings["StorageConnectionString"].ToString());

        //    // Create a new table client and create the Account table if it doesn't exist.
        //    tableClient = storageAccount.CreateCloudTableClient();
        //    accountTable = tableClient.GetTableReference("account");
        //    accountTable.CreateIfNotExists();
        //}

        // POST api/CustomRegistration
        public HttpResponseMessage Post(RegistrationRequest registrationRequest)
        {
            if (!Regex.IsMatch(registrationRequest.username, "^[a-zA-Z0-9]{4,}$"))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, 
                    "Invalid username (at least 4 chars, alphanumeric only)");
            }
            else if (registrationRequest.password.Length < 8)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, 
                    "Invalid password (at least 8 chars required)");
            }
            else if (!registrationRequest.email.Contains("@"))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest,
                    "Please supply a valid email address");
            }

            // TODO: Comment out for Azure Table storage.
            // SQL Database version using Entity Framework for data access.
            MobileServiceContext context = new MobileServiceContext();
            Account account = context.Accounts.Where(a => a.Username == registrationRequest.username).SingleOrDefault();

            //// TODO: Azure Table storage version.
            //// Create a query for a specific username.
            //TableQuery<Account> query = new TableQuery<Account>().Where(
            //    TableQuery.GenerateFilterCondition("Username", QueryComparisons.Equal, 
            //    registrationRequest.username));

            //// Execute the query to retrieve the account.
            //Account account = accountTable.ExecuteQuery(query).FirstOrDefault();

           
            // If there's already a confirmed account, return an error response.
            if (account != null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, 
                    "That username already exists.");
            }
            else
            {
                byte[] salt = CustomLoginProviderUtils.generateSalt();
                Account newAccount = new Account
                {
                    //// TODO: Uncomment the below for Azure Table storage.
                    //PartitionKey = "partition",
                    //RowKey = Guid.NewGuid().ToString(),
                    Username = registrationRequest.username,
                    Salt = salt,
                    SaltedAndHashedPassword =
                    CustomLoginProviderUtils.hash(registrationRequest.password, salt),
                    IsConfirmed = true, // this should be false until confirmed.
                    email = registrationRequest.email,
                    friendlyName = registrationRequest.friendlyName
                };

                // TODO: Comment out for Azure Table storage.
                // SQL Database version.
                context.Accounts.Add(newAccount);
                context.SaveChanges();

                //// TODO: Azure Table storage version.
                //// Insert the new account into the table.                
                //accountTable.Execute(TableOperation.Insert(newAccount));

                // This is where you would kick-off the independent verification process, 
                // such as sending an email using SendGrid integration (http://aka.ms/busjrt).
                // https://github.com/sendgrid/sendgrid-csharp

                // Return the success response.
                return this.Request.CreateResponse(HttpStatusCode.Created);
            }
        }
    }   
}
