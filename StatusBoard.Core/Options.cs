using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core
{
    public class Options
    {
        readonly List<StatusCheck> checks;

        public string StatusPageHtml { get; set; } = Properties.Resources.StatusPage_html;
        public string StatusPageJs { get; set; } = Properties.Resources.StatusBoard_js;
        public string StatusPageCss { get; set; } = Properties.Resources.StatusBoard_css;
        public string StatusPageJquery { get; set; } = Properties.Resources.jquery_2_2_3_min;
        public Func<StatusCheck, Exception, CheckResult> CheckErrorHandler { get; set; } = DefaultCheckErrorHandler;

        public Options(IEnumerable<StatusCheck> checks)
        {
            this.checks = checks.ToList();
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
            System.Diagnostics.Trace.WriteLine($"Check {check.CheckId} failed with exeption {ex}");

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

        public async Task<WebResponse> RunCheck(string checkId)
        {
            var check = checks.SingleOrDefault(c => c.CheckId == checkId);
            if (check == null)
            {
                throw new ArgumentException($"Check id {checkId} does not exist.", nameof(checkId));
            }
            CheckResult checkResult = await RunOneCheck(check);
            var response = new
            {
                CurrentTime = DateTime.Now.ToString("u"),
                CheckResult = checkResult,
            };
            return WebResponse.JsonResponse(response);
        }

        private async Task<CheckResult> RunOneCheck(StatusCheck check)
        {
            CheckResult checkResult;
            try
            {
                checkResult = await check.GetCurrentStatus();
            }
            catch (Exception ex)
            {
                checkResult = CheckErrorHandler(check, ex);
            }

            return checkResult;
        }

        public async Task<WebResponse> RunAllChecks(StatusValue? failLevel = null)
        {
            var allAsyncChecks = checks.Select(check => RunOneCheck(check));
            var checkResults = (await Task.WhenAll(allAsyncChecks));
            var statusValues = checkResults.Select(r => r.StatusValue);
            var worstResult = statusValues.Max();
            var message = string.Join(", ", statusValues.GroupBy(r => r).OrderByDescending(g => g.Key).Select(g => $"{g.Key} = {g.Count()}"));

            int httpStatusCode = 200;
            if (failLevel.HasValue && worstResult >= failLevel)
            {
                httpStatusCode = 500;
            }

            return WebResponse.JsonResponse(new
            {
                CurrentTime = DateTime.Now.ToString("u"),
                CheckResult = new CheckResult(worstResult, message),
            },
            httpStatusCode
            );
        }
    }
}
