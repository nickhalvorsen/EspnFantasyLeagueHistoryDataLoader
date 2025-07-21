using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace XflDataLoader;

public class Functions(ILogger<Functions> logger, DataLoader dataLoader)
{
    private readonly ILogger<Functions> _logger = logger;
    private readonly DataLoader _dataLoader = dataLoader;

    [Function("LoadAllData")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        await _dataLoader.LoadAllData();

        _logger.LogDebug("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}