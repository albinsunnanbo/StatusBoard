using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StatusBoard.Core;

namespace StatusBoard.Demo.Checks
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

        public override CheckResult GetCurrentStatus()
        {
            System.Threading.Thread.Sleep(2500);
            return new CheckResult
            {
                StatusValue = StatusValue.OK,
                Message = "Worth waiting for?",
            };
        }
    }
}