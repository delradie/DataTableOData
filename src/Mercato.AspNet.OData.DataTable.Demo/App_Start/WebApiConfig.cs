using Microsoft.AspNet.OData.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Mercato.AspNet.OData.DataTableExtension.Demo
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.EnableDependencyInjection();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
