using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;

using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Security.Claims;
using CustomAuthMobileService.DataObjects;
using CustomAuthMobileService.Models;

using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;

namespace CustomAuthMobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomLoginController : ApiController
    {
        public ApiServices Services { get; set; }
        public IServiceTokenHandler handler { get; set; }

        //// TODO: Uncomment to use Azure Table storage.
        //private CloudTableClient tableClient;
        //private CloudTable accountTable;

        protected override void Initialize(System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            //// TODO: Uncomment to use Azure Table storage.
            //// Parse the Storage account connection string.
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //    ConfigurationManager.ConnectionStrings["StorageConnectionString"].ToString());

            //// Create a new table client and create the Account table if it doesn't exist.
            //tableClient = storageAccount.CreateCloudTableClient();
            //accountTable = tableClient.GetTableReference("account");
            //accountTable.CreateIfNotExists();
        }

        // POST api/CustomLogin
        public HttpResponseMessage Post(LoginRequest loginRequest)
        {
            // TODO: Comment out for Azure Table storage.
            MobileServiceContext context = new MobileServiceContext();
            Account account = context.Accounts
                .Where(a => a.Username == loginRequest.username).SingleOrDefault();

            //// TODO: Uncomment to use Azure Table storage.
            //// Create a query for a specific username.
            //TableQuery<Account> query = new TableQuery<Account>().Where(
            //    TableQuery.GenerateFilterCondition("Username", QueryComparisons.Equal,
            //    loginRequest.username));

            //// Execute the query to retrieve the account.
            //Account account = accountTable.ExecuteQuery(query).SingleOrDefault();

            if (account != null)
            {
                if (!account.IsConfirmed)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest,
                        "You must first confim your account registration.");
                }

                byte[] incoming = CustomLoginProviderUtils
                    .hash(loginRequest.password, account.Salt);

                if (CustomLoginProviderUtils.slowEquals(incoming, account.SaltedAndHashedPassword))
                {
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, loginRequest.username));
                    LoginResult loginResult = new CustomLoginProvider(handler)
                        .CreateLoginResult(claimsIdentity, Services.Settings.MasterKey);
                    var customLoginResult = new CustomLoginResult()
                    {
                        UserId = loginResult.User.UserId,
                        MobileServiceAuthenticationToken = loginResult.AuthenticationToken
                    };
                    return this.Request.CreateResponse(HttpStatusCode.OK, customLoginResult);
                }
            }
            return this.Request.CreateResponse(HttpStatusCode.Unauthorized,
                "Invalid username or password");
        }
    }
}
