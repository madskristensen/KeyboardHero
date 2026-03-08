using System.Linq;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Threading;

namespace KeyboardHero
{
    internal class CommandIntercepter(JoinableTaskFactory _jtf) : IDisposable
    {
        private DTE2 _dte;
        private CommandEvents _events;
        private bool _showShortcut;
        private CommandHistory _history;
        private readonly Key[] _keys = Enum.GetValues(typeof(Key)).Cast<Key>().Where(key => key != Key.None).ToArray();
        private static bool _isEnabled;
        private bool _disposed;

        public async Task InitializeAsync()
        {
            await IgnoreManager.InitializeAsync();

            await _jtf.SwitchToMainThreadAsync();

            _history = new CommandHistory();

            _dte = await VS.GetRequiredServiceAsync<DTE, DTE2>();
            _events = _dte.Events.CommandEvents;

            General options = await General.GetLiveInstanceAsync();
            _isEnabled = options.Enabled;

            _events.BeforeExecute += OnBeforeExecute;
            _events.AfterExecute += OnAfterExecute;
            General.Saved += OnOptionsSaved;
        }

        private void OnOptionsSaved(General options)
        {
            _isEnabled = options.Enabled;
        }

        private void OnBeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            if (_disposed) return;
            _showShortcut = _isEnabled && !_keys.Any(Keyboard.IsKeyDown);
        }

        private void OnAfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            if (_disposed) return;

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
            return IgnoreManager.Contains(name);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_events != null)
                {
                    _events.BeforeExecute -= OnBeforeExecute;
                    _events.AfterExecute -= OnAfterExecute;
                }
                
                General.Saved -= OnOptionsSaved;
                _history?.Dispose();
                _disposed = true;
            }
        }
    }
}