using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

public class EndExperiment : ActionFilterAttribute
{

    // This is currently not used anywhere but might prove useful in future
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            // Sign out the user
            var signOutTask = context.HttpContext.SignOutAsync(); // Default scheme, or specify "Cookies"
            signOutTask.Wait();

            // Redirect to the home page
            context.Result = new RedirectToActionResult("Index", "Home", null);
            return;
        }

        base.OnActionExecuting(context);
    }
}