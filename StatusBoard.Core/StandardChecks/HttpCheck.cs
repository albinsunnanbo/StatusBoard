using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core.StandardChecks
{
    public abstract class HttpCheck : StatusCheck
    {
        private readonly string id;
        private readonly Uri uri;

        public HttpCheck(string id, Uri uri)
        {
            this.uri = uri;
            this.id = id;
        }

        public override string Name
        {
            get
            {
                return $"Download {uri}";
            }
        }
        public async override Task<CheckResult> GetCurrentStatus()
        {
            try
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                var result = await wc.DownloadStringTaskAsync(uri);
                return CheckResult.ResultOk();
            }
            catch (Exception)
            {
                return CheckResult.ResultError("Failed to download");
            }
        }
    }
}
