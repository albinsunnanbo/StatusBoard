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
        protected readonly Uri uri;
        protected bool UseDefaultCredentials { get; set; } = false;

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
        private string chainErrors = "";
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

            foreach (X509ChainStatus chainStatus in chain.ChainStatus)
            {
                chainErrors += string.Format("Chain error: {0} {1}", chainStatus.Status, chainStatus.StatusInformation) + Environment.NewLine;
            }

            return true; // Always return true at this stage, inspect sslPolicyErrors later
        }

        public async override Task<CheckResult> GetCurrentStatus()
        {
            //Do webrequest to get info on secure site
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UseDefaultCredentials = UseDefaultCredentials;
            request.ServerCertificateValidationCallback = ValidateServerCertificate;

            using (var response = await request.GetResponseAsync())
            {
                var peakContent = response.GetResponseStream().ReadByte();

                if (_sslPolicyErrors != SslPolicyErrors.None)
                {
                    return CheckResult.ResultError($"Certificate is not valid with policy errors {_sslPolicyErrors}. ChainErrors: {chainErrors}, Expiry date: {expiryDate}");
                }

                if (!isValid)
                {
                    return CheckResult.ResultError($"Certificate is not valid. ChainErrors: {chainErrors}, Expiry date: {expiryDate}");
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
