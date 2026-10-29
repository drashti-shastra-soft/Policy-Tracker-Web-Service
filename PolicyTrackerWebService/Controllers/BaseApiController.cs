using PolicyTrackerWebService.Infrastructure.CustomException;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    /// <summary>
    /// Common response handling (same status codes as the AttendEdge controllers)
    /// and access to the logged-in user from the JWT.
    /// </summary>
    public abstract class BaseApiController : ApiController
    {
        protected int CurrentUserId
        {
            get
            {
                var principal = User as ClaimsPrincipal;
                var claim = principal == null ? null : principal.Claims.FirstOrDefault(c => c.Type == "userId");
                int id;
                return claim != null && int.TryParse(claim.Value, out id) ? id : 0;
            }
        }

        protected bool IsAdmin
        {
            get
            {
                var principal = User as ClaimsPrincipal;
                return principal != null && principal.Claims.Any(c =>
                    (c.Type == "role" || c.Type == ClaimTypes.Role) && string.Equals(c.Value, "Admin", StringComparison.OrdinalIgnoreCase));
            }
        }

        protected HttpResponseMessage Execute(Func<object> action)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, action());
            }
            catch (APIRequestFailedException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        protected HttpResponseMessage ExecuteNoContent(Action action)
        {
            try
            {
                action();
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (APIRequestFailedException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        protected HttpResponseMessage AdminOnly()
        {
            return Request.CreateResponse(HttpStatusCode.Forbidden, "Only an administrator can do this!");
        }

    }
}
