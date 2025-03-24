global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace KeyboardHero
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.KeyboardHeroString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(ShortcutHistoryWindow.Pane), Style = VsDockStyle.Tabbed, Window = ToolWindowGuids.SolutionExplorer)]
    //[ProvideToolWindowVisibility(typeof(ShortcutHistoryWindow.Pane), VSConstants.UICONTEXT.NoSolution_string)]
    public sealed class KeyboardHeroPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();

            CommandIntercepter commandIntercepter = new(JoinableTaskFactory);
            await commandIntercepter.InitializeAsync();
        }
    }
}