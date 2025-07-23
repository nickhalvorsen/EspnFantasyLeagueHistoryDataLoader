using System.Net;
using Azure.Identity;
using EspnFantasyLeagueHistoryDataLoader.src.Context;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, configBuilder) =>
    {
        configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        //configBuilder.AddEnvironmentVariables();

        var keyVaultUri = Environment.GetEnvironmentVariable("KEY_VAULT_URL");
        Console.WriteLine($"Using Key Vault URL: {keyVaultUri}");
        if (string.IsNullOrEmpty(keyVaultUri))
        {
            throw new ArgumentException("Missing Key Vault URI in environment variable 'KEY_VAULT_URL'.");
        }

        // DefaultAzureCredential attempts different methods of authentication.
        // On the hosted function, it will use the managed identity of the function app.
        // Locally, it will use the Azure CLI or Visual Studio credentials.
        configBuilder.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
    })
     .ConfigureServices((context, s) =>
    {
        s.AddApplicationInsightsTelemetryWorkerService();
        s.ConfigureFunctionsApplicationInsights();
        s.AddHttpClient("EspnApiHttpClient", client =>
        {
            var baseUrl = context.Configuration["ESPN_API_BASE_URL"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentException("Missing ESPN API base URL in configuration.");
            }

            client.BaseAddress = new Uri(baseUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var cookieContainer = new CookieContainer();

            var baseUrl = context.Configuration["ESPN_API_BASE_URL"];

            // Set the default cookie for the base URI
            cookieContainer.Add(new Uri(baseUrl), new Cookie("SWID", context.Configuration["EspnSwidCookie"]));
            cookieContainer.Add(new Uri(baseUrl), new Cookie("espn_s2", context.Configuration["EspnS2Cookie"]));

            return new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
            };
        });

        s.Configure<LoggerFilterOptions>(options =>
        {
            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
            // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
            LoggerFilterRule? toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });

        s.AddScoped<DataLoader>();

        s.AddDbContext<XflLeagueHistoryContext>((options) =>
        {
            options.UseSqlServer(context.Configuration["DatabaseConnectionString"]);
        });
    })
    .Build();

host.Run();