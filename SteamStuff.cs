using System.Text.Json;
using System.Net.Http;

namespace GamesLibraryApp
{
    public class SteamGame : Game
    {

    }

    public class SteamStats : Stats
    {

    }
    public static class SteamStuff
    {
        private static string APIKey = "";
        private static long SteamID = 0;
        private static string GetOwnedGamesURL = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={APIKey}&steamid={SteamID}&format=json&include_appinfo=true&include_played_free_games=true";
        private static readonly HttpClient hc = new HttpClient();

        public static void Init()
        {
            hc.Timeout = TimeSpan.FromSeconds(10);
        }

        public static void Update()
        {
            string[] settings = Settings.GetSteamStuff();
            APIKey = settings[0];
            SteamID = long.Parse(settings[1]);
            GetOwnedGamesURL = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={APIKey}&steamid={SteamID}&format=json&include_appinfo=true&include_played_free_games=true";
        }
        public static async Task<bool> IsSteamAPIUp()
        {
            Update();
            InternetStuff ist = new InternetStuff();
            var response = await ist.IsSourceUp(GetOwnedGamesURL);
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

        public static async Task<(List<SteamGame> Games, SteamStats?)> Games()
        {
            if (await IsEverythingOK() == true)
            {
                InternetStuff ist = new InternetStuff();
                string rawdata = await ist.GetData(GetOwnedGamesURL);
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
                        steamgame.Icon = $"https://media.steampowered.com/steamcommunity/public/images/apps/{appid}/{game.GetProperty("img_icon_url").GetString()}.jpg";
                        // We try - catch because not every game that shows up actually has achievements or stats or anything of that nature.
                        // Additionally, not every 'game' in a user's library is a valid game (some have no data other than an appid, and aren't even counted towards the game total (I believe?)).
                        try
                        {
                            string rawachdata = await ist.GetData($"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?appid={appid}&steamid={SteamID}&key={APIKey}");
                            JsonDocument achdata = JsonDocument.Parse(rawachdata);
                            JsonElement achievements = achdata.RootElement.GetProperty("playerstats").GetProperty("achievements");
                            steamgame.TotalAchievements = 0;
                            steamgame.AchievementsEarned = 0;
                            // for each element in playerstats -> achievements
                            foreach (JsonElement achievement in achievements.EnumerateArray())
                            {
                                steamgame.TotalAchievements++; // increase total achievements in game
                                Stats.AchievementsTotal++; // increase total achievements overall
                                // if achievement achieved = 1
                                if (achievement.GetProperty("achieved").GetInt32() == 1) // achievement -> achieved
                                {
                                    steamgame.AchievementsEarned++; // increase earned achievements counter
                                    Stats.AchievementsEarnedTotal++; // increase total achievements earned stats
                                }
                            }
                            // Achievements Percent
                            steamgame.Percent = Math.Round(((double)steamgame.AchievementsEarned / steamgame.TotalAchievements) * 100);

                            // If percent = 100, perfect game
                            if (steamgame.Percent == 100)
                            {
                                steamgame.IsPerfectGame = true;
                                Stats.PerfectGames++;
                            }
                        }
                        catch // as mentioned above, not every game has achievements!
                        {

                        }
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
    }
}