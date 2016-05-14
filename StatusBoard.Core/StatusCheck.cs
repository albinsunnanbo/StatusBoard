using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core
{
    public abstract class StatusCheck
    {
        public abstract string Name { get; }

        public virtual string CheckId
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public abstract Task<CheckResult> GetCurrentStatus();
    }
}
