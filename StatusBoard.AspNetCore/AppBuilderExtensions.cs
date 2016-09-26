using Microsoft.AspNetCore.Builder;
using StatusBoard.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.AspNetCore
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseStatusBoard(this IApplicationBuilder builder, IEnumerable<StatusCheck> checks)
        {
            return builder.UseStatusBoard(new Options(checks));
        }

        public static IApplicationBuilder UseStatusBoard(this IApplicationBuilder builder, Options options)
        {
            return builder.UseMiddleware<StatusBoardMiddleware>(options);
        }
    }
}
