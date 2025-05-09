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

        public static string Prettify(string rawName, int maxLength = 200)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!rawName.Contains('.'))
            {
                return rawName;
            }

            ReadOnlySpan<char> localizedNameSpan = rawName.AsSpan();
            var index = localizedNameSpan.LastIndexOf('.') + 1;
            ReadOnlySpan<char> nameSpan = localizedNameSpan.Slice(index);

            var prettyName = Regex.Replace(nameSpan.ToString(), "([a-z])([A-Z])", "$1 $2");

            if (prettyName.Length > maxLength)
            {
                prettyName = prettyName.Substring(0, maxLength) + "...";
            }

            return prettyName;
        }

        private static bool IsShortcutInteresting(string shortcut)
        {
            return !string.IsNullOrWhiteSpace(shortcut) && (shortcut.Contains("Ctrl") || shortcut.Contains("Alt") || shortcut.Contains("Shift"));
        }
    }
}
