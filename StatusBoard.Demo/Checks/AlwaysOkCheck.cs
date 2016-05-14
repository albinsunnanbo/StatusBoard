using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StatusBoard.Core;

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

        public override CheckResult GetCurrentStatus()
        {
            return new CheckResult
            {
                 StatusValue = StatusValue.OK,
            };
        }
    }
}