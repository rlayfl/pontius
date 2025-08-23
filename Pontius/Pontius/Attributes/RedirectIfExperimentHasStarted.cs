using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RedirectIfExperimentHasStarted : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var routeValues = context.RouteData.Values;
        var controller = routeValues["controller"]?.ToString();
        var action = routeValues["action"]?.ToString();
        if (string.Equals(controller, "buoys", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(action, "overview", StringComparison.OrdinalIgnoreCase))
        {
            base.OnActionExecuting(context);
            return;
        }

        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var hasStartedTestClaim = user.FindFirst("HasStartedExperiment");
            if (hasStartedTestClaim != null &&
                bool.TryParse(hasStartedTestClaim.Value, out var hasStarted) &&
                hasStarted)
            {
                context.Result = new RedirectToActionResult("overview", "buoys", null);
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}
