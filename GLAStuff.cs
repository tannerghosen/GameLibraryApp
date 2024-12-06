namespace GamesLibraryApp
{
    public class Game
    {
        public string Name { get; set; }
        public string Icon { get; set; }
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
}
