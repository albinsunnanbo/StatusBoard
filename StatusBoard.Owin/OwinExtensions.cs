using Microsoft.Owin;
using StatusBoard.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Owin
{
    public static class OwinExtensions
    {
        public static async Task WriteToOwinContext(this IOwinContext context, WebResponse webResponse)
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
