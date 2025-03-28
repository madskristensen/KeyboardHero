
namespace KeyboardHero
{
    [Command(PackageIds.Toggle)]
    internal sealed class ToggleCommand : BaseCommand<ToggleCommand>
    {
        protected override void BeforeQueryStatus(EventArgs e)
        {
            Command.Checked = General.Instance.Enabled;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            General options = await General.GetLiveInstanceAsync();
            options.Enabled = !options.Enabled;
            await options.SaveAsync();

            Command.Checked = options.Enabled;

            if (Command.Checked)
            {
                await VS.StatusBar.ShowMessageAsync("Keyboard Hero is now enabled.");
            }
            else
            {
                await VS.StatusBar.ShowMessageAsync("Keyboard Hero is now disabled.");
            }
        }
    }
}
