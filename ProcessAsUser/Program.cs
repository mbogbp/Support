using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using CommandLine;
using Common.Logging;
using Xpand.Utils.Helpers;
using Xpand.Utils.Threading;

namespace ProcessAsUser{
    internal class Program{
        public const Int32 WAIT_TIMEOUT = 258;
        public static readonly ILog Logger = LogManager.GetLogger<Program>();
        [STAThread]
        internal static void Main(string[] args) {
            var options = new Options();
            bool arguments = Parser.Default.ParseArguments(args, options);
            Logger.Info("Arguments parsed=" + arguments);
            if (arguments){
                WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
                Debug.Assert(windowsIdentity != null, "windowsIdentity != null");
                var processAsUser = new ProcessAsUser(options);
                if (windowsIdentity.IsSystem){
                    Logger.Info("IsSystem");
                    KillRunningProcesses();
                    try{
                        var tokenSource = ExitOnTimeout(options);
                        using (var rdClient = new RDClient(processAsUser)){
                            rdClient.Closing += (sender, eventArgs) => {
                                Logger.Info("Closing");
                                tokenSource.Cancel();
                            };
                            Application.Run(rdClient);
                        }
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
                Logger.Error(message);
                throw new ArgumentException(message);
            }
        }

        public static CancellationTokenSource ExitOnTimeout(Options options){
            var tokenSource = options.Timeout.Execute(() =>{
                Logger.Info("TIMEOUT");
                Environment.Exit(WAIT_TIMEOUT);
            });
            return tokenSource;
        }

        private static void KillRunningProcesses(){
            var currentProcess = Process.GetCurrentProcess();
            foreach (var process in currentProcess.GetRunningProcess().ToArray()){
                var id = process.Id;
                process.Kill();
                Logger.Info("Process " + id + " killed");
            }
        }
    }
}