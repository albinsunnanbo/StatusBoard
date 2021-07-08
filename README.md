# StatusBoard
Status board framework for .NET Web Applications

Installation
------------
Install from NuGet: https://www.nuget.org/packages/ASP.NET-StatusBoard.Owin/

Configure in the Owin Startup
Optionally create an CheckErrorHandler for global error handling

	/// <summary>
	/// Configure owin StatusBoard.
	/// </summary>
	/// <param name="app">Owin app</param>
	private static void ConfigureStatusBoard(IAppBuilder app)
	{
		// Find all status available status checks
		// Scan the assembly where you have implemented your StatusChecks
		var statusChecks = StatusBoard.Core.Utilities.GetAllStatusChecksInAssembly(System.Reflection.Assembly.GetExecutingAssembly());
		var options = new Options(statusChecks);
		// Configure global error handler
		options.CheckErrorHandler = StatusBoardCheckErrorHandler;
		// Configure optional timeouts
		options.CheckAllFailOnErrorTimeout = TimeSpan.FromSeconds(1);
		app.UseStatusBoard(options);
	}

	/// <summary>
	/// Global StatusBoard error handler
	/// </summary>
	private static CheckResult StatusBoardCheckErrorHandler(StatusCheck check, Exception ex)
	{
		var errorId = LogUtility.LogException(ex, $"Check {check.CheckId} failed with exception {ex}");
		return CheckResult.ResultError($"An error occurred with LogId = {errorId}");
	}

Getting started
---------------
Once configured the StatusBoard is located on http://your-site.com/Status

Checkout the demo project on GitHub.
The demoproject can be tested on http://statusboard-demo.azurewebsites.net/

To create status checks, implement the abstract class `StatusCheck`.
Examples from the demo project
* Normal: https://github.com/albinsunnanbo/StatusBoard/blob/master/StatusBoard.Owin.Demo/Checks/AlwaysOkCheck.cs
* Async:  https://github.com/albinsunnanbo/StatusBoard/blob/master/StatusBoard.Owin.Demo/Checks/LongRunningCheck.cs
* Error:  https://github.com/albinsunnanbo/StatusBoard/blob/master/StatusBoard.Owin.Demo/Checks/AlwaysErrorCheck.cs
