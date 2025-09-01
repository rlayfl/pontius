using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class LogPageVisitToFirebase : IAsyncActionFilter
{
    private readonly string _firebasepageVisitsTableEndpoint =
        "https://pontius-b5de5-default-rtdb.europe-west1.firebasedatabase.app/pageVisits.json";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var skip = context.ActionDescriptor.EndpointMetadata
            .OfType<SkipGlobalFiltersAttribute>()
            .Any();

        // Always run the action
        var executed = await next();

        if (skip || executed.Canceled || executed.Exception != null)
            return;

        await next();

        var requestPath = context.HttpContext.Request.Path.Value?.Trim('/').ToLowerInvariant();
        if (requestPath == "statistics/logclick")
        {
            return;
        }

        var payload = new
        {
            username = "TestUsername",
            password = "TestPassword",
            clickTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            url = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString
        };

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.HttpContext.RequestAborted);
        var response = await httpClient.PostAsync(_firebasepageVisitsTableEndpoint, content, cts.Token);
        response.EnsureSuccessStatusCode();
    }
}
