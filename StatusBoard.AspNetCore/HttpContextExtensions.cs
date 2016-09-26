using Microsoft.AspNetCore.Http;
using StatusBoard.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.AspNetCore
{
    public static class HttpContextExtensions
    {
        public static async Task WriteToHttpContext(this HttpContext context, WebResponse webResponse)
        {
            context.Response.ContentType = webResponse.ContentType;
            if (webResponse.HttpStatusCode.HasValue)
            {
                context.Response.StatusCode = webResponse.HttpStatusCode.Value;
            }
            await context.Response.WriteAsync(webResponse.Content);
        }
    }
}
