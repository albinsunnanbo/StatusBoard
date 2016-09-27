using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StatusBoard.Core;
using System.Threading.Tasks;

namespace StatusBoard.Owin.Demo.Checks
{
    public class AlwaysWarningCheck : Core.StatusCheck
    {
        public override string Name
        {
            get
            {
                return "Always Warning";
            }
        }

        public override Task<CheckResult> GetCurrentStatus()
        {
            return Task.FromResult(CheckResult.ResultWarning("Warning message"));
        }
    }
}