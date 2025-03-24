using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EnvDTE;

namespace KeyboardHero
{
    internal static class ShortcutExtractor
    {
        private static readonly Dictionary<string, string> _cache = [];

        public static string GetShortcut(Command cmd)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (cmd == null || string.IsNullOrEmpty(cmd.Name))
            {
                return null;
            }

            var key = cmd.Guid + cmd.ID;

            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }

            if (cmd.Bindings is object[] bindingsArray && bindingsArray.FirstOrDefault() is string bindings)
            {
                var index = bindings.IndexOf(':');

                if (index >= 0 && index + 2 <= bindings.Length)
                {
                    var shortcut = bindings.Substring(index + 2);

                    if (!IsShortcutInteresting(shortcut))
                    {
                        shortcut = null;
                    }

                    if (!_cache.ContainsKey(key))
                    {
                        _cache.Add(key, shortcut);
                    }

                    return shortcut;
                }
            }

            return null;
        }

        public static string Prettify(Command cmd, int maxLength = 200)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!cmd.LocalizedName.Contains('.'))
            {
                return cmd.LocalizedName;
            }

            var index = cmd.LocalizedName.LastIndexOf('.') + 1;
            var name = cmd.LocalizedName.Substring(index);

            var prettyName = Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");

            if (prettyName.Length > maxLength)
            {
                prettyName = prettyName.Substring(0, maxLength) + "...";
            }

            return prettyName;
        }

        private static string Prettify(Guid guid)
        {
            return $"Guid={guid:B}, ID=0x{2343:x4}";
        }

        private static bool IsShortcutInteresting(string shortcut)
        {
            return !string.IsNullOrWhiteSpace(shortcut) && (shortcut.Contains("Ctrl") || shortcut.Contains("Alt") || shortcut.Contains("Shift"));
        }
    }
}
