using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StatusBoard.Core;
using System.Threading.Tasks;

namespace StatusBoard.Demo.Checks
{
    public class AlwaysOkCheck : Core.StatusCheck
    {
        public override string Name
        {
            get
            {
                return "Always OK";
            }
        }

        public override Task<CheckResult> GetCurrentStatus()
        {
            return Task.FromResult(CheckResult.ResultOk());
        }
    }
}