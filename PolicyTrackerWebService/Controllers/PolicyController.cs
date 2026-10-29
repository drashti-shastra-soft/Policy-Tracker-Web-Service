using PolicyTrackerWebService.Infrastructure.IService;
using System;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [Authorize, RoutePrefix("api/Policy")]
    public class PolicyController : BaseApiController
    {
        private readonly IPolicyService _service;

        public PolicyController(IPolicyService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("GetAll")]
        public HttpResponseMessage GetAll(Domain.PolicyLister mLister)
        {
            return Execute(() => _service.GetAll(mLister ?? new Domain.PolicyLister()));
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Upsert(Domain.Policy mPolicy)
        {
            return Execute(() =>
            {
                if (mPolicy.Id <= 0) mPolicy.CreatedBy = CurrentUserId;
                mPolicy.LastModifiedBy = CurrentUserId;
                return _service.Upsert(mPolicy);
            });
        }

        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            return Execute(() => _service.Get(id));
        }

        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            return ExecuteNoContent(() => _service.Delete(id, CurrentUserId));
        }

        /// <summary>Records the renewal outcome (quote sent / renewed / lost ...).</summary>
        [HttpPost]
        [Route("FollowUp")]
        public HttpResponseMessage FollowUp(Domain.RenewalFollowUp mFollowUp)
        {
            return Execute(() =>
            {
                mFollowUp.LastModifiedBy = CurrentUserId;
                return _service.FollowUp(mFollowUp);
            });
        }

        /// <summary>Policies expiring in the month that are not renewed yet.</summary>
        [HttpGet]
        [Route("DueList")]
        public HttpResponseMessage DueList(DateTime? month = null, int? brokerId = null, bool includeClosed = false)
        {
            return Execute(() => _service.GetDueList(month ?? DateTime.Today, brokerId, includeClosed));
        }
    }
}
