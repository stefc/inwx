using System.Collections.Immutable;
using System.Globalization;

using Microsoft.Extensions.Configuration;

using stefc.inwx;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var domainName = args.FirstOrDefault("example.com");

        var config = new ConfigurationBuilder()
			.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			.AddJsonFile("appsettings.json")
			.AddUserSecrets<Program>()
			.Build();

        // The credentials stored as user-secrets
        var user = config["inwx-user"] ?? "<unknown>";
        var password = config["inwx-password"] ?? "<unknown>";

        ApiClient apiClient = new ApiClient(ApiClient.API_URL_LIVE, Language.EN);
        var response = await apiClient.AccountLogin(user,password, false); 

        // Show my customer no at INWX
        System.Console.WriteLine($"Customer-No:{response.customerNo}");

        // List all DNS Records of my Domain 'example.com'
        var domainInfo = await apiClient.NameServerIno(domainName);
        foreach(var rec in domainInfo.Records) {
            System.Console.WriteLine($"{rec.id}\t{rec.name}\t{rec.type}\t{rec.content}\t{rec.ttl}\t{rec.prio}");
        }

        var recAbc = domainInfo.Records.SingleOrDefault( r => r.name.StartsWith("abc"));

        if (recAbc != null) 
        {
            await apiClient.NameServerUpdateRecord(recAbc.id, recAbc.type, recAbc.name, "Hello World!");
            System.Console.WriteLine("Record updated");
        }
        else
        {
            // Add a DNS Entry 
            var recordId = await apiClient.NameServerCreateRecord(domainInfo.roId, "TXT", "abc", "xyz");
            System.Console.WriteLine($"Added new record with Id:{recordId}");
        }



        // Logout from Service
        await apiClient.AccountLogout();
    }
}