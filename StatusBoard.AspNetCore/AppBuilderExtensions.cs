﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
        public static void UseStatusBoard(this IApplicationBuilder app, IEnumerable<StatusCheck> checks, IEnumerable<Proxy> proxies)
        {
            app.UseStatusBoard(new Options(checks, proxies));
        }

        public static IApplicationBuilder UseStatusBoard(this IApplicationBuilder builder, Options options)
        {
            return builder.UseMiddleware<StatusBoardMiddleware>(options, builder.ApplicationServices);
        }
    }
}