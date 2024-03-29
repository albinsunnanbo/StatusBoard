﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core
{
    public class BackgroundWorker
    {
        public WebResponse CachedWebResponse { get; private set; } = new WebResponse("Not initialized", "text/plain", 500);
        
        public BackgroundWorker(Func<Task<WebResponse>> task, Func<string, Exception, CheckResult> errorHandler, TimeSpan interval)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        CachedWebResponse = task().Result;
                        sw.Stop();
                    }
                    catch (Exception ex)
                    {
                        CachedWebResponse = WebResponse.JsonResponse(errorHandler("Internal error in background check", ex), 500);
                    }

                    // Delay before starting new
                    System.Threading.Thread.Sleep(interval);
                }
            });
        }
    }
}
