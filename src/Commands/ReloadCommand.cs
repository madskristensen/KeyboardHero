namespace KeyboardHero
{
    [Command(PackageIds.ReloadWindow)]
    internal sealed class ReloadCommand : BaseCommand<ReloadCommand>
    {
        protected override void Execute(object sender, EventArgs e)
        {
            ReloadRequested?.Invoke(this, EventArgs.Empty);
        }

        public static event EventHandler ReloadRequested;
    }
}
