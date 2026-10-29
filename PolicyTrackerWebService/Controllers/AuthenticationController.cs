using PolicyTrackerWebService.Infrastructure.IService;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [AllowAnonymous, RoutePrefix("api/Authentication")]
    public class AuthenticationController : BaseApiController
    {
        private readonly IAuthenticationService _service;

        public AuthenticationController(IAuthenticationService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post(Domain.Credential mCredential)
        {
            return Execute(() => _service.Authenticate(mCredential));
        }
    }
}
