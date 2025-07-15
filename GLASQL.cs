using Microsoft.Data.Sqlite;

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
                using (var cmd = new SqliteCommand(games, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
