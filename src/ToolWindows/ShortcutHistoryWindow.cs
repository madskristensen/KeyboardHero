using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Imaging;

namespace KeyboardHero
{
    public class ShortcutHistoryWindow : BaseToolWindow<ShortcutHistoryWindow>
    {
        public override string GetTitle(int toolWindowId) => Vsix.Name;

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            await CommandHistory.InitializeAsync();

            DTE2 dte = await VS.GetRequiredServiceAsync<DTE, DTE2>();

            return new ShortcutHistoryWindowControl(dte);
        }

        [Guid("4eb47c43-5502-4ccd-9c9a-84d52d144022")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.FSWCF;
            }
        }
    }
}
