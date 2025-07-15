using Microsoft.Data.Sqlite;
using Microsoft.Maui.ApplicationModel.Communication;
using System;

namespace GamesLibraryApp
{
    public static class GLASQL
    {
        public static SqliteConnection Connect()
        {
            return new SqliteConnection("Data Source=database.db;");
        }

        public static void Init()
        {
            if (!File.Exists("database.db"))
            {
                using (var con = Connect())
                {
                    con.Open();
                    con.Close();
                }
            }
            CreateTable();
        }

        public static void CreateTable()
        {
            string games = "CREATE TABLE IF NOT EXISTS games (id INTEGER PRIMARY KEY AUTOINCREMENT, appid INTEGER, name TEXT, platform TEXT, playtime INTEGER, achearned INTEGER, totalach INTEGER, image TEXT)";
            using (var con = Connect())
            {
                con.Open();
                using (var cmd = new SqliteCommand(games, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteTable()
        {
            string games = "DROP TABLE games";
            using (var con = Connect())
            {
                con.Open();
                using (var cmd = new SqliteCommand(games, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void Reset()
        {
            DeleteTable();
            CreateTable();
        }

        public static bool DoesGameExist(Game game)
        {
            if (game == null) return false;
            string g = "SELECT * from games WHERE name = @name";
            using (var cmd = new SqliteCommand(g, Connect()))
            {
                cmd.Parameters.AddWithValue("@name", game.Name);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count >= 1)
                {
                    return true;
                }
            }
            return false;
        }
        public static void AddGame(Game game)
        {
            if (DoesGameExist(game))
            {
                UpdateGame(game);
            }
            else
            {
                string g = "INSERT INTO games (appid, name, platform, playtime, achearned, totalach, image) VALUES (@appid, @name, @platform, @playtime, @achearned, @totalach, @image)";
                using (var cmd = new SqliteCommand(g, Connect()))
                {
                    cmd.Parameters.AddWithValue("@name", game.Name);
                    cmd.Parameters.AddWithValue("@platform", "Steam"); // eventually support will be added into the Game class to specify platform
                    cmd.Parameters.AddWithValue("@playtime", game.Playtime);
                    cmd.Parameters.AddWithValue("@achearned", game.AchievementsEarned);
                    cmd.Parameters.AddWithValue("@totalach", game.TotalAchievements);
                    cmd.Parameters.AddWithValue("@image", ""); // todo
                    cmd.ExecuteNonQuery();
                }
            }

        }
        public static void UpdateGame(Game game)
        {
            if (!DoesGameExist(game)) return;
            string g = "UPDATE games SET appid = @appid, name = @name, platform = @platform, playtime = @playtime, achearned = @achearned, totalach = @totalach, image = @image WHERE name = @name";
            using (var cmd = new SqliteCommand(g, Connect()))
            {
                cmd.Parameters.AddWithValue("@name", game.Name);
                cmd.Parameters.AddWithValue("@platform", "Steam"); // eventually support will be added into the Game class to specify platform
                cmd.Parameters.AddWithValue("@playtime", game.Playtime);
                cmd.Parameters.AddWithValue("@achearned", game.AchievementsEarned);
                cmd.Parameters.AddWithValue("@totalach", game.TotalAchievements);
                cmd.Parameters.AddWithValue("@image", ""); // todo
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteGame(Game game)
        {
            if (!DoesGameExist(game)) return;
            string g = "DELETE FROM games WHERE name = @name";
            using (var cmd = new SqliteCommand(g, Connect()))
            {
                cmd.Parameters.AddWithValue("@name", game.Name);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
