using System.Linq;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Threading;

namespace KeyboardHero
{
    internal class CommandIntercepter(JoinableTaskFactory _jtf)
    {
        private DTE2 _dte;
        private CommandEvents _events;
        private bool _showShortcut;
        private CommandHistory _history;
        private readonly Key[] _keys = [Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift];

        public async Task InitializeAsync()
        {
            await IgnoreManager.InitializeAsync();

            await _jtf.SwitchToMainThreadAsync();

            _history = new CommandHistory();

            _dte = await VS.GetRequiredServiceAsync<DTE, DTE2>();
            _events = _dte.Events.CommandEvents;

            _events.BeforeExecute += OnBeforeExecute;
            _events.AfterExecute += OnAfterExecute;
        }

        private void OnBeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            _showShortcut = !_keys.Any(Keyboard.IsKeyDown);
        }

        private void OnAfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!_showShortcut || CustomIn is not null || CustomOut is not null)
            {
                return;
            }

            Command cmd;
            try
            {
                cmd = _dte.Commands.Item(Guid, ID);
            }
            catch (ArgumentException)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(cmd?.Name) || ShouldCommandBeIgnored(cmd.LocalizedName ?? cmd.Name))
            {
                return;
            }

            var shortcut = ShortcutExtractor.GetShortcut(cmd);

            if (string.IsNullOrWhiteSpace(shortcut))
            {
                return;
            }

            _history.AddCommandAsync(cmd).FireAndForget();
            VS.StatusBar.ShowMessageAsync($"Use {shortcut} to invoke '{ShortcutExtractor.Prettify(cmd.LocalizedName)}'").FireAndForget();
        }

        private static bool ShouldCommandBeIgnored(string name)
        {
            return IgnoreManager.Cache?.Contains(name, StringComparer.OrdinalIgnoreCase) == true;
        }
    }
}