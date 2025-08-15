using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RedirectIfNOTAuthenticatedAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            // Redirect to Login page if not authenticated
            context.Result = new RedirectToActionResult("Start", "Experiment", null);
            return;
        }

        // Proceed with the action if authenticated
        base.OnActionExecuting(context);
    }
}
