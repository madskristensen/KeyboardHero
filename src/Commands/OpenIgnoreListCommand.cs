using System.IO;

namespace KeyboardHero
{
    [Command(PackageIds.OpenIgnoreList)]
    internal sealed class OpenIgnoreListCommand : BaseCommand<OpenIgnoreListCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var fileName = IgnoreManager.GetFileName();

            if (File.Exists(fileName))
            {
                await VS.Documents.OpenAsync(fileName);
            }
            else
            {
                await VS.MessageBox.ShowErrorAsync("Ignore list file not found.");
            }
        }
    }
}
