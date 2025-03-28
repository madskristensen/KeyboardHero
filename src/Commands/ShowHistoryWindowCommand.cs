namespace KeyboardHero
{
    [Command(PackageIds.ShowHistoryWindow)]
    internal sealed class ShowHistoryWindowCommand : BaseCommand<ShowHistoryWindowCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ShortcutHistoryWindow.ShowAsync();
        }
    }
}
