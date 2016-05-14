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
        public static void WriteToOwinContext(this IOwinContext context, WebResponse webResponse)
        {
            context.Response.ContentType = webResponse.ContentType;
            context.Response.Write(webResponse.Content);
        }
    }
}
