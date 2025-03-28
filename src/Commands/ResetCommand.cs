namespace KeyboardHero
{
    [Command(PackageIds.Reset)]
    internal sealed class ResetCommand : BaseCommand<ResetCommand>
    {
        protected override void Execute(object sender, EventArgs e)
        {
            ResetRequested?.Invoke(this, EventArgs.Empty);
        }

        public static event EventHandler ResetRequested;
    }
}
