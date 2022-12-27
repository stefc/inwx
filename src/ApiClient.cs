using System;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http;


using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;

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

        private readonly HttpClient httpClient;

        public static string generateClientTransactionId()
        {
            return $"DomRobot-{new Random().Next(1000000000)}";
        }

        public ApiClient(string apiUrl, Language lang)
        {
            this.apiUrl = apiUrl;
            this.language = lang == Language.DE ? "DE" : "EN";
            this.httpClient = BuildClient();
            this.httpClient.BaseAddress = new Uri(this.apiUrl);
            this.httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IWebProxy Proxy { get; private set; } = null;

        private HttpClient BuildClient()
        {
            if (this.Proxy == null)
            {
                var client = new HttpClient();
                
                return client;
            }

            var handler = new HttpClientHandler();
            handler.Proxy = this.Proxy;
            
            return new HttpClient(handler);
        }


        public record ApiRequest<T>(string method, T @params) { };
        public record class Envelope<T>(int code, string msg, T resData) {};
        public record Unit() { };

        public record LoginParams(string user, string pass, [property: JsonPropertyName("case-insensitive")] bool caseInsensitive) { };
        public record LoginResponse(int customerId, int customerNo, int accountId, string tfa, string buildDate, int version) { }
        // "method": "account.login",
        // "params": {"user":"blakeks", "pass":"SCHEDULED", "lang":"de", "clTRID":"CLIENT-123123"}

        public async Task<LoginResponse> AccountLogin(string user, string password, bool caseInsensitive = false)
        {
            var login = new ApiRequest<LoginParams>("account.login", new LoginParams(user, password, caseInsensitive));
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var response = await this.httpClient.PostAsJsonAsync<ApiRequest<LoginParams>>(string.Empty, login, options);
            response = response.EnsureSuccessStatusCode();
            if (response != null) {
                var result = await response.Content.ReadFromJsonAsync<Envelope<LoginResponse>>();
                if (result.code != 1000) 
                    throw new Exception(result.msg);
                return result?.resData;
            }
            else throw new Exception(response.ToString());
        }

        public async Task<Unit> AccountLogout() 
        {
            var logout = new ApiRequest<Unit>("account.logout", new Unit());
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var response = await this.httpClient.PostAsJsonAsync<ApiRequest<Unit>>(string.Empty, logout, options);
            response = response.EnsureSuccessStatusCode();
            if (response != null) {
                var result = await response.Content.ReadFromJsonAsync<Envelope<Unit>>();
                if (result.code != 1500) 
                    throw new Exception(result.msg);
                return result?.resData;
            }
            else throw new Exception(response.ToString());
        }


    }
}
