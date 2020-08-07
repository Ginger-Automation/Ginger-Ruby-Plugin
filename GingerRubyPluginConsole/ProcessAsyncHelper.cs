using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace GingerRubyPluginConsole
{
    /// <summary>
    /// Process helper with asynchronous interface
    /// - Based on https://gist.github.com/georg-jung/3a8703946075d56423e418ea76212745
    /// - And on https://stackoverflow.com/questions/470256/process-waitforexit-asynchronously
    /// </summary>
    public static class ProcessHelper
    {
        public static void StartProcess()
        {
            string foobat =
    @"START ping -t localhost
START ping -t google.com
ECHO Batch file is done!
EXIT /B 123
";

            File.WriteAllText("foo.bat", foobat);

            Process p = new Process
            {
                StartInfo =
                new ProcessStartInfo("foo.bat")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            p.Start();

            var _ = ConsumeReader(p.StandardOutput);
            _ = ConsumeReader(p.StandardError);

            Console.WriteLine("Calling WaitForExit()...");
            p.WaitForExit();
            Console.WriteLine("Process has exited. Exit code: {0}", p.ExitCode);
            Console.WriteLine("WaitForExit returned.");
        }

        async static Task ConsumeReader(TextReader reader)
        {
            string text;

            while ((text = await reader.ReadLineAsync()) != null)
            {
                Console.WriteLine(text);
            }
        }
        public static async Task<Result> RunAsync(ProcessStartInfo startInfo, int? timeoutMs = null)
        {
            Result result = new Result();

            using (var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true })
            {
                // List of tasks to wait for a whole process exit
                List<Task> processTasks = new List<Task>();

                // === EXITED Event handling ===
                var processExitEvent = new TaskCompletionSource<object>();
                process.Exited += (sender, args) =>
                {
                    processExitEvent.TrySetResult(true);
                };
                processTasks.Add(processExitEvent.Task);

                // === STDOUT handling ===
                var stdOutBuilder = new StringBuilder();
                if (process.StartInfo.RedirectStandardOutput)
                {
                    var stdOutCloseEvent = new TaskCompletionSource<bool>();

                    process.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data == null)
                        {
                            stdOutCloseEvent.TrySetResult(true);
                        }
                        else
                        {
                            stdOutBuilder.Append(e.Data);
                        }
                    };

                    processTasks.Add(stdOutCloseEvent.Task);
                }
                else
                {
                    // STDOUT is not redirected, so we won't look for it
                }

                // === STDERR handling ===
                var stdErrBuilder = new StringBuilder();
                if (process.StartInfo.RedirectStandardError)
                {
                    var stdErrCloseEvent = new TaskCompletionSource<bool>();

                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data == null)
                        {
                            stdErrCloseEvent.TrySetResult(true);
                        }
                        else
                        {
                            stdErrBuilder.Append(e.Data);
                        }
                    };

                    processTasks.Add(stdErrCloseEvent.Task);
                }
                else
                {
                    // STDERR is not redirected, so we won't look for it
                }

                // === START OF PROCESS ===
                if (!process.Start())
                {
                    result.ExitCode = process.ExitCode;
                    return result;
                }

                // Reads the output stream first as needed and then waits because deadlocks are possible
                if (process.StartInfo.RedirectStandardOutput)
                {
                    process.BeginOutputReadLine();
                }
                else
                {
                    // No STDOUT
                }

                if (process.StartInfo.RedirectStandardError)
                {
                    process.BeginErrorReadLine();
                }
                else
                {
                    // No STDERR
                }

                // === ASYNC WAIT OF PROCESS ===

                // Process completion = exit AND stdout (if defined) AND stderr (if defined)
                Task processCompletionTask = Task.WhenAll(processTasks);

                // Task to wait for exit OR timeout (if defined)
                Task<Task> awaitingTask = timeoutMs.HasValue
                    ? Task.WhenAny(Task.Delay(timeoutMs.Value), processCompletionTask)
                    : Task.WhenAny(processCompletionTask);

                // Let's now wait for something to end...
                if ((await awaitingTask.ConfigureAwait(false)) == processCompletionTask)
                {
                    // -> Process exited cleanly
                    result.ExitCode = process.ExitCode;
                }
                else
                {
                    // -> Timeout, let's kill the process
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                // Read stdout/stderr
                result.StdOut = stdOutBuilder.ToString();
                result.StdErr = stdErrBuilder.ToString();
            }

            return result;
        }

        public static Result RunProcess(ProcessStartInfo processStartInfo, int timeout)
        {
            Result result = new Result();

            using (Process process = new Process() { StartInfo = processStartInfo, EnableRaisingEvents = true })
            {
                
                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        // Process completed. Check process.ExitCode here.
                        result.ExitCode = process.ExitCode;
                    }
                    else
                    {
                        // Timed out.
                    }
                }
                result.StdOut = output.ToString();
                result.StdErr = error.ToString();
            }
            return result;
        }

        /// <summary>
        /// Run process result
        /// </summary>
        public class Result
        {
            /// <summary>
            /// Exit code
            /// <para>If NULL, process exited due to timeout</para>
            /// </summary>
            public int? ExitCode { get; set; } = null;

            /// <summary>
            /// Standard error stream
            /// </summary>
            public string StdErr { get; set; } = "";

            /// <summary>
            /// Standard output stream
            /// </summary>
            public string StdOut { get; set; } = "";
        }
    }

    
}
