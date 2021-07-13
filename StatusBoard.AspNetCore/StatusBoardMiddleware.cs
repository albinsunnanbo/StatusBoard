using Microsoft.AspNetCore.Http;
using StatusBoard.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.AspNetCore
{
    public class StatusBoardMiddleware
    {
        readonly Core.Options options;
        readonly RequestDelegate next;
        readonly IServiceProvider serviceProvider;
        readonly WebResponseProducer webResponseProducer;

        public StatusBoardMiddleware(RequestDelegate next, Options options, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.next = next;
            this.options = options;
            this.webResponseProducer = new WebResponseProducer(options, EvaluateCoreCheck);
        }

        private async Task<CheckResult> EvaluateCoreCheck(StatusCheck check)
        {
            var coreCheck = check as StatusCheckCore;
            if (coreCheck != null)
            {
                return await coreCheck.GetCurrentStatus(serviceProvider);
            }
            return await check.GetCurrentStatus();
        }

        public async Task Invoke(HttpContext context)
        {
            var uri = new Uri("http://localhost" + context.Request.Path.Value); // Fake absolute path
            var segments = uri.Segments;
            if (segments.Length >= 2 && segments[1].ToLower().TrimEnd('/') == "status")
            {
                var webResponse = await webResponseProducer.CreateWebResponse(segments);
                if (webResponse != null)
                {
                    await context.WriteToHttpContext(webResponse);
                    return;
                }
                if (segments[1].ToLower().EndsWith("/"))
                {
                    context.Response.Redirect(context.Request.Path.Value.Substring(0, context.Request.Path.Value.Length - 1));
                    return;
                }
                await context.Response.WriteAsync($"Invalid status request '{segments[1]}'");
                return;
            }
            await next.Invoke(context);
        }
    }
}
