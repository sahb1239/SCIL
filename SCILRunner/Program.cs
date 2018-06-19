using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCILRunner.Model;

namespace SCILRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ConsoleOptions>(args)
                .WithParsed<ConsoleOptions>(RunOptionsAndReturnExitCode)
                .WithNotParsed(error => { });
            Console.ReadKey();
        }

        private static void RunOptionsAndReturnExitCode(ConsoleOptions opts)
        {
            Run(opts).GetAwaiter().GetResult();
        }

        private static async Task Run(ConsoleOptions opts)
        {
            // Configure services
            var services = SetupServices().BuildServiceProvider();

            // Get database context
            var context = services.GetRequiredService<DataContext>();
            context.Database.EnsureCreated();
            
            // Get all files
            var files = Directory.GetFiles(opts.InputPath, "*.apk", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                // Check if file is already processed
                if (await context.Set<Scan>().AnyAsync(scan => scan.FilePath == file && scan.Finished))
                {
                    Console.WriteLine($"[Skipping]: {file}");
                    continue;
                }

                Console.WriteLine($"[Scanning]: {file}");

                // Get file info
                var fileInfo = new FileInfo(file);

                // Create output path
                var outputPath = Path.Combine(opts.OutputPath, fileInfo.Name);
                Directory.CreateDirectory(outputPath);

                // Create scan entry in db (or create new)
                Scan dbScan = await context.Set<Scan>().FirstOrDefaultAsync(scan => scan.FilePath == file);
                if (dbScan == null)
                {
                    dbScan = new Scan
                    {
                        FilePath = file,
                        OutputPath = outputPath,
                        Status = ScanStatus.Started,
                        Finished = false,
                        StartTime = DateTime.Now
                    };
                    context.Add(dbScan);
                    await context.SaveChangesAsync();
                }

                // Create cancellation token source and set timeout
                var cancellationTokenSource = new CancellationTokenSource();
                if (opts.Timeout.HasValue)
                {
                    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(opts.Timeout.Value));
                }

                // Get token
                var cancellationToken = cancellationTokenSource.Token;

                // Process file
                await ProcessFile(file, outputPath, opts.SCILPath, opts.Args, context, dbScan, cancellationToken);
            }
        }

        private static async Task ProcessFile(string file, string outputPath, string SCILPath, string args, DataContext dataContext, Scan scan, CancellationToken token)
        {
            // Set scan entry to starting
            scan.Status = ScanStatus.Started;
            scan.StartTime = DateTime.Now;
            dataContext.Update(scan);
            await dataContext.SaveChangesAsync(token);
            
            // Create semaphore to support exiting process
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

            // Add process start info
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"{SCILPath} --InputFile \"{file}\" --OutputPath \"{outputPath}\" {args}",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Start the process
            Process process = Process.Start(processInfo);

            // Add list of managed processes
            List<Process> managedProcesses = new List<Process>();

            // Attach handlers for process
            List<string> processOutput = new List<string>();
            process.OutputDataReceived += (sender, eventArgs) =>
            {
                processOutput.Add(eventArgs.Data);
                Console.WriteLine(eventArgs.Data);

                if (eventArgs.Data?.StartsWith("FlixPID:") ?? false)
                {
                    if (int.TryParse(eventArgs.Data.Substring("FlixPID:".Length), out int processId))
                    {
                        var flix = Process.GetProcessById(processId);
                        managedProcesses.Add(flix);
                    }
                }
            };
            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                processOutput.Add(eventArgs.Data);
                Console.WriteLine(eventArgs.Data);
            };

            // Asynchronously read the standard output of the spawned process. 
            // This raises OutputDataReceived events for each line of output.
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Attach on process exited
            process.Exited += (sender, eventArgs) => { semaphore.Release(); };
            process.EnableRaisingEvents = true;

            // Check if the process already has exited
            if (process.HasExited)
            {
                semaphore.Release();
            }

            bool shouldCancelTask = false;
            var task = Task.Run(async () =>
            {
                while (!shouldCancelTask)
                {
                    var memorySum = managedProcesses.ToList().Select(x => x.WorkingSet64).Sum();
                    scan.Datapoint.Add(new DataPoint {MemoryUsage = process.WorkingSet64 + memorySum, Timestamp = DateTime.Now});
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }

            }, token);

            try
            {
                // Wait for either Console.Cancel or Exit
                await semaphore.WaitAsync(token).ConfigureAwait(false);
            }
            catch(OperationCanceledException ex)
            {
            }

            try
            {
                shouldCancelTask = true;
                await task;
            }
            catch (OperationCanceledException ex)
            {
            }

            // Check if process has exited
            if (!process.HasExited)
            {
                // Kill the process
                process.Kill();

                // Kill all flix processes
                foreach (var managedProcess in managedProcesses)
                {
                    if (!managedProcess.HasExited)
                    {
                        managedProcess.Kill();
                    }
                }

                Console.WriteLine($"[Stopped]: {file}");

                // Set scan entry to starting
                scan.Status = ScanStatus.Unsuccessful;
            }
            else
            {
                // Set scan entry to starting
                scan.Status = ScanStatus.Succeeded;
            }

            scan.Finished = true;
            scan.EndTime = DateTime.Now;

            // Write process output
            var processOutputFile = Path.Combine(outputPath, "process_output.txt");
            // ReSharper disable once MethodSupportsCancellation
            await File.WriteAllLinesAsync(processOutputFile, processOutput.ToList());

            // Update
            dataContext.Update(scan);
            // ReSharper disable once MethodSupportsCancellation
            await dataContext.SaveChangesAsync();
        }

        private static IServiceCollection SetupServices()
        {
            // Registrer services
            var services = new ServiceCollection();
            
            // Add EF Core
            services.AddDbContextPool<DataContext>(options => options.UseSqlite("Data Source=Scan.db"));

            return services;
        }
    }

    internal class ConsoleOptions
    {
        [Value(0, MetaName = "Input path",
            HelpText = "Input path to be processed",
            Required = true)]
        public string InputPath { get; set; }

        [Value(1, MetaName = "Output path",
            HelpText = "Output path to be processed",
            Required = true)]
        public string OutputPath { get; set; }

        [Value(2, MetaName = "SCIL path",
            HelpText = "Path to SCIL",
            Required = true)]
        public string SCILPath { get; set; }

        [Option("timeout", Required = false, HelpText = "Timeout in seconds")]
        public long? Timeout { get; set; }

        [Option("args", Required = false, HelpText = "SCIL arguments")]
        public string Args { get; set; }
    }
}
