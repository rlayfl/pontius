using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RedirectIfAuthenticatedAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            // Optional: Access claims if needed
            var username = user.Identity.Name;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            // Redirect to the home page if already authenticated
            context.Result = new RedirectToActionResult("Index", "Home", null);
            return; // Important: exit early so base.OnActionExecuting isn't executed unnecessarily
        }

        base.OnActionExecuting(context);
    }
}