using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core.StandardChecks
{
    public abstract class CertificateCheck : StatusCheck
    {
        private readonly Uri uri;

        public CertificateCheck(Uri uri)
        {
            this.uri = uri;
        }

        public override string Name
        {
            get
            {
                return $"Certificate for {uri}";
            }
        }

        private SslPolicyErrors _sslPolicyErrors;
        private X509Certificate _certificate;
        private bool isValid;
        private string expiryDate;
        private bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            this._sslPolicyErrors = sslPolicyErrors;
            this._certificate = certificate;

            X509Certificate2 cert2 = new X509Certificate2(_certificate);

            isValid = cert2.Verify();
            expiryDate = cert2.GetExpirationDateString();

            return true; // Always return true at this stage, inspect sslPolicyErrors later
        }

        public async override Task<CheckResult> GetCurrentStatus()
        {
            //Do webrequest to get info on secure site
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.ServerCertificateValidationCallback = ValidateServerCertificate;

            using (var response = await request.GetResponseAsync())
            {
                var peakContent = response.GetResponseStream().ReadByte();

                if (_sslPolicyErrors != SslPolicyErrors.None)
                {
                    return CheckResult.ResultError($"Certificate is not valid with policy errors {_sslPolicyErrors}, Expiry date: {expiryDate}");
                }

                ////convert the X509Certificate to an X509Certificate2 object by passing it into the constructor
                //X509Certificate2 cert2 = new X509Certificate2(_certificate);

                //var isValid = cert2.Verify();
                if (!isValid)
                {
                    return CheckResult.ResultError($"Certificate is not valid, Expiry date: {expiryDate}");
                }

                var date = DateTime.Parse(expiryDate);
                if (date < DateTime.Today.AddMonths(1))
                {
                    return CheckResult.ResultWarning($"Expiry date (less than one month left): {expiryDate}");
                }

                return CheckResult.ResultOk($"Expiry date: {expiryDate}");
            }
        }
    }
}
