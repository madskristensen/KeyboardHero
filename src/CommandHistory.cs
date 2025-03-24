using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Newtonsoft.Json;

namespace KeyboardHero
{
    public class CommandDto()
    {
        public string Name { get; init; }
        public string Guid { get; init; }
        public int Id { get; init; }
        public int Count { get; set; } = 1;
    }

    internal class CommandHistory()
    {
        private Timer _timer;
        public static List<CommandDto> Commands { get; private set; }

        public static async Task InitializeAsync()
        {
            Commands = await LoadAsync();
            Saved?.Invoke(null, EventArgs.Empty);
        }

        public static void Reset()
        {
            var filePath = GetFileName();

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            Commands?.Clear();
            Saved?.Invoke(null, EventArgs.Empty);
        }

        public async Task AddCommandAsync(Command cmd)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Commands ??= await LoadAsync();

            CommandDto existing = Commands.FirstOrDefault(c =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return c.Guid == cmd.Guid && c.Id == cmd.ID;
            });

            if (existing != null)
            {
                existing.Count++;
            }
            else
            {
                Commands.Add(new CommandDto() { Name = cmd.LocalizedName, Guid = cmd.Guid, Id = cmd.ID });
            }

            _timer ??= new Timer(Save);
            _timer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        private void Save(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            SaveAsync().FireAndForget();
        }

        private async Task SaveAsync()
        {
            var filePath = GetFileName();
            using (var writer = new StreamWriter(filePath))
            {
                IOrderedEnumerable<CommandDto> commands = Commands.OrderByDescending(c => c.Count);
                var json = JsonConvert.SerializeObject(commands, Formatting.Indented);
                await writer.WriteAsync(json);
            }

            Saved?.Invoke(this, EventArgs.Empty);
        }

        private static async Task<List<CommandDto>> LoadAsync()
        {
            var filePath = GetFileName();

            if (!File.Exists(filePath))
            {
                return [];
            }

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    var json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<List<CommandDto>>(json) ?? [];
                }
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
                return [];
            }
        }

        private static string GetFileName()
        {
            var temp = Path.GetTempPath();
            return Path.Combine(temp, "vs-command-history.json");
        }

        public static event EventHandler Saved;
    }
}
