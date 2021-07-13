using Microsoft.Owin;
using StatusBoard.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Owin
{
    public class StatusBoardMiddleware : OwinMiddleware
    {
        readonly Core.Options options;
        readonly WebResponseProducer webResponseProducer;

        public StatusBoardMiddleware(OwinMiddleware next, Options options) : base(next)
        {
            this.options = options;
            this.webResponseProducer = new WebResponseProducer(options, null);
        }

        public async override Task Invoke(IOwinContext context)
        {
            var segments = context.Request.Uri.Segments;
            if (segments.Length >= 2 && segments[1].ToLower().TrimEnd('/') == "status")
            {
                var webResponse = await webResponseProducer.CreateWebResponse(segments);
                if (webResponse != null)
                {
                    await context.WriteToOwinContext(webResponse);
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
            await Next.Invoke(context);
        }

    }
}
