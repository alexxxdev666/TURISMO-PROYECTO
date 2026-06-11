using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Turismo.Security;

public class AdminOnlyAttribute : ActionFilterAttribute
{
    public const string AdminEmail = "admin@turismo.local";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");
        var userEmail = session.GetString("UserEmail");

        if (!userId.HasValue || !string.Equals(userEmail, AdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                ["controller"] = "Home",
                ["action"] = "AccessDenied",
                ["returnUrl"] = returnUrl
            });
        }
    }
}
