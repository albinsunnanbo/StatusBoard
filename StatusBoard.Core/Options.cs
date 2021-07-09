using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core
{
    public class Options
    {
        readonly List<StatusCheck> checks;
        readonly List<Proxy> proxies;

        public string StatusPageHtml { get; set; } = Properties.Resources.StatusPage_html;
        public string StatusPageJs { get; set; } = Properties.Resources.StatusBoard_js;
        public string StatusPageCss { get; set; } = Properties.Resources.StatusBoard_css;
        public string StatusPageJquery { get; set; } = Properties.Resources.jquery_2_2_3_min;
        public Func<StatusCheck, Exception, CheckResult> CheckErrorHandler { get; set; } = DefaultCheckErrorHandler;
        public TimeSpan? CheckIndividualTimeout { get; set; } = null;
        public TimeSpan? CheckAllTimeout { get; set; } = null;
        public TimeSpan? CheckAllNoProxyTimeout { get; set; } = null;
        public TimeSpan? CheckAllFailOnWarningTimeout { get; set; } = null;
        public TimeSpan? CheckAllFailOnErrorTimeout { get; set; } = null;
        public bool RunCheckAllAsBackgroundWorker { get; set; }
        public bool RunCheckAllNoProxyAsBackgroundWorker { get; set; }
        public bool RunCheckAllFailOnWarningAsBackgroundWorker { get; set; }
        public bool RunCheckAllFailOnErrorAsBackgroundWorker { get; set; }
        public TimeSpan BackgroundworkerInterval { get; set; } = TimeSpan.FromSeconds(1);

        public Options(IEnumerable<StatusCheck> checks)
            : this(checks, Enumerable.Empty<Proxy>())
        {
        }
        public Options(IEnumerable<StatusCheck> checks, IEnumerable<Proxy> proxies)
        {
            this.checks = checks.ToList();
            this.proxies = proxies.ToList();
        }

        public WebResponse GetStartPage()
        {
            return WebResponse.HtmlResponse(StatusPageHtml);
        }
        public WebResponse GetJs()
        {
            return WebResponse.JavaScriptResponse(StatusPageJs);
        }
        public WebResponse GetCss()
        {
            return WebResponse.CssResponse(StatusPageCss);
        }
        public WebResponse GetJquery()
        {
            return WebResponse.JavaScriptResponse(StatusPageJquery);
        }

        private static CheckResult DefaultCheckErrorHandler(StatusCheck check, Exception ex)
        {
            // If you copy'n paste this code, this is where you insert your custom logger!
            System.Diagnostics.Trace.WriteLine($"Check {check.CheckId} failed with exception {ex}");

            return CheckResult.ResultError("Oopsie daisy. That one didn't go as planned.");
        }

        public WebResponse GetDirectoryListing()
        {
            var response = new
            {
                CurrentTime = DateTime.Now.ToString("u"),
                Checks = checks,
            };
            return WebResponse.JsonResponse(response);
        }

        public WebResponse GetProxyListing()
        {
            var response = new
            {
                CurrentTime = DateTime.Now.ToString("u"),
                Proxies = proxies.Select(p => new
                {
                    p.Title,
                }),
            };
            return WebResponse.JsonResponse(response);
        }

        public Uri GetProxyBaseUri(int proxyId)
        {
            return proxies[proxyId].ProxyBaseUri;
        }

        public async Task<WebResponse> RunCheck(string checkId, TimeSpan? timeout)
        {
            return await RunCheck(checkId, check => check.GetCurrentStatus(), timeout);
        }
        public async Task<WebResponse> RunCheck(string checkId, Func<StatusCheck, Task<CheckResult>> evaluator, TimeSpan? timeout)
        {
            var check = checks.SingleOrDefault(c => c.CheckId == checkId);
            if (check == null)
            {
                throw new ArgumentException($"Check id {checkId} does not exist.", nameof(checkId));
            }
            var timer = Stopwatch.StartNew();
            CheckResult checkResult = await RunOneCheck(check, evaluator, timeout);
            timer.Stop();
            var response = new
            {
                CurrentTime = DateTime.Now.ToString("u"),
                CheckResult = checkResult,
                Duration = $"{Math.Round(timer.Elapsed.TotalMilliseconds, 0)} ms",
            };
            return WebResponse.JsonResponse(response);
        }

        private async Task<CheckResult> RunOneCheck(StatusCheck check, Func<StatusCheck, Task<CheckResult>> evaluator, TimeSpan? timeout)
        {
            CheckResult checkResult;
            try
            {
                var timeoutCombined = check.Timeout ?? timeout;
                var task = evaluator(check);
                var tasks = new Task[] { task };
                var sw = System.Diagnostics.Stopwatch.StartNew();
                if (timeoutCombined.HasValue)
                {
                    tasks = new[] { task, Task.Delay(timeoutCombined.Value) };
                }
                if (await Task.WhenAny(tasks) == task) // Timeout pattern
                {
                    // Success / fail from base class
                    return task.Result;
                }
                else
                {
                    task.ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.Faulted)
                        {
                            DefaultCheckErrorHandler(check, t.Exception);
                        }
                        else
                        {
                            DefaultCheckErrorHandler(check, new TimeoutException($"Statuscheck '{check.Name}' finally completed with status {t.Status.ToString()} after {sw.Elapsed}"));
                        }
                    });
                    // Fail
                    //Logging.LogHelper.LogError($"Certcheck timeout {Name}", nameof(CertCheck));
                    DefaultCheckErrorHandler(check, new TimeoutException($"Statuscheck '{check.Name}' did not complete within time limit {timeoutCombined.Value}"));

                    return new CheckResult(check.TimeoutErrorLevel, "Statuscheck timeout " + check.Name);
                }
            }
            catch (Exception ex)
            {
                checkResult = CheckErrorHandler(check, ex);
            }

            return checkResult;
        }

        public async Task<WebResponse> RunAllChecks(StatusValue? failLevel = null, Func<StatusCheck, Task<CheckResult>> evaluator = null, bool checkProxies = true, TimeSpan? timeout = null)
        {
            if (evaluator == null)
            {
                evaluator = check => check.GetCurrentStatus();
            }
            var timer = Stopwatch.StartNew();
            var allAsyncChecks = checks.Select(check => RunOneCheck(check, evaluator, timeout));
            var checkResults = (await Task.WhenAll(allAsyncChecks));
            var statusValues = checkResults.Select(r => r.StatusValue);
            var message = string.Join(", ", statusValues.GroupBy(r => r).OrderByDescending(g => g.Key).Select(g => $"{g.Key} = {g.Count()}"));
            if (checkProxies)
            {
                foreach (var proxy in proxies)
                {
                    var url = proxy.ProxyBaseUri + "/CheckAllNoProxy";
                    using (var wc = new System.Net.WebClient())
                    {
                        var result = await wc.DownloadStringTaskAsync(url);
                        var allChecksResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AllChecksResult>(result);
                        statusValues = statusValues.Concat(new[] { allChecksResult.CheckResult.StatusValue });
                        message += ", " + proxy.Title + ": " + allChecksResult.CheckResult.Message;
                    }
                }
            }
            var worstResult = statusValues.Max();
            timer.Stop();

            int httpStatusCode = 200;
            if (failLevel.HasValue && worstResult >= failLevel)
            {
                httpStatusCode = 500;
            }

            return WebResponse.JsonResponse(new AllChecksResult
            {
                CurrentTime = DateTime.Now.ToString("u"),
                CheckResult = new CheckResult(worstResult, message),
                Duration = $"{Math.Round(timer.Elapsed.TotalMilliseconds, 0)} ms",
            },
            httpStatusCode
            );
        }
    }
}
