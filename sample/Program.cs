using System.Collections.Immutable;
using System.Globalization;

using Microsoft.Extensions.Configuration;

using stefc.inwx;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
			.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			.AddJsonFile("appsettings.json")
			.AddUserSecrets<Program>()
			.Build();

        var user = config["inwx-user"] ?? "<unknown>";
        var password = config["inwx-password"] ?? "<unknown>";

        ApiClient apiClient = new ApiClient(ApiClient.API_URL_LIVE, Language.EN);
        var response = await apiClient.AccountLogin(user,password, false); 

        System.Console.WriteLine($"Customer-No:{response.customerNo}");

        await apiClient.AccountLogout();
    }

    
}