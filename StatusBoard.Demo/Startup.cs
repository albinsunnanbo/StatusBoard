using Microsoft.Owin;
using Owin;
using StatusBoard.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartupAttribute(typeof(StatusBoard.Demo.Startup))]
namespace StatusBoard.Demo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseStatusBoard(
                Utilities.GetAllStatusChecksInAssembly(typeof(Core.StandardChecks.HttpCheck).Assembly)
                    .Concat(
                Utilities.GetAllStatusChecksInAssembly(System.Reflection.Assembly.GetExecutingAssembly()))
                );
        }
    }
}