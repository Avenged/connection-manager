using ConnectionManager.Common;
using ConnectionManagerFullTrust.MessageHandlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ConnectionManagerFullTrust
{
    internal class Program
    {
        private static readonly LogWriter logWriter = new LogWriter();
        private static NamedPipeClientStream connection;
        private static List<IMessageHandler> messageHandlers;
        private static ManualResetEvent appServiceExit;

        public static Logger Logger { get; private set; }

        [STAThread]
        private static async Task Main(string[] args)
        {
            Logger = new Logger(logWriter);
            await logWriter.InitializeAsync("debug_fulltrust.log");
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            if (HandleCommandLineArgs())
            {
                // Handles OpenShellCommandInExplorer
                return;
            }

            try
            {
                messageHandlers = new List<IMessageHandler>
                {
                    new FileOperationsHandler()
                };

                // Connect to app service and wait until the connection gets closed
                appServiceExit = new ManualResetEvent(false);
                InitializeAppServiceConnection();

                // Initialize message handlers
                messageHandlers.ForEach(mh => mh.Initialize(connection));

                // Wait until the connection gets closed
                appServiceExit.WaitOne();

                // Wait for ongoing file operations
                messageHandlers.OfType<FileOperationsHandler>().Single().WaitForCompletion();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Logger.UnhandledError(exception, exception.Message);
        }

        private static async void InitializeAppServiceConnection()
        {
            var packageSid = ApplicationData.Current.LocalSettings.Values["PackageSid"];

            connection = new NamedPipeClientStream(".",
                $"Sessions\\{Process.GetCurrentProcess().SessionId}\\AppContainerNamedObjects\\{packageSid}\\ConnectionManagerInteropService_ServerPipe",
                PipeDirection.InOut, PipeOptions.Asynchronous);

            try
            {
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(19));
                await connection.ConnectAsync(cts.Token);
                connection.ReadMode = PipeTransmissionMode.Message;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Could not initialize pipe!");
            }

            if (connection.IsConnected)
            {
                var info = (Buffer: new byte[connection.InBufferSize], Message: new StringBuilder());
                BeginRead(info);
            }
            else
            {
                appServiceExit.Set();
            }
        }

        private static void BeginRead((byte[] Buffer, StringBuilder Message) info)
        {
            var isConnected = connection.IsConnected;
            if (isConnected)
            {
                try
                {
                    connection.BeginRead(info.Buffer, 0, info.Buffer.Length, EndReadCallBack, info);
                }
                catch
                {
                    isConnected = false;
                }
            }
            if (!isConnected)
            {
                appServiceExit.Set();
            }
        }

        private static void EndReadCallBack(IAsyncResult result)
        {
            var readBytes = connection.EndRead(result);
            if (readBytes > 0)
            {
                var info = ((byte[] Buffer, StringBuilder Message))result.AsyncState;

                // Get the read bytes and append them
                info.Message.Append(Encoding.UTF8.GetString(info.Buffer, 0, readBytes));

                if (!connection.IsMessageComplete) // Message is not complete, continue reading
                {
                    BeginRead(info);
                }
                else
                {
                    var message = info.Message.ToString().TrimEnd('\0');

                    Connection_RequestReceived(connection, JsonConvert.DeserializeObject<Dictionary<string, object>>(message));

                    // Begin a new reading operation
                    var nextInfo = (Buffer: new byte[connection.InBufferSize], Message: new StringBuilder());
                    BeginRead(nextInfo);
                }
            }
            else // Disconnected
            {
                appServiceExit.Set();
            }
        }

        private static async void Connection_RequestReceived(PipeStream conn, Dictionary<string, object> message)
        {
            // Get a deferral because we use an awaitable API below to respond to the message
            // and we don't want this call to get cancelled while we are waiting.
            if (message == null)
            {
                return;
            }

            if (message.ContainsKey("Arguments"))
            {
                // This replaces launching the fulltrust process with arguments
                // Instead a single instance of the process is running
                // Requests from UWP app are sent via AppService connection
                var arguments = (string)message["Arguments"];
                //Logger.Info($"Argument: {arguments}");

                await ParseArgumentsAsync(message, arguments);
            }
        }

        private static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool HandleCommandLineArgs()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var arguments = (string)localSettings.Values["Arguments"];
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                localSettings.Values.Remove("Arguments");

                if (arguments == "StartUwp")
                {
                    var folder = localSettings.Values.Get("Folder", "");
                    localSettings.Values.Remove("Folder");

                    using Process process = new Process();
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = "connectionmanager.exe";
                    process.StartInfo.Arguments = folder;
                    process.Start();

                    TerminateProcess((int)localSettings.Values["pid"]);
                    return true;
                }
                else if (arguments == "TerminateUwp")
                {
                    TerminateProcess((int)localSettings.Values["pid"]);
                    return true;
                }
                else if (arguments == "ShellCommand")
                {
                    TerminateProcess((int)localSettings.Values["pid"]);

                    using Process process = new Process();
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = "explorer.exe";
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.Arguments = (string)localSettings.Values["ShellCommand"];
                    process.Start();

                    return true;
                }
            }
            return false;
        }

        private static async Task ParseArgumentsAsync(Dictionary<string, object> message, string arguments)
        {
            switch (arguments)
            {
                case "Terminate":
                    // Exit fulltrust process (UWP is closed or suspended)
                    appServiceExit.Set();
                    break;

                case "Elevate":
                    // Relaunch fulltrust process as admin
                    if (!IsAdministrator())
                    {
                        try
                        {
                            using (Process elevatedProcess = new Process())
                            {
                                elevatedProcess.StartInfo.Verb = "runas";
                                elevatedProcess.StartInfo.UseShellExecute = true;
                                elevatedProcess.StartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                                elevatedProcess.StartInfo.Arguments = "elevate";
                                elevatedProcess.Start();
                            }
                            await Win32API.SendMessageAsync(connection, new ValueSet() { { "Success", 0 } }, message.Get("RequestID", (string)null));
                            appServiceExit.Set();
                        }
                        catch (Win32Exception)
                        {
                            // If user cancels UAC
                            await Win32API.SendMessageAsync(connection, new ValueSet() { { "Success", 1 } }, message.Get("RequestID", (string)null));
                        }
                    }
                    else
                    {
                        await Win32API.SendMessageAsync(connection, new ValueSet() { { "Success", -1 } }, message.Get("RequestID", (string)null));
                    }
                    break;

                default:
                    foreach (var mh in messageHandlers)
                    {
                        await mh.ParseArgumentsAsync(connection, message, arguments);
                    }
                    break;
            }
        }

        private static void TerminateProcess(int processId)
        {
            // Kill the process. This is a BRUTAL WAY to kill a process.
#if DEBUG
            // In debug mode this kills this process too??
#else
            Process.GetProcessById(processId).Kill();
#endif
        }
    }
}
