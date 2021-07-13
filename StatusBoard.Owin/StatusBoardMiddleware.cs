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
            var segments = context.Request.Uri.Segments;
            if (segments.Length >= 2 && segments[1].ToLower().TrimEnd('/') == "status")
            {
                var webResponse = await CreateWebResponse(segments);
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

        public async Task<WebResponse> CreateWebResponse(string[] segments)
        {
            WebResponse webResponse;
            if (segments.Length == 2)
            {
                webResponse = options.GetStartPage();
                return webResponse;
            }
            switch (segments[2].ToLower().TrimEnd('/'))
            {
                case "js":
                    return options.GetJs();
                case "css":
                    return options.GetCss();
                case "jquery":
                    return options.GetJquery();
                case "directory":
                    webResponse = options.GetDirectoryListing();
                    return webResponse;
                case "proxy":
                    if (segments.Length > 3)
                    {
                        var proxyCheckId = segments[3].TrimEnd('/');
                        var proxyId = int.Parse(proxyCheckId);
                        var proxyBaseUrl = options.GetProxyBaseUri(proxyId).AbsoluteUri.TrimEnd('/');
                        var proxyCombinedUrl = proxyBaseUrl + "/" + string.Join("/", segments.Skip(4).Select(s => s.TrimEnd('/')));
                        using (var wc = new System.Net.WebClient())
                        {
                            var result = await wc.DownloadStringTaskAsync(proxyCombinedUrl);
                            return WebResponse.JsonResponse(result);
                        }
                    }
                    else
                    {
                        webResponse = options.GetProxyListing();
                        return webResponse;
                    }
                case "check":
                    var checkId = segments[3].Trim('/');
                    webResponse = await options.RunCheck(checkId, options.CheckIndividualTimeout);
                    return webResponse;
                case "checkallnoproxy":
                    webResponse = await options.RunAllChecks(checkProxies: false, timeout: options.CheckAllNoProxyTimeout);
                    return webResponse;
                case "checkall":
                    webResponse = await options.RunAllChecks(timeout: options.CheckAllTimeout);
                    return webResponse;

                case "checkallfailonwarning":

                    webResponse = await options.RunAllChecks(StatusValue.WARNING, timeout: options.CheckAllFailOnWarningTimeout);
                    return webResponse;
                case "checkallfailonerror":

                    webResponse = await options.RunAllChecks(StatusValue.ERROR, timeout: options.CheckAllFailOnErrorTimeout);
                    return webResponse;
            }
            return null;
        }
    }
}
