using PolicyTrackerWebService.Infrastructure.IService;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [Authorize, RoutePrefix("api/Payout")]
    public class PayoutController : BaseApiController
    {
        private readonly IPayoutService _service;

        public PayoutController(IPayoutService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("GetAll")]
        public HttpResponseMessage GetAll(Domain.PayoutLister mLister)
        {
            return Execute(() => _service.GetAll(mLister ?? new Domain.PayoutLister()));
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Upsert(Domain.Payout mPayout)
        {
            return Execute(() =>
            {
                if (mPayout.Id <= 0) mPayout.CreatedBy = CurrentUserId;
                mPayout.LastModifiedBy = CurrentUserId;
                return _service.Upsert(mPayout);
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
    }
}
