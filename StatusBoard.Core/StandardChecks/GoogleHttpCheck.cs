using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core.StandardChecks
{
    public class GoogleHttpCheck : HttpCheck
    {
        public GoogleHttpCheck()
            : base("GoogleHttpCheck", new Uri("http://google.com"))
        {
        }
    }
}
