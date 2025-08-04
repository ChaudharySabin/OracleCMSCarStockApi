using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace api.Handlers
{
    public class MustBeOwnUserHandler : AuthorizationHandler<MustBeOwnUserRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustBeOwnUserRequirement requirement)
        {
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.Resource is HttpContext httpContext)
            {
                var routeData = httpContext.GetRouteData();
                if (routeData.Values["id"] is string idString
                && int.TryParse(idString, out var routeId)
                && int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                {
                    if (routeId == userId)
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail(); //Not same userId
                    }
                }
                else
                {
                    context.Fail(); //Cannot parse userID
                }
            }
            else
            {
                context.Fail(); //Not HttpContext
            }

            return Task.CompletedTask;
        }
    }
}