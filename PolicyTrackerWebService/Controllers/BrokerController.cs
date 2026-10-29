using PolicyTrackerWebService.Infrastructure.IService;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [Authorize, RoutePrefix("api/Broker")]
    public class BrokerController : BaseApiController
    {
        private readonly IBrokerService _service;

        public BrokerController(IBrokerService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("GetAll")]
        public HttpResponseMessage GetAll(Domain.BrokerLister mLister)
        {
            return Execute(() => _service.GetAll(mLister ?? new Domain.BrokerLister()));
        }

        [HttpGet]
        [Route("GetActive")]
        public HttpResponseMessage GetActive()
        {
            return Execute(() => _service.GetActive());
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Upsert(Domain.Broker mBroker)
        {
            if (!IsAdmin) return AdminOnly();
            return Execute(() =>
            {
                if (mBroker.Id <= 0) mBroker.CreatedBy = CurrentUserId;
                mBroker.LastModifiedBy = CurrentUserId;
                return _service.Upsert(mBroker);
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
            if (!IsAdmin) return AdminOnly();
            return ExecuteNoContent(() => _service.Delete(id, CurrentUserId));
        }
    }
}
