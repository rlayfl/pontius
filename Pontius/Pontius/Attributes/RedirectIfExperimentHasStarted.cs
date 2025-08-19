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
        if (string.Equals(controller, "experiment", StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(action, "test", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(action, "end", StringComparison.OrdinalIgnoreCase)))
        {
            base.OnActionExecuting(context);
            return;
        }

        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var hasStartedTestClaim = user.FindFirst("HasExperimentStarted");
            if (hasStartedTestClaim != null &&
                bool.TryParse(hasStartedTestClaim.Value, out var hasStarted) &&
                hasStarted)
            {
                context.Result = new RedirectToActionResult("test", "experiment", null);
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}
