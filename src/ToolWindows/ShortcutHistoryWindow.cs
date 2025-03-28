using System.ComponentModel.Design;
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
        private static RatingPrompt _ratingPrompt;

        public override string GetTitle(int toolWindowId) => Vsix.Name;

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            await CommandHistory.InitializeAsync();

            DTE2 dte = await VS.GetRequiredServiceAsync<DTE, DTE2>();

            General options = await General.GetLiveInstanceAsync();
            _ratingPrompt ??= new("MadsKristensen.KeyboardHero", Vsix.Name, options, 2);
            _ratingPrompt.RegisterSuccessfulUsage();

            return new ShortcutHistoryWindowControl(dte);
        }

        [Guid("4eb47c43-5502-4ccd-9c9a-84d52d144022")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.FSWCF;
                ToolBar = new CommandID(PackageGuids.KeyboardHero, PackageIds.Toolbar);
            }
        }
    }
}
