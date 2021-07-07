using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core.StandardChecks
{
    public class BadCertificateCheck : CertificateCheck
    {
        public BadCertificateCheck()
            : base(new Uri("https://self-signed.badssl.com/"))
        {
        }
    }
}
