using System.Collections.Generic;
using System.IO;

namespace KeyboardHero
{
    internal class IgnoreManager
    {
        private static readonly List<string> _defaultIgnoreList =
        [
            "Edit.GoToFindCombo",
            "Debug.LocationToolbar.ProcessCombo",
            "Debug.LocationToolbar.ThreadCombo",
            "Debug.LocationToolbar.StackFrameCombo",
            "Build.SolutionPlatforms",
            "Build.SolutionConfigurations"
        ];

        public static List<string> Cache;


        public static async Task InitializeAsync()
        {
            if (Cache != null)
                return;

            var fileName = GetFileName();

            if (File.Exists(fileName))
            {
                Cache = [];

                using (var reader = new StreamReader(fileName))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            Cache.Add(line);
                        }
                    }
                }
            }
            else
            {
                Cache = _defaultIgnoreList;
                Save();
            }
        }

        public static async Task ReloadAsync()
        {
            Cache = null;
            await InitializeAsync();
        }

        public static void AddCommandToIgnoreList(string commandName)
        {
            if (!string.IsNullOrEmpty(commandName) && !Cache.Contains(commandName))
            {
                Cache.Add(commandName);
                Save();
            }
        }

        public static void Save()
        {
            try
            {
                Cache ??= _defaultIgnoreList;
                var fileName = GetFileName();
                File.WriteAllLines(fileName, Cache);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        public static string GetFileName()
        {
            var temp = Path.GetTempPath();
            return Path.Combine(temp, "vs-command-history-ignore.txt");
        }
    }
}
