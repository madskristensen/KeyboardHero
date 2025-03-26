using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EnvDTE80;
using Microsoft.VisualStudio.Imaging;

namespace KeyboardHero
{
    public partial class ShortcutHistoryWindowControl : UserControl
    {
        private readonly DTE2 _dte;

        public ShortcutHistoryWindowControl(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _dte = dte;

            InitializeComponent();
            LoadList();
            pnlList.PreviewMouseLeftButtonUp += OnMouseClick;
        }

        private void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is CrispImage ignore)
            {
                IgnoreManager.AddCommandToIgnoreList(ignore.Tag?.ToString());
                if (ignore.Parent is FrameworkElement parent)
                {
                    parent.Visibility = Visibility.Hidden;
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            CommandHistory.Updated += OnUpdate;
            IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!VsShellUtilities.ShellIsShuttingDown && IsVisible)
            {
                OnUpdate(this, EventArgs.Empty);
            }
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.StartOnIdle(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (IsVisible)
                {
                    LoadList();
                }

            }, VsTaskRunContext.UIThreadIdlePriority).FireAndForget();
        }

        private void LoadList()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            pnlList.Children.Clear();

            IOrderedEnumerable<CommandDto> commands = CommandHistory.Commands
                .Where(c => !IgnoreManager.Cache.Contains(c.Name))
                .OrderByDescending(c => c.Count);

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
                };

                var count = new Label
                {
                    Content = $"({command.Count} times)",
                    Opacity = .7,
                };

                var label = new Label
                {
                    Content = $"for {ShortcutExtractor.Prettify(command.Name, 130)}",
                    Margin = new Thickness(0),
                    Padding = new Thickness(0, 5, 0, 5),
                    ToolTip = new ToolTip { Content = command.Name }
                };

                var shortcut = new Label
                {
                    Content = $"{ShortcutExtractor.GetShortcut(cmd)}",
                    FontWeight = FontWeights.Bold,
                };

                var ignore = new CrispImage
                {
                    Moniker = KnownMonikers.Cancel,
                    Width = 12,
                    Height = 12,
                    Opacity = .7,
                    Cursor = Cursors.Hand,
                    ToolTip = new ToolTip { Content = "Ignore this command and remove from the list" },
                    Tag = cmd.LocalizedName ?? cmd.Name,
                };

                if (shortcut.Content is string text && !string.IsNullOrEmpty(text))
                {
                    panel.Children.Add(shortcut);
                    panel.Children.Add(label);
                    panel.Children.Add(count);
                    panel.Children.Add(ignore);

                    pnlList.Children.Add(panel);
                }
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            pnlList.Children.Clear();

            ThreadHelper.JoinableTaskFactory.StartOnIdle(async () =>
            {
                await IgnoreManager.ReloadAsync();
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var fileName = IgnoreManager.GetFileName();

            if (!File.Exists(fileName))
            {
                IgnoreManager.Save();
            }

            VS.Documents.OpenAsync(fileName).FireAndForget();
        }
    }
}
