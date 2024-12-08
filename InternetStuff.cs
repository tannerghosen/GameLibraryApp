using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GamesLibraryApp
{
    public class TokenResponse
    {
        public string refreshtoken { get; set; }
        public string accesstoken { get; set; }
    }
    public class InternetStuff
    {
        private readonly HttpClient hc = new HttpClient();
        public async Task<string> GetData(string url)
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

        public async Task<bool> IsSourceUp(string url)
        {
            try
            {
                var response = await hc.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Mockup code to get access into some APIs via OAuth.
        public async Task<(string, string)> Login(string url, string username, string password)
        {
            try
            {
                var response = await hc.PostAsync(url, new StringContent(JsonSerializer.Serialize(new { username = username, password = password }), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<TokenResponse>(await response.Content.ReadAsStringAsync());
                    return (data.refreshtoken, data.accesstoken);
                }
                return (null, null);
            }
            catch
            {
                return (null, null);
            }
        }
        public InternetStuff()
        {
            hc.Timeout = TimeSpan.FromSeconds(10);
        }
    }
}
