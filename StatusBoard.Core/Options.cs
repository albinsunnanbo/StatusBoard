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

        public WebResponse GetDirectoryListing()
        {
            var response = new
            {
                CurrentTime = DateTime.Now.ToString("u"),
                Checks = checks,
            };
            return WebResponse.JsonResponse(response);
        }

        public WebResponse RunCheck(string checkId)
        {
            var check = checks.SingleOrDefault(c => c.CheckId == checkId);
            if (check == null)
            {
                throw new ArgumentException($"Check id {checkId} does not exist.", nameof(checkId));
            }
            var checkResult = check.GetCurrentStatus();
            var response = new
            {
                CurrentTime = DateTime.Now.ToString("u"),
                CheckResult = checkResult,
            };
            return WebResponse.JsonResponse(response);
        }

        public WebResponse RunAllChecks()
        {
            var checkResults = checks.Select(check => check.GetCurrentStatus().StatusValue).ToList();
            var worstResult = checkResults.Max();
            var message = string.Join(", ", checkResults.GroupBy(r => r).OrderByDescending(g => g.Key).Select(g => $"{g.Key} = {g.Count()}"));
            return WebResponse.JsonResponse(new
            {
                CurrentTime = DateTime.Now.ToString("u"),
                CheckResult = new CheckResult
                {
                    StatusValue = worstResult,
                    Message = message,
                },
            }
            );
        }
    }
}
