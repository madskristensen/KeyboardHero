using System.IO;

namespace KeyboardHero
{
    [Command(PackageIds.OpenCommandList)]
    internal sealed class OpenHistoryFileCommand : BaseCommand<OpenHistoryFileCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var fileName = CommandHistory.GetFileName();

            if (File.Exists(fileName))
            {
                await VS.Documents.OpenAsync(fileName);
            }
            else
            {
                await VS.MessageBox.ShowErrorAsync("Command file not found.");
            }
        }
    }
}
