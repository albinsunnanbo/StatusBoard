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
        private readonly Core.Options options;
        private readonly BackgroundWorker checkAllNoProxyBackgroundWorker;
        private readonly BackgroundWorker checkAllBackgroundWorker;
        private readonly BackgroundWorker checkAllFailOnWarningBackgroundWorker;
        private readonly BackgroundWorker checkAllFailOnErrorBackgroundWorker;

        private class DummyCheck : StatusCheck
        {
            private readonly string message;

            public override string Name => "DummyCheck";
            public DummyCheck(string message)
            {
                this.message = message;
            }

            public override Task<CheckResult> GetCurrentStatus()
            {
                return Task.FromResult(CheckResult.ResultError(message));
            }
        }



        public StatusBoardMiddleware(OwinMiddleware next, Options options) : base(next)
        {
            this.options = options;


            if (options.RunCheckAllNoProxyAsBackgroundWorker)
            {
                checkAllNoProxyBackgroundWorker = new BackgroundWorker(
                    () => options.RunAllChecks(checkProxies: false, timeout: options.CheckAllNoProxyTimeout),
                    (string err, Exception ex) => options.CheckErrorHandler(new DummyCheck(err), ex),
                    TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)
                    );
            }
            if (options.RunCheckAllAsBackgroundWorker)
            {
                checkAllBackgroundWorker = new BackgroundWorker(
                    () => options.RunAllChecks(timeout: options.CheckAllTimeout),
                    (string err, Exception ex) => options.CheckErrorHandler(new DummyCheck(err), ex),
                    TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)
                    );
            }
            if (options.RunCheckAllFailOnWarningAsBackgroundWorker)
            {
                checkAllFailOnWarningBackgroundWorker = new BackgroundWorker(
                    () => options.RunAllChecks(StatusValue.WARNING, timeout: options.CheckAllFailOnWarningTimeout),
                    (string err, Exception ex) => options.CheckErrorHandler(new DummyCheck(err), ex),
                    TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)
                    );
            }
            if (options.RunCheckAllFailOnErrorAsBackgroundWorker)
            {
                checkAllFailOnErrorBackgroundWorker = new BackgroundWorker(
                    () => options.RunAllChecks(StatusValue.ERROR, timeout: options.CheckAllFailOnErrorTimeout),
                    (string err, Exception ex) => options.CheckErrorHandler(new DummyCheck(err), ex),
                    TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)
                    );
            }
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
                            using (var wc = new System.Net.WebClient())
                            {
                                var result = await wc.DownloadStringTaskAsync(proxyCombinedUrl);
                                context.WriteToOwinContext(WebResponse.JsonResponse(result));
                            }
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
                    var webResponse = await options.RunCheck(checkId, options.CheckIndividualTimeout);
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAllNoProxy"), out remainingLevel2))
                {
                    WebResponse webResponse;
                    if (options.RunCheckAllNoProxyAsBackgroundWorker)
                    {
                        webResponse = checkAllNoProxyBackgroundWorker.CachedWebResponse;
                    }
                    else
                    {
                        webResponse = await options.RunAllChecks(checkProxies: false, timeout: options.CheckAllNoProxyTimeout);
                    }
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAll"), out remainingLevel2))
                {
                    WebResponse webResponse;
                    if (options.RunCheckAllAsBackgroundWorker)
                    {
                        webResponse = checkAllBackgroundWorker.CachedWebResponse;
                    }
                    else
                    {
                        webResponse = await options.RunAllChecks(timeout: options.CheckAllTimeout);
                    }
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAllFailOnWarning"), out remainingLevel2))
                {
                    WebResponse webResponse;
                    if (options.RunCheckAllFailOnWarningAsBackgroundWorker)
                    {
                        webResponse = checkAllFailOnWarningBackgroundWorker.CachedWebResponse;
                    }
                    else
                    {
                        webResponse = await options.RunAllChecks(StatusValue.WARNING, timeout: options.CheckAllFailOnWarningTimeout);
                    }
                    context.WriteToOwinContext(webResponse);
                    return;
                }
                if (remainingLevel1.StartsWithSegments(new PathString("/CheckAllFailOnError"), out remainingLevel2))
                {
                    WebResponse webResponse;
                    if (options.RunCheckAllFailOnErrorAsBackgroundWorker)
                    {
                        webResponse = checkAllFailOnErrorBackgroundWorker.CachedWebResponse;
                    }
                    else
                    {
                        webResponse = await options.RunAllChecks(StatusValue.ERROR, timeout: options.CheckAllFailOnWarningTimeout);
                    }
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
