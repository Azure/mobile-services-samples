using System.Web.Http;
using System.Web.Routing;

namespace blog20141010
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WebApiConfig.Register();
        }
    }
}