using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core
{
    public class CheckResult
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusValue StatusValue { get; set; }

        public string Message { get; set; }
    }
}
