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
        return new OkObjectResult("Data loaded successfully.");
    }

    [Function("FetchData")]
    [ResponseCache(Duration = 60 * 60 * 8)]  // Cache for 8 hours
    public async Task<IActionResult> FetchDataAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        var data = await _dataLoader.GetAllData();

        return new OkObjectResult(data);
    }
}