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

        public WebResponse(string content, string contentType)
        {
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

        public static WebResponse JsonResponse(string content)
        {
            return new WebResponse(content, "application/json");
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
        
        public static WebResponse JsonResponse(object content)
        {
            var jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(content);
            return JsonResponse(jsonResponse);
        }
    }
}
