using Microsoft.Extensions.DependencyInjection;
using StatusBoard.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatusBoard.AspNetCore.Demo.Checks
{
    public class CheckWithDependencyInjection : StatusCheckCore
    {
        public override string Name
        {
            get
            {
                return "ASP.NET Core - With dependency injection";
            }
        }
        public override Task<CheckResult> GetCurrentStatus(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<Services.HelloWorldService>();
            var serviceResult = service.GetHelloText();
            return Task.FromResult(CheckResult.ResultOk(serviceResult));
        }
    }
}
