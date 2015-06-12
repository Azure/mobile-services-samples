using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;

using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Security.Claims;
using MobileServiceTableStorage.DataObjects;
using MobileServiceTableStorage.Models;

namespace MobileServiceTableStorage.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomLoginController : ApiController
    {
        public ApiServices Services { get; set; }
        public IServiceTokenHandler handler { get; set; }

        // POST api/CustomLogin
        public HttpResponseMessage Post(LoginRequest loginRequest)
        {
            todoContext context = new todoContext();
            Account account = context.Accounts
                .Where(a => a.Username == loginRequest.username).SingleOrDefault();
            if (account != null)
            {
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
