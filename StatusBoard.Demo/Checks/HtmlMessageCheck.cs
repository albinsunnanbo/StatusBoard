using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StatusBoard.Core;
using System.Threading.Tasks;

namespace StatusBoard.Demo.Checks
{
    public class HtmlMessageCheck : Core.StatusCheck
    {
        public override string Name
        {
            get
            {
                return "HTML Message";
            }
        }

        public override Task<CheckResult> GetCurrentStatus()
        {
            return Task.FromResult(CheckResult.ResultOk("<h1>Larger than life!</h1>", true));
        }
    }
}