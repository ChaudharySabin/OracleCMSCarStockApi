using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace api.Handlers
{
    public class MustHaveSameDealerIdHandler : AuthorizationHandler<MustHaveSameDealerIdRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustHaveSameDealerIdRequirement requirement)
        {
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.Resource is HttpContext httpContext)
            {
                var routeData = httpContext.GetRouteData();
                if (routeData.Values["id"] is string idString && int.TryParse(idString, out int routeId) && int.TryParse(context.User.FindFirstValue("dealerId"), out int dealerId))
                {
                    if (routeId == dealerId)
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail(); //Different dealer cannot access
                    }
                }
                else
                {
                    context.Fail(); //Cannot parse id from either route or dealer i.e dealerId wasn't set for the dealer or wasn't passed for the route
                }
            }
            else
            {
                context.Fail(); //Not http context
            }

            return Task.CompletedTask;
        }
    }
}