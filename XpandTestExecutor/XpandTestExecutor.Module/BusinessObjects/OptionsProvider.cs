using System.Collections.Generic;
using System.IO;
using DevExpress.EasyTest.Framework;

namespace XpandTestExecutor.Module.BusinessObjects {
    public class OptionsProvider {
        private static readonly OptionsProvider _optionsProvider=new OptionsProvider();
        Dictionary<string,Options> _options;

        public static OptionsProvider Instance {
            get { return _optionsProvider; }
        }

        public Options this[string fileName] {
            get { return _options[fileName.ToLower()]; }
        }

        public static void Init(string[] easyTestFileNames) {
            Instance._options = new Dictionary<string, Options>();
            foreach (var path in easyTestFileNames) {
                var directoryName = Path.GetDirectoryName(path) + "";
                string fileName = Path.Combine(directoryName, "config.xml");
                var optionsStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var options = Options.LoadOptions(optionsStream, null, null, directoryName);
                optionsStream.Close();
                Instance._options.Add(path.ToLower(), options);
            }
        }
    }
}
