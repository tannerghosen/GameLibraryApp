using System.Text.Json;
using System.Net.Http;

namespace GamesLibraryApp
{
    public static class Settings
    {
        private static string SettingsFile = Path.Combine(FileSystem.AppDataDirectory, "settings.json");
        // Steam
        private static string SteamAPIKey = "";
        private static long SteamID = 0;

        public static void Init()
        {
            if (!File.Exists(SettingsFile))
            {
                SaveSettings();
            }
            else
            {
                string json = File.ReadAllText(SettingsFile); // read the file as a string
                JsonDocument settings = JsonDocument.Parse(json); // parse it as a json string

                SteamAPIKey = settings.RootElement.GetProperty("SteamAPIKey").GetString();
                SteamID = settings.RootElement.GetProperty("SteamID").GetInt64();
                //GetOwnedGamesURL = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={APIKey}&steamid={SteamID}&format=json&include_appinfo=true&include_played_free_games=true";

                settings.Dispose(); // end the Parse
            }
        }

        public static void UpdateSettings(string setting, string value)
        {
            switch (setting)
            {
                case "sapikey":
                    SteamAPIKey = value;
                    break;
                case "steamid":
                    SteamID = long.Parse(value);
                    break;
                default:
                    break;
            }
            SaveSettings();
        }

        public static void SaveSettings()
        {
            // We write into our settings.json file a JSON object
            // This contains our settings.
            string sapikey = JsonSerializer.Serialize(SteamAPIKey);
            long steamid = SteamID;
            using (StreamWriter writer = new StreamWriter(SettingsFile))
            {
                writer.WriteLine("{");
                writer.WriteLine("\"SteamAPIKey\": " + sapikey + ",");
                writer.WriteLine("\"SteamID\": " + steamid);
                writer.WriteLine("}");
                writer.Close();
            }
        }

        public static string[] GetSettings()
        {
            return new string[] {SteamAPIKey, SteamID.ToString() };
        }
    }
}