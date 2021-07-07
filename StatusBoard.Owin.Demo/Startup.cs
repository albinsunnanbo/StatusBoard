using Microsoft.Owin;
using Owin;
using StatusBoard.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

[assembly: OwinStartupAttribute(typeof(StatusBoard.Owin.Demo.Startup))]
namespace StatusBoard.Owin.Demo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var checks = Utilities.GetAllStatusChecksInAssembly(typeof(Core.StandardChecks.HttpCheck).Assembly)
                    .Concat(
                Utilities.GetAllStatusChecksInAssembly(System.Reflection.Assembly.GetExecutingAssembly()));
            var proxies = new List<Proxy>
            {
                new Proxy
                {
                    Title = "Recursive proxy to self",
                    ProxyBaseUri = new Uri( ConfigurationManager.AppSettings["Proxy"]),
                }
            };
            app.UseStatusBoard(checks, proxies);
        }
    }
}