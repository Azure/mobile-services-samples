using System.Net.Http.Formatting;
using System.Web.Http;

namespace MultiChannelToDo
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var config = GlobalConfiguration.Configuration.Formatters;
            config.Clear();
            config.Add(new JsonMediaTypeFormatter());
        }
    }
}
