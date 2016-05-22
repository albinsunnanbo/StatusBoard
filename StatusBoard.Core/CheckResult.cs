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
        public StatusValue StatusValue { get; private set; }

        public string Message { get; private set; }

        public CheckResult(StatusValue statusValue)
        {
            StatusValue = statusValue;
        }

        public CheckResult(StatusValue statusValue, string message) : this(statusValue)
        {
            Message = message;
        }

        public static CheckResult ResultOk()
        {
            return new CheckResult(StatusValue.OK);
        }

        public static CheckResult ResultOk(string message)
        {
            return new CheckResult(StatusValue.OK, message);
        }

        public static CheckResult ResultWarning(string message)
        {
            return new CheckResult(StatusValue.WARNING, message);
        }

        public static CheckResult ResultError(string message)
        {
            return new CheckResult(StatusValue.ERROR, message);
        }
    }
}
