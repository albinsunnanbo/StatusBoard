using StatusBoard.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.AspNetCore
{
    public abstract class StatusCheckCore : StatusCheck
    {
        public override sealed Task<CheckResult> GetCurrentStatus()
        {
            throw new NotImplementedException("StatusCheckCore must be invoked with the IServiceProvider argument.");
        }

        public abstract Task<CheckResult> GetCurrentStatus(IServiceProvider serviceProvider);
    }
}
