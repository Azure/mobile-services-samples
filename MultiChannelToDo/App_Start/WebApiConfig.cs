using MultiChannelToDo.Models;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using MultiChannelToDo.Models;
using System.Web.Http;

namespace MultiChannelToDo
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional }
            );
        }
    }
}
