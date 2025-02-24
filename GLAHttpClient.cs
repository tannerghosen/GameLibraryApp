namespace GamesLibraryApp
{
    public static class GLAHttpClient
    {
        private static readonly HttpClient hc = new HttpClient();
        public static async Task<string> GetData(string url)
        {
            try
            {
                var response = await hc.GetStringAsync(url);
                //Console.WriteLine("JSON Success");
                return response;
            }
            catch (HttpRequestException e)
            {
                //Console.WriteLine("JSON Failure");
                //Console.WriteLine(e.Message);
                return null;
            }
        }

        public static async Task<bool> IsUrlUp(string url)
        {
            try
            {
                var response = await hc.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                Console.Error.WriteLine("ERROR: IsUrlUp: Failed to connect to the url at " + url + " .");
                return false;
                
            }
        }

        static GLAHttpClient()
        {
            hc.Timeout = TimeSpan.FromSeconds(10);
        }
    }
}
