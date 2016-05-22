using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StatusBoard.Core;
using System.Threading.Tasks;

namespace StatusBoard.Demo.Checks
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
            return Task.Run(() => CheckResult.ResultWarning("Warning message"));
        }
    }
}