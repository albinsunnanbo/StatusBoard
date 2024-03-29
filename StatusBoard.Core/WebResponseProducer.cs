﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core
{
    public class WebResponseProducer
    {
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

        private readonly Options options;
        private readonly Func<StatusCheck, Task<CheckResult>> evaluator;
        private readonly BackgroundWorker checkAllNoProxyBackgroundWorker;
        private readonly BackgroundWorker checkAllBackgroundWorker;
        private readonly BackgroundWorker checkAllFailOnWarningBackgroundWorker;
        private readonly BackgroundWorker checkAllFailOnErrorBackgroundWorker;

        public WebResponseProducer(Options options, Func<StatusCheck, Task<CheckResult>> evaluator)
        {
            this.options = options;
            this.evaluator = evaluator ?? (c => c.GetCurrentStatus());

            if (options.RunCheckAllNoProxyAsBackgroundWorker)
            {
                checkAllNoProxyBackgroundWorker = InitBackgroundWorker(options, checkProxies: false, timeout: options.CheckAllNoProxyTimeout, evaluator: this.evaluator);
            }
            if (options.RunCheckAllAsBackgroundWorker)
            {
                checkAllBackgroundWorker = InitBackgroundWorker(options, timeout: options.CheckAllTimeout, evaluator: this.evaluator);
            }
            if (options.RunCheckAllFailOnWarningAsBackgroundWorker)
            {
                checkAllFailOnWarningBackgroundWorker = InitBackgroundWorker(options, StatusValue.WARNING, timeout: options.CheckAllFailOnWarningTimeout, evaluator: this.evaluator);
            }
            if (options.RunCheckAllFailOnErrorAsBackgroundWorker)
            {
                checkAllFailOnErrorBackgroundWorker = InitBackgroundWorker(options, StatusValue.ERROR, timeout: options.CheckAllFailOnErrorTimeout, evaluator: this.evaluator);
            }
        }

        private static BackgroundWorker InitBackgroundWorker(Options options, StatusValue? failLevel = null, Func<StatusCheck, Task<CheckResult>> evaluator = null, bool checkProxies = true, TimeSpan? timeout = null)
        {
            return new BackgroundWorker(
                                () => options.RunAllChecks(failLevel, evaluator, checkProxies, timeout),
                                (string err, Exception ex) => options.CheckErrorHandler(new DummyCheck(err), ex),
                                options.BackgroundworkerInterval
                                );
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
                    if (options.RunCheckAllNoProxyAsBackgroundWorker)
                    {
                        webResponse = checkAllNoProxyBackgroundWorker.CachedWebResponse;
                    }
                    else
                    {
                        webResponse = await options.RunAllChecks(checkProxies: false, timeout: options.CheckAllNoProxyTimeout, evaluator: evaluator);
                    }
                    return webResponse;
                case "checkall":
                    if (options.RunCheckAllAsBackgroundWorker)
                    {
                        webResponse = checkAllBackgroundWorker.CachedWebResponse;
                    }
                    else
                    {
                        webResponse = await options.RunAllChecks(timeout: options.CheckAllTimeout, evaluator: evaluator);
                    }
                    return webResponse;
                case "checkallfailonwarning":
                    if (options.RunCheckAllFailOnWarningAsBackgroundWorker)
                    {
                        webResponse = checkAllFailOnWarningBackgroundWorker.CachedWebResponse;
                    }
                    else
                    {
                        webResponse = await options.RunAllChecks(StatusValue.WARNING, timeout: options.CheckAllFailOnWarningTimeout, evaluator: evaluator);
                    }
                    return webResponse;
                case "checkallfailonerror":
                    if (options.RunCheckAllFailOnErrorAsBackgroundWorker)
                    {
                        webResponse = checkAllFailOnErrorBackgroundWorker.CachedWebResponse;
                    }
                    else
                    {
                        webResponse = await options.RunAllChecks(StatusValue.ERROR, timeout: options.CheckAllFailOnErrorTimeout, evaluator: evaluator);
                    }
                    return webResponse;
            }
            return null;
        }
    }
}
