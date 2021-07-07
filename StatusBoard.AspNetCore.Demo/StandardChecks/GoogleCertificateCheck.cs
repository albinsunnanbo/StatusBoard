using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core.StandardChecks
{
    public class GoogleCertificateCheck : CertificateCheck
    {
        public GoogleCertificateCheck()
            : base(new Uri("https://google.com"))
        {
        }
    }
}
