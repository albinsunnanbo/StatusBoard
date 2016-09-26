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

        public StatusBoardMiddleware(RequestDelegate next, Options options)
        {
            this.next = next;
            this.options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            PathString remainingLevel1;
            PathString remainingLevel2;
            if (context.Request.Path.StartsWithSegments(new PathString("/Status"), out remainingLevel1))
            {
                if (!remainingLevel1.HasValue)
                {
                    var webResponse = options.GetStartPage();
                    await context.WriteToHttpContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/js"), out remainingLevel2))
                {
                    await context.WriteToHttpContext(options.GetJs());
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/css"), out remainingLevel2))
                {
                    await context.WriteToHttpContext(options.GetCss());
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/jQuery"), out remainingLevel2))
                {
                    await context.WriteToHttpContext(options.GetJquery());
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/Directory"), out remainingLevel2))
                {
                    var webResponse = options.GetDirectoryListing();
                    await context.WriteToHttpContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/Check"), out remainingLevel2))
                {
                    var checkId = remainingLevel2.Value.TrimStart('/');
                    var webResponse = await options.RunCheck(checkId);
                    await context.WriteToHttpContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAll"), out remainingLevel2))
                {
                    var webResponse = await options.RunAllChecks();
                    await context.WriteToHttpContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAllFailOnWarning"), out remainingLevel2))
                {
                    var webResponse = await options.RunAllChecks(StatusValue.WARNING);
                    await context.WriteToHttpContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAllFailOnError"), out remainingLevel2))
                {
                    var webResponse = await options.RunAllChecks(StatusValue.ERROR);
                    await context.WriteToHttpContext(webResponse);
                    return;
                }
                await context.Response.WriteAsync("Invalid status request");
                return;
            }
            await next.Invoke(context);
        }
    }
}
