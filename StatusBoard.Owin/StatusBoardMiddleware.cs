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

        public StatusBoardMiddleware(OwinMiddleware next, Options options) : base(next)
        {
            this.options = options;
        }

        public async override Task Invoke(IOwinContext context)
        {
            PathString remainingLevel1;
            PathString remainingLevel2;
            PathString remainingLevel3;
            if (context.Request.Path.StartsWithSegments(new PathString("/Status"), out remainingLevel1))
            {
                if (!remainingLevel1.HasValue)
                {
                    var webResponse = options.GetStartPage();
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/js"), out remainingLevel2))
                {
                    context.WriteToOwinContext(options.GetJs());
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/css"), out remainingLevel2))
                {
                    context.WriteToOwinContext(options.GetCss());
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/jQuery"), out remainingLevel2))
                {
                    context.WriteToOwinContext(options.GetJquery());
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/Directory"), out remainingLevel2))
                {
                    var webResponse = options.GetDirectoryListing();
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/Proxy"), out remainingLevel2))
                {
                    if (remainingLevel2.HasValue)
                    {
                        var nextSlash = remainingLevel2.Value.IndexOf('/', 1);
                        var proxyId = int.Parse(remainingLevel2.Value.Substring(1, nextSlash - 1));
                        if (remainingLevel2.StartsWithSegments(new PathString("/" + proxyId), out remainingLevel3))
                        {
                            var proxyBaseUrl = options.GetProxyBaseUri(proxyId).AbsoluteUri.TrimEnd('/');
                            var proxyCombinedUrl = proxyBaseUrl + remainingLevel3.Value;
                            System.Net.WebClient wc = new System.Net.WebClient();
                            var result = await wc.DownloadStringTaskAsync(proxyCombinedUrl);
                            context.WriteToOwinContext(WebResponse.JsonResponse(result));
                            return;
                        }
                    }
                    else
                    {
                        var webResponse = options.GetProxyListing();
                        context.WriteToOwinContext(webResponse);
                        return;
                    }
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/Check"), out remainingLevel2))
                {
                    var checkId = remainingLevel2.Value.TrimStart('/');
                    var webResponse = await options.RunCheck(checkId);
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAll"), out remainingLevel2))
                {
                    var webResponse = await options.RunAllChecks();
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAllNoProxy"), out remainingLevel2))
                {
                    var webResponse = await options.RunAllChecks(checkProxies: false);
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAllFailOnWarning"), out remainingLevel2))
                {
                    var webResponse = await options.RunAllChecks(StatusValue.WARNING);
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAllFailOnError"), out remainingLevel2))
                {
                    var webResponse = await options.RunAllChecks(StatusValue.ERROR);
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                context.Response.Write("Invalid status request");
                return;
            }
            await Next.Invoke(context);
        }
    }
}
