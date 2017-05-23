using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatusBoard.Owin;
using StatusBoard.Core;

namespace Owin
{
    public static class AppBuilderExtensions
    {
        public static void UseStatusBoard(this IAppBuilder app, IEnumerable<StatusCheck> checks)
        {
            app.UseStatusBoard(new Options(checks));
        }
        public static void UseStatusBoard(this IAppBuilder app, IEnumerable<StatusCheck> checks, IEnumerable<Proxy> proxies)
        {
            app.UseStatusBoard(new Options(checks, proxies));
        }

        public static void UseStatusBoard(
            this IAppBuilder app, Options options)
        {
            app.Use<StatusBoardMiddleware>(options);
        }
    }
}
