using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace script_shell
{
    public class CoreService : IHostedService
    {
        protected string ScriptPath { get; set; } = string.Empty;

        private readonly string[] allowedExtensions = new string[] { ".ps1" };

        public CoreService(IConfiguration config)
        {
            ScriptPath = config.GetValue<string>(nameof(ScriptPath)) 
                ?? string.Empty;
        }

        protected async Task RunAsync(CancellationToken cancellationToken)
        {
            var files = Directory.EnumerateFiles(ScriptPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => allowedExtensions.Contains(Path.GetExtension(file)))
                .ToArray();

            foreach (var filePath in files)
            {
                await Console.Out.WriteLineAsync($"Starting script: {Path.GetFileName(filePath)}");
                await ExecuteScriptAsync(filePath);
            }

            await Console.Out.WriteLineAsync("Completed. Press enter to exit:");
            await Console.In.ReadLineAsync(cancellationToken);
        }

        private Task ExecuteScriptAsync(string filePath)
        {
            // Create a process to run PowerShell
            Process process = new();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = $"-ExecutionPolicy Bypass -File \"{filePath}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Event handler for capturing output
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

            // Start the process
            process.Start();

            // Begin capturing output
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for the process to exit
            process.WaitForExit();

            // Close the process
            process.Close();

            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if(cancellationToken.IsCancellationRequested)
                return;

            await Task.Run(async () =>
            {
                await RunAsync(cancellationToken);
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
