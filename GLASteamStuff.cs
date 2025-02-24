using System.Text.Json;

namespace GamesLibraryApp
{
    public class SteamGame : Game // SteamGame inherits from Game, used to differentiate between Steam and non-Steam games
    {
    }

    public class SteamStats : Stats // SteamStats inherits from Stats, see above why we seperate them
    {

    }
    public static class GLASteamStuff
    {
        private static string APIKey = ""; // API Key
        private static long SteamID = 0; // User's Steam ID
        private static string GetOwnedGamesURL = ""; // URL to get the user's owned games

        public static void Update()
        {
            string[] settings = Settings.GetSettings();
            APIKey = settings[0];
            SteamID = long.Parse(settings[1]);
            GetOwnedGamesURL = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={APIKey}&steamid={SteamID}&format=json&include_appinfo=true&include_played_free_games=true";
        }
        public static async Task<bool> IsSteamAPIUp()
        {
            Update();
            var response = await GLAHttpClient.IsUrlUp("https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid=440");
            return response;
        }

        // called at the start of a command to ensure we can actually do stuff with it
        public static async Task<bool> IsEverythingOK()
        {
            if (await IsSteamAPIUp() == true)
            {
                if ((APIKey == "" || SteamID == 0))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        // This gets the Steam Library
        public static async Task<(List<SteamGame> Games, SteamStats?)> Games()
        {
            if (await IsEverythingOK() == true)
            {
                string rawdata = await GLAHttpClient.GetData(GetOwnedGamesURL);
                if (rawdata != null)
                {
                    JsonDocument data = JsonDocument.Parse(rawdata);
                    JsonElement games = data.RootElement.GetProperty("response").GetProperty("games");
                    SteamStats Stats = new SteamStats();
                    List<SteamGame> Games = new List<SteamGame>();
                    // for each element in games
                    foreach (JsonElement game in games.EnumerateArray())
                    {
                        int appid;
                        SteamGame steamgame = new SteamGame();
                        steamgame.Name = game.GetProperty("name").GetString(); // game -> name
                        steamgame.Playtime = Math.Round(game.GetProperty("playtime_forever").GetDouble() / 60, 2);
                        Stats.TotalPlaytime += steamgame.Playtime;
                        appid = game.GetProperty("appid").GetInt32(); // game -> appid
                        steamgame.Id = appid;
                        // structure is media.steampowered.com/steamcommunity/public/images/apps/game's id/img's name.jpg
                        // while img_icon_url might give the impression it's the whole url to the img, it's merely the hashed name of the image
                        // we don't have to do anything with this hash, just put it at the end of the link and put a .jpg after it
                        steamgame.Icon = $"https://media.steampowered.com/steamcommunity/public/images/apps/{appid}/{game.GetProperty("img_icon_url").GetString()}.jpg";
                        (int earn, int total, double perc, bool isperf) = await Achievements(appid);
                        steamgame.AchievementsEarned = earn;
                        Stats.AchievementsEarnedTotal += earn;
                        steamgame.TotalAchievements = total;
                        Stats.AchievementsTotal += total;
                        steamgame.Percent = perc;
                        steamgame.IsPerfectGame = isperf;
                        Stats.PerfectGames += (steamgame.IsPerfectGame == true ? 1 : 0);
                        Games.Add(steamgame);
                    }
                    Stats.TotalGames = data.RootElement.GetProperty("response").GetProperty("game_count").GetInt32();
                    return (Games, Stats);
                }
                else
                {
                    return (null, null);
                }
            }
            else
            {
                return (null, null);
            }
        }
        
        // This gets the achievements per steam game where applicable.
        public static async Task<(int, int, double, bool)> Achievements(int appid)
        {
            // We try - catch because not every game that shows up actually has achievements or stats or anything of that nature.
            try
            {
                string rawachdata = await GLAHttpClient.GetData($"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?appid={appid}&steamid={SteamID}&key={APIKey}");
                JsonDocument achdata = JsonDocument.Parse(rawachdata);
                JsonElement achievements = achdata.RootElement.GetProperty("playerstats").GetProperty("achievements");
                int TotalAchievements = 0;
                int AchievementsEarned = 0;
                // for each element in playerstats -> achievements
                foreach (JsonElement achievement in achievements.EnumerateArray())
                {
                    TotalAchievements++; // increase total achievements in game
                                               // if achievement achieved = 1
                    if (achievement.GetProperty("achieved").GetInt32() == 1) // achievement -> achieved
                    {
                        AchievementsEarned++; // increase earned achievements counter
                    }
                }
                // Achievements Percent / Perfect Game
                double Percent = Math.Round(((double)AchievementsEarned / TotalAchievements) * 100);
                bool IsPerfectGame = (Percent == 100 ? true: false);

                return (AchievementsEarned, TotalAchievements, Percent, IsPerfectGame);
            }
            catch // as mentioned above, not every game has achievements!
            {
                return (0, 0, 0, false);
            }
        }
    }
}