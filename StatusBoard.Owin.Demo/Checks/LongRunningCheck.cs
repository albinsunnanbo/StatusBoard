using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StatusBoard.Core;
using System.Threading.Tasks;

namespace StatusBoard.Owin.Demo.Checks
{
    public class LongRunningCheck : Core.StatusCheck
    {
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