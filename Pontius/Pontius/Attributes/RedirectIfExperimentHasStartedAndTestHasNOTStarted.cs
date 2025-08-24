using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RedirectIfExperimentHasStartedAndTestHasNOTStarted : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        //Skip this filter
        if (context.ActionDescriptor.EndpointMetadata.OfType<SkipGlobalFiltersAttribute>().Any())
            return;

        var routeValues = context.RouteData.Values;
        var controller = routeValues["controller"]?.ToString();
        var action = routeValues["action"]?.ToString();
        // Exclude buoys/overview, account/logout, experiment/end, and experiment/setTestHasStarted
        if ((string.Equals(controller, "buoys", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(action, "overview", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(controller, "account", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(action, "logout", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(controller, "experiment", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(action, "end", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(controller, "experiment", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(action, "setTestHasStarted", StringComparison.OrdinalIgnoreCase)))
        {
            base.OnActionExecuting(context);
            return;
        }

        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var hasStartedExperimentClaim = user.FindFirst("HasStartedExperiment");
            var hasStartedTestClaim = user.FindFirst("HasStartedTest");

            bool HasTrue(string? s) => string.Equals(s?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

            if (HasTrue(hasStartedExperimentClaim?.Value) &&
                !HasTrue(hasStartedTestClaim?.Value))
            {
                context.Result = new RedirectToActionResult("overview", "buoys", null);
                return;
            }

        }

        base.OnActionExecuting(context);
    }
}
