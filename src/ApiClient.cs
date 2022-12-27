using System.Net;
using System.Net.Http.Json;
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


        public record class ApiRequest<T>(string method, T @params) { };
        public record class Envelope<T>(int code, string msg, T resData) { };
        public record Unit() { };

        private async Task<R> Post<T, R>(string methodName, int okCode, T arg) where R : class
        {
            var request = new ApiRequest<T>(methodName, arg);
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var response = await this.httpClient.PostAsJsonAsync<ApiRequest<T>>(string.Empty, request, options);
            response = response.EnsureSuccessStatusCode();
            if (response != null)
            {
                var result = await response.Content.ReadFromJsonAsync<Envelope<R>>();
                if (result?.code != okCode)
                    throw new Exception(result?.msg);
                return result?.resData!;
            }
            else throw new Exception(response?.ToString());
        }

        // login service

        public record class LoginParams(string user, string pass, [property: JsonPropertyName("case-insensitive")] bool caseInsensitive) { };
        public record class LoginResponse(int customerId, int customerNo, int accountId, string tfa, string buildDate, int version) { }

        public async Task<LoginResponse> AccountLogin(string user, string password, bool caseInsensitive = false)
        => await Post<LoginParams, LoginResponse>("account.login", 1000, new LoginParams(user, password, caseInsensitive));

        public async Task<Unit> AccountLogout()
        => await Post<Unit, Unit>("account.logout", 1500, new Unit());

        // Retrieve all nameserver info including the nameserver records 

        public record class NameServerInfoParams(string domain) { };
        public record class NameServerInfoResponse(
            int roId,
            string domain,
            string type,
            int count,
            [property: JsonPropertyName("record")]
            NameServerRecord[] Records
        )
        { };

        public record class NameServerRecord(
            int id,
            string name,
            string type,
            string content,
            int ttl,
            int prio
        )
        { };

        public async Task<NameServerInfoResponse> NameServerIno(string domain)
        => await Post<NameServerInfoParams, NameServerInfoResponse>("nameserver.info", 1000, new NameServerInfoParams(domain));

        // Creating a new nameserver entry 
        public record class NameServerCreateRecordParams(int roId, string type, string name, string content, int ttl = 3600, int prio = 0) { };
        public record class NameServerCreateRecordResponse(int id) { }

        public async Task<int> NameServerCreateRecord(int roId, string type, string name, string content, int ttl = 3600, int prio = 0)
        => (await Post<NameServerCreateRecordParams, NameServerCreateRecordResponse>("nameserver.createRecord", 1000,
                  new NameServerCreateRecordParams(roId, type, name, content, ttl, prio))).id;

        // Update an existing nameserver entry

        public record class NameServerUpdateRecordParams(int id, string type, string name, string content) { };

        public async Task<Unit> NameServerUpdateRecord(int id, string type, string name, string content)
        => await Post<NameServerUpdateRecordParams, Unit>("nameserver.updateRecord", 1000,
                new NameServerUpdateRecordParams(id, type, name, content));
    }
}