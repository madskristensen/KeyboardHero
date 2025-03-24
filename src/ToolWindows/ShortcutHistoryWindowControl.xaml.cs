using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EnvDTE80;

namespace KeyboardHero
{
    public partial class ShortcutHistoryWindowControl : UserControl
    {
        private readonly DTE2 _dte;

        public ShortcutHistoryWindowControl(DTE2 dte)
        {
            _dte = dte;

            InitializeComponent();
            LoadList();

            CommandHistory.Saved += OnSaved;
            IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!VsShellUtilities.ShellIsShuttingDown)
            {
                OnSaved(this, EventArgs.Empty);
            }
        }

        private void OnSaved(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.StartOnIdle(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                LoadList();
            }, VsTaskRunContext.UIThreadIdlePriority).FireAndForget();
        }

        private void LoadList()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            pnlList.Children.Clear();

            IOrderedEnumerable<CommandDto> commands = CommandHistory.Commands.Where(c => c.Count > 1).OrderByDescending(c => c.Count);

            if (!commands.Any())
            {
                pnlList.HorizontalAlignment = HorizontalAlignment.Center;
                pnlList.Children.Add(new Label { Content = "No commands recorded yet..." });

                return;
            }

            pnlList.HorizontalAlignment = HorizontalAlignment.Left;

            foreach (CommandDto command in commands)
            {
                EnvDTE.Command cmd = _dte.Commands.Item(command.Guid, command.Id);

                var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    ToolTip = new ToolTip() { Content = cmd.LocalizedName }
                };

                var count = new Label
                {
                    Content = $"({command.Count} times)",
                    Opacity = .7,
                };

                var label = new Label
                {
                    Content = $"for {ShortcutExtractor.Prettify(cmd, 130)}",
                    Margin = new Thickness(0),
                    Padding = new Thickness(0, 5, 0, 5)
                };

                var shortcut = new Label
                {
                    Content = $"{ShortcutExtractor.GetShortcut(cmd)}",
                    FontWeight = FontWeights.Bold,
                };

                panel.Children.Add(shortcut);
                panel.Children.Add(label);
                panel.Children.Add(count);

                pnlList.Children.Add(panel);
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            pnlList.Children.Clear();

            ThreadHelper.JoinableTaskFactory.StartOnIdle(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await CommandHistory.InitializeAsync();

            }, VsTaskRunContext.UIThreadIdlePriority).FireAndForget();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (VS.MessageBox.ShowConfirm("Do you want to delete all commands from this list?"))
            {
                CommandHistory.Reset();
            }
        }
    }
}
