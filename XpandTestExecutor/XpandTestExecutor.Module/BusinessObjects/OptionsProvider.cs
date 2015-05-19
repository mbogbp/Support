using System;
using System.Collections.Generic;
using System.IO;
using DevExpress.EasyTest.Framework;

namespace XpandTestExecutor.Module.BusinessObjects {
    public class OptionsProvider {
        private static readonly OptionsProvider _optionsProvider=new OptionsProvider();
        Dictionary<Guid,Options> _options;

        public static OptionsProvider Instance {
            get { return _optionsProvider; }
        }

        public Options this[Guid guid] {
            get { return _options[guid]; }
        }

        public static void Init(EasyTest[] easyTests) {
            Instance._options = new Dictionary<Guid, Options>();
            foreach (var easyTest in easyTests) {
                var directoryName = Path.GetDirectoryName(easyTest.FileName) + "";
                string fileName = Path.Combine(directoryName, "config.xml");
                var optionsStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var options = Options.LoadOptions(optionsStream, null, null, directoryName);
                optionsStream.Close();
                Instance._options.Add(easyTest.Oid, options);
            }
        }
    }
}
