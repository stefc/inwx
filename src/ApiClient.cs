using System;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http;


using System.Threading.Tasks;
using System.Text.Json;

namespace stefc.inwx
{


    public class LoginResponse { };

    public enum Language { EN, DE };
    public class ApiClient
    {
        public const string API_URL_LIVE = "https://api.domrobot.com/jsonrpc/";
        public const string API_URL_OTE = "https://api.ote.domrobot.com/jsonrpc/";

        private readonly string apiUrl;
        private readonly string language;

        public static string generateClientTransactionId()
        {
            return $"DomRobot-{new Random().Next(1000000000)}";
        }

        public ApiClient(string apiUrl, Language lang)
        {
            this.apiUrl = apiUrl;
            this.language = lang == Language.DE ? "DE" : "EN";
        }

        public IWebProxy Proxy { get; private set; } = null;

        private HttpClient BuildClient()
        {
            if (this.Proxy == null)
                return new HttpClient();

            var handler = new HttpClientHandler();
            handler.Proxy = this.Proxy;
            return new HttpClient(handler);
        }


        public record ApiRequest<T>(string method, T @params) { };
        public record LoginParams(string user, string pass) { };
        public record LoginResponse(int customerId, int customerNo, int accountId, string tfa, string buildDate, int version) { }
        // "method": "account.login",
        // "params": {"user":"blakeks", "pass":"SCHEDULED", "lang":"de", "clTRID":"CLIENT-123123"}

        public async Task<LoginResponse> AccountLogin(string user, string password)
        {

            var login = new ApiRequest<LoginParams>("account.login", new LoginParams(user, password));
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var response = await BuildClient().PostAsJsonAsync<ApiRequest<LoginParams>>("https://localhost:12345/stocks/", login, options);
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
    }
}
