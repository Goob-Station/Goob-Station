using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Content.Pirate.Server.Pacification.Managers
{
    public sealed class PacifyManager
    {
        private static readonly Dictionary<string, (string UserName, DateTime ExpirationDate)> _pacifiedPlayers = new();
        private static readonly string FilePath = Path.Combine("data", "pacify_list.json");
        private static bool _isDataLoaded = false;

        public PacifyManager()
        {
            if (!_isDataLoaded)
            {
                LoadFromFile();
                _isDataLoaded = true;
            }
        }

        public void AddToPacify(string userId, string userName, int days)
        {
            if (days < 1)
            {
                throw new ArgumentException("Кількість днів повинна бути щонайменше 1.");
            }

            var expirationDate = DateTime.Now.AddDays(days);
            if (!_pacifiedPlayers.ContainsKey(userId))
            {
                _pacifiedPlayers[userId] = (userName, expirationDate);
                SaveToFile();
            }
            else
            {
                _pacifiedPlayers[userId] = (userName, expirationDate);
                SaveToFile();
            }
        }

        public void RemoveFromPacify(string userId)
        {
            if (_pacifiedPlayers.ContainsKey(userId))
            {
                _pacifiedPlayers.Remove(userId);
                SaveToFile();
            }
        }

        public List<(string UserId, string UserName, DateTime ExpirationDate)> GetPacifyList()
        {
            var expiredEntries = _pacifiedPlayers.Where(p => p.Value.ExpirationDate <= DateTime.Now).ToList();

            foreach (var entry in expiredEntries)
            {
                _pacifiedPlayers.Remove(entry.Key);
            }

            if (expiredEntries.Count > 0)
            {
                SaveToFile();
            }

            return _pacifiedPlayers
                .Where(p => p.Value.ExpirationDate > DateTime.Now)
                .Select(p => (p.Key, p.Value.UserName, p.Value.ExpirationDate))
                .ToList();
        }

        private static void SaveToFile()
        {
            try
            {
                if (!Directory.Exists("data"))
                {
                    Directory.CreateDirectory("data");
                }

                var serializableData = _pacifiedPlayers.ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        UserName = entry.Value.UserName,
                        ExpirationDate = entry.Value.ExpirationDate
                    }
                );

                var json = JsonSerializer.Serialize(serializableData);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save pacify list: {ex.Message}");
            }
        }

        private static void LoadFromFile()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);

                    var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                    if (data != null)
                    {
                        bool dataModified = false;

                        foreach (var entry in data)
                        {
                            if (entry.Value.TryGetValue("ExpirationDate", out var expirationDateObj) &&
                                expirationDateObj != null &&
                                entry.Value.TryGetValue("UserName", out var userNameObj) &&
                                userNameObj != null)
                            {
                                var expirationDate = DateTime.Parse(expirationDateObj.ToString()!);
                                var userName = userNameObj.ToString()!;

                                if (expirationDate > DateTime.Now)
                                {
                                    _pacifiedPlayers[entry.Key] = (userName, expirationDate);
                                }
                                else
                                {
                                    dataModified = true;
                                }
                            }
                        }

                        if (dataModified)
                        {
                            SaveToFile();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load pacify list: {ex.Message}");
            }
        }

        public bool IsPacified(string playerName)
        {
            GetPacifyList();
            return _pacifiedPlayers.Any(entry =>
                string.Equals(entry.Value.UserName, playerName, StringComparison.OrdinalIgnoreCase) &&
                entry.Value.ExpirationDate > DateTime.Now);
        }
    }
}
