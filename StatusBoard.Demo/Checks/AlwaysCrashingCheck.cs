using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StatusBoard.Core;
using System.Threading.Tasks;

namespace StatusBoard.Demo.Checks
{
    public class AlwaysCrashingCheck : Core.StatusCheck
    {
        public override string Name
        {
            get
            {
                return "Always Crashing";
            }
        }

        public override Task<CheckResult> GetCurrentStatus()
        {
            throw new InvalidOperationException("Bam");
        }
    }
}