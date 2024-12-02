﻿using System.Text.Json;
using System.Net.Http;

namespace GamesLibraryApp
{
    public class SteamGame
    {
        public string Name { get; set; }
        public string Icon { get; set; }

        public int AppId { get; set; }
        public double Playtime { get; set; }
        public int AchievementsEarned { get; set; }
        public int TotalAchievements { get; set; }
        public double Percent { get; set; }
        public bool IsPerfectGame { get; set; }
    }

    public class Stats
    {
        public int TotalGames { get; set; }
        public double TotalPlaytime { get; set; }
        public int AchievementsEarnedTotal { get; set; }
        public int AchievementsTotal { get; set; }
        public int PerfectGames { get; set; }
    }
    public static class SteamStuff
    {
        private static string APIKey = "";
        private static long SteamID = 0;
        private static string GetOwnedGamesURL = "";
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
            GetOwnedGamesURL = settings[2];
        }
        public static async Task<bool> IsSteamAPIUp()
        {
            Update();
            try
            {
                var response = await hc.GetAsync(GetOwnedGamesURL);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string> GetData(string url)
        {
            try
            {
                var JsonResponse = await hc.GetStringAsync(url);
                //Console.WriteLine("JSON Success");
                return JsonResponse;
            }
            catch (HttpRequestException e)
            {
                //Console.WriteLine("JSON Failure");
                //Console.WriteLine(e.Message);
                return null;
            }
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

        public static async Task<(SteamGame[]? Games, Stats? Stats)> Games()
        {
            if (await IsEverythingOK() == true)
            {
                string rawdata = await GetData(GetOwnedGamesURL);
                if (rawdata != null)
                {
                    JsonDocument data = JsonDocument.Parse(rawdata);
                    JsonElement games = data.RootElement.GetProperty("response").GetProperty("games");
                    Stats Stats = new Stats();
                    List<SteamGame> Games = new List<SteamGame>();
                    // for each element in games
                    foreach (JsonElement game in games.EnumerateArray())
                    {
                        SteamGame steamgame = new SteamGame();
                        steamgame.Name = game.GetProperty("name").GetString(); // game -> name
                        steamgame.Playtime = Math.Round(game.GetProperty("playtime_forever").GetDouble() / 60, 2);
                        Stats.TotalPlaytime += steamgame.Playtime;
                        steamgame.AppId = game.GetProperty("appid").GetInt32(); // game -> appid
                        steamgame.Icon = $"https://media.steampowered.com/steamcommunity/public/images/apps/{steamgame.AppId}/{game.GetProperty("img_icon_url").GetString()}.jpg";
                        // We try - catch because not every game that shows up actually has achievements or stats or anything of that nature.
                        // Additionally, not every 'game' in a user's library is a valid game (some have no data other than an appid, and aren't even counted towards the game total (I believe?)).
                        try
                        {
                            string rawachdata = await GetData($"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?appid={steamgame.AppId}&steamid={SteamID}&key={APIKey}");
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
                    return (Games.ToArray(), Stats);
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