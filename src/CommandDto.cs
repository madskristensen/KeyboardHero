namespace KeyboardHero
{
    public class CommandDto()
    {
        public string Name { get; init; }
        public string Guid { get; init; }
        public int Id { get; init; }
        public int Count { get; set; } = 1;

        public string GetPrettyName(int maxLenght = 130)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var name = Name?.Replace("Reattachto", "Re-attach to") ?? "";
            return ShortcutExtractor.Prettify(name, maxLenght);
        }
    }
}
