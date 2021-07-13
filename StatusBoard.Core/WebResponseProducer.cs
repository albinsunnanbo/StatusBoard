using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core
{
    public class WebResponseProducer
    {
        private readonly Options options;
        private readonly Func<StatusCheck, Task<CheckResult>> evaluator;

        public WebResponseProducer(Options options, Func<StatusCheck, Task<CheckResult>> evaluator)
        {
            this.options = options;
            this.evaluator = evaluator;
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
                    webResponse = await options.RunCheck(checkId, evaluator, options.CheckIndividualTimeout);
                    return webResponse;
                case "checkallnoproxy":
                    webResponse = await options.RunAllChecks(checkProxies: false, timeout: options.CheckAllNoProxyTimeout, evaluator: evaluator);
                    return webResponse;
                case "checkall":
                    webResponse = await options.RunAllChecks(timeout: options.CheckAllTimeout, evaluator: evaluator);
                    return webResponse;
                case "checkallfailonwarning":
                    webResponse = await options.RunAllChecks(StatusValue.WARNING, timeout: options.CheckAllFailOnWarningTimeout, evaluator: evaluator);
                    return webResponse;
                case "checkallfailonerror":
                    webResponse = await options.RunAllChecks(StatusValue.ERROR, timeout: options.CheckAllFailOnErrorTimeout, evaluator: evaluator);
                    return webResponse;
            }
            return null;
        }
    }
}
