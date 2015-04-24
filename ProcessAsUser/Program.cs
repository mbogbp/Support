using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using CommandLine;
using Xpand.Utils.Helpers;

namespace ProcessAsUser{
    internal class Program{
        [STAThread]
        internal static void Main(string[] args){
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener("processAsUser.log"));
            Trace.Listeners.Add(new ConsoleTraceListener());
            var options = new Options();
            bool arguments = Parser.Default.ParseArguments(args, options);
            Trace.TraceInformation("Arguments parsed="+arguments);
            if (arguments){
                WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
                Debug.Assert(windowsIdentity != null, "windowsIdentity != null");
                var processAsUser = new ProcessAsUser(options);
                if (windowsIdentity.IsSystem){
                    try {
                        var tokenSource = options.Timeout.Execute(() => {
                            Trace.TraceInformation("TIMEOUT");
                            Environment.Exit(0);
                        });
                        Application.Run(new RDClient(processAsUser));
                        tokenSource.Cancel();
                    }
                    catch (ObjectDisposedException){
                    }
                }
                else{
                    throw new NotImplementedException();
                }
            }
            else{
                string message = options.GetUsage();
                Trace.TraceError(message);
                throw new ArgumentException(message);
            }
        }

    }
}