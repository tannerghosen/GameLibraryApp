﻿using System.Text.Json;
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
            var response = await InternetStuff.IsSourceUp(GetOwnedGamesURL);
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
                string rawdata = await InternetStuff.GetData(GetOwnedGamesURL);
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
                string rawachdata = await InternetStuff.GetData($"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?appid={appid}&steamid={SteamID}&key={APIKey}");
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