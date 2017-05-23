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

        public bool UseHtml { get; private set; }

        public CheckResult(StatusValue statusValue)
        {
            StatusValue = statusValue;
        }

        [JsonConstructor]
        public CheckResult(StatusValue statusValue, string message, bool useHtml = false) : this(statusValue)
        {
            Message = message;
            UseHtml = useHtml;
        }

        public static CheckResult ResultOk()
        {
            return new CheckResult(StatusValue.OK);
        }

        public static CheckResult ResultOk(string message, bool useHtml = false)
        {
            return new CheckResult(StatusValue.OK, message, useHtml);
        }

        public static CheckResult ResultWarning(string message, bool useHtml = false)
        {
            return new CheckResult(StatusValue.WARNING, message, useHtml);
        }

        public static CheckResult ResultError(string message, bool useHtml = false)
        {
            return new CheckResult(StatusValue.ERROR, message, useHtml);
        }
    }
}
