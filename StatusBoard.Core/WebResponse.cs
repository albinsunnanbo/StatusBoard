using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core
{
    public class WebResponse
    {
        readonly string content;
        readonly string contentType;
        readonly int? httpStatusCode;

        public WebResponse(string content, string contentType, int? httpStatusCode = null)
        {
            this.httpStatusCode = httpStatusCode;
            this.contentType = contentType;
            this.content = content;
        }

        public string Content
        {
            get
            {
                return content;
            }
        }

        public string ContentType
        {
            get
            {
                return contentType;
            }
        }

        public int? HttpStatusCode
        {
            get
            {
                return httpStatusCode;
            }
        }

        public static WebResponse JsonResponse(string content, int? httpStatusCode = null)
        {
            return new WebResponse(content, "application/json", httpStatusCode);
        }

        public static WebResponse HtmlResponse(string content)
        {
            return new WebResponse(content, "text/html; charset=utf-8");
        }

        public static WebResponse JavaScriptResponse(string content)
        {
            return new WebResponse(content, "application/javascript; charset=utf-8");
        }

        public static WebResponse CssResponse(string content)
        {
            return new WebResponse(content, "text/css; charset=utf-8");
        }

        public static WebResponse JsonResponse(object content, int? httpStatusCode = null)
        {
            var jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(content);
            return JsonResponse(jsonResponse, httpStatusCode);
        }
    }
}
