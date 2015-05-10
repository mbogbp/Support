using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DevExpress.EasyTest.Framework;
using Xpand.Utils.Helpers;
using XpandTestExecutor.Module.BusinessObjects;

namespace XpandTestExecutor.Module {
    public class TestEnviroment {
        public static void KillProcessAsUser() {
            var processes = Process.GetProcesses().Where(process => process.ProcessName.Contains("ProcessAsUser")).ToArray();
            foreach (var process in processes) {
                process.Kill();
            }
        }

        public static void KillWebDev(string name) {
            EnviromentEx.KillProccesses(name, i => Process.GetProcessById(i).ProcessName.StartsWith("WebDev.WebServer40"));
        }

        public static void Cleanup(EasyTest[] easyTests) {
            var processes = Process.GetProcesses().Where(process => process.ProcessName.StartsWith("WebDev.WebServer40")).ToArray();
            foreach (var source in processes) {
                source.Kill();
            }

            foreach (var easyTest in easyTests.GroupBy(test => Path.GetDirectoryName(test.FileName))) {
                var path = Path.Combine(easyTest.Key, "config.xml");
                try {
                    using (var optionsStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        var options = Options.LoadOptions(optionsStream, null, null, easyTest.Key);
                        foreach (var alias in options.Aliases.Cast<TestAlias>().Where(@alias => alias.ContainsAppPath())) {
                            var appPath = alias.UpdateAppPath(null,true);
                            var usersDir = Path.Combine(appPath, TestRunner.EasyTestUsersDir);
                            if (Directory.Exists(usersDir))
                                Directory.Delete(usersDir, true);
                        }
                    }
                }
                catch (Exception e) {
                    throw new Exception(easyTest.Key, e);
                }
            }

        }

        public static void Setup(EasyTestExecutionInfo info,bool unlink) {
            string configPath = Path.GetDirectoryName(info.EasyTest.FileName) + "";
            string fileName = Path.Combine(configPath, "config.xml");
            TestUpdater.UpdateTestConfig(info, fileName,unlink);
            AppConfigUpdater.Update(fileName, configPath, info,unlink);
            TestUpdater.UpdateTestFile(info,unlink);
        }
    }
}