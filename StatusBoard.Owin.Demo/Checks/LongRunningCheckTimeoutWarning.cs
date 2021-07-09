using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StatusBoard.Core;
using System.Threading.Tasks;

namespace StatusBoard.Owin.Demo.Checks
{
    public class LongRunningCheckTimeoutWarning : Core.StatusCheck
    {
        public LongRunningCheckTimeoutWarning()
        {
            Timeout = TimeSpan.FromSeconds(1);
            TimeoutErrorLevel = StatusValue.WARNING;
        }
        public override string Name
        {
            get
            {
                return "Always OK, after a while";
            }
        }

        public async override Task<CheckResult> GetCurrentStatus()
        {
            await Task.Delay(2500);
            return CheckResult.ResultOk("Worth waiting for?");
        }
    }
}