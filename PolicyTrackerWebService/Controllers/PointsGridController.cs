using PolicyTrackerWebService.Infrastructure.IService;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [Authorize, RoutePrefix("api/PointsGrid")]
    public class PointsGridController : BaseApiController
    {
        private readonly IPointsGridService _service;

        public PointsGridController(IPointsGridService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("GetAll")]
        public HttpResponseMessage GetAll(Domain.PointsGridLister mLister)
        {
            return Execute(() => _service.GetAll(mLister ?? new Domain.PointsGridLister()));
        }

        [HttpGet]
        [Route("GetActive")]
        public HttpResponseMessage GetActive()
        {
            return Execute(() => _service.GetActive());
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Upsert(Domain.PointsGrid mPointsGrid)
        {
            if (!IsAdmin) return AdminOnly();
            return Execute(() =>
            {
                if (mPointsGrid.Id <= 0) mPointsGrid.CreatedBy = CurrentUserId;
                mPointsGrid.LastModifiedBy = CurrentUserId;
                return _service.Upsert(mPointsGrid);
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
