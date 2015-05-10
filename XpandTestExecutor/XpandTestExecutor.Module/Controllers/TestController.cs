using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using Xpand.Persistent.Base.General;
using XpandTestExecutor.Module.BusinessObjects;

namespace XpandTestExecutor.Module.Controllers {
    public class TestControllerHelper:WindowController {
        internal const string Selected = "Selected";
        internal const string FromFile = "FromFile";
        internal const string System = "System";
        private const string Current = "Current";
        private readonly SingleChoiceAction _userModeAction;
        private readonly SingleChoiceAction _executionModeAction;
        public TestControllerHelper() {
            TargetWindowType=WindowType.Main;
            var windowsIdentity = WindowsIdentity.GetCurrent();
            Debug.Assert(windowsIdentity != null, "windowsIdentity != null");

            _userModeAction = new SingleChoiceAction(this, "UserMode", PredefinedCategory.Tools) { Caption = "Identity" };
            _userModeAction.Items.Add(new ChoiceActionItem(System, System));
            _userModeAction.Items.Add(new ChoiceActionItem(Current, Current));
            _userModeAction.Active[""] = windowsIdentity.IsSystem;

            _executionModeAction = new SingleChoiceAction(this, "ExecutionMode", PredefinedCategory.Tools) { Caption = "Execution" };
            _executionModeAction.Items.Add(new ChoiceActionItem(FromFile, FromFile));
            _executionModeAction.Items.Add(new ChoiceActionItem(Selected, Selected));
        }

        public SingleChoiceAction UserModeAction {
            get { return _userModeAction; }
        }

        public SingleChoiceAction ExecutionModeAction {
            get { return _executionModeAction; }
        }
    }
    public class TestController : ObjectViewController<ListView, EasyTest> {
        private const string CancelRun = "Cancel Run";
        private const string Run = "Run";
        private readonly SimpleAction _runTestAction;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly SimpleAction _unlinkTestAction;
        private SingleChoiceAction _executionModeAction;
        private SingleChoiceAction _userModeAction;

        public TestController() {
            _runTestAction = new SimpleAction(this, "RunTest", PredefinedCategory.View) { Caption = Run };
            _runTestAction.Execute += RunTestActionOnExecute;

            _unlinkTestAction = new SimpleAction(this, "UnlinkTest", PredefinedCategory.View) {Caption = "Unlink"};
            _unlinkTestAction.Execute+=UnlinkTestActionOnExecute;   
        }

        protected override void OnActivated() {
            base.OnActivated();
            var testControllerHelper = Application.MainWindow.GetController<TestControllerHelper>();
            _executionModeAction = testControllerHelper.ExecutionModeAction;
            _userModeAction = testControllerHelper.UserModeAction;
        }

        private void UnlinkTestActionOnExecute(object sender, SimpleActionExecuteEventArgs e) {
            var easyTests = e.SelectedObjects.Cast<EasyTest>().ToArray();
            if (ReferenceEquals(_executionModeAction.SelectedItem.Data, TestControllerHelper.FromFile)) {
                var fileNames = File.ReadAllLines("easytests.txt").Where(s => !string.IsNullOrEmpty(s)).ToArray();
                easyTests = EasyTest.GetTests(ObjectSpace, fileNames);
            }
            foreach (var info in easyTests.SelectMany(test => test.GetCurrentSequenceInfos())) {
                info.WindowsUser = WindowsUser.CreateUsers((UnitOfWork)ObjectSpace.Session(), false).First();
                TestEnviroment.Setup(info,true);
            }
            ObjectSpace.RollbackSilent();

        }

        private void RunTestActionOnExecute(object sender, SimpleActionExecuteEventArgs e) {
            var isSystemMode = IsSystemMode();
            if (_runTestAction.Caption==CancelRun){
                if (_cancellationTokenSource != null) _cancellationTokenSource.Cancel();
                _runTestAction.Caption = Run;
            }
            else if (ReferenceEquals(_executionModeAction.SelectedItem.Data, TestControllerHelper.Selected)) {
                _runTestAction.Caption = CancelRun;
                if (!isSystemMode) {
                    _unlinkTestAction.DoExecute();
                }
                _cancellationTokenSource = TestRunner.Execute(e.SelectedObjects.Cast<EasyTest>().ToArray(), isSystemMode,
                    task => _runTestAction.Caption = Run);}
            else {
                var fileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "EasyTests.txt");
                TestRunner.Execute(fileName, isSystemMode);
            }
        }

        private bool IsSystemMode() {
            return _executionModeAction.Active && ReferenceEquals(_userModeAction.SelectedItem.Data, TestControllerHelper.System);
        }
    }
}