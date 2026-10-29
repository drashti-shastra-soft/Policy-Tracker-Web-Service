using PolicyTrackerWebService.Infrastructure.IService;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [Authorize, RoutePrefix("api/Category")]
    public class CategoryController : BaseApiController
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("GetAll")]
        public HttpResponseMessage GetAll(Domain.CategoryLister mLister)
        {
            return Execute(() => _service.GetAll(mLister ?? new Domain.CategoryLister()));
        }

        [HttpGet]
        [Route("GetActive")]
        public HttpResponseMessage GetActive()
        {
            return Execute(() => _service.GetActive());
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Upsert(Domain.Category mCategory)
        {
            if (!IsAdmin) return AdminOnly();
            return Execute(() =>
            {
                if (mCategory.Id <= 0) mCategory.CreatedBy = CurrentUserId;
                mCategory.LastModifiedBy = CurrentUserId;
                return _service.Upsert(mCategory);
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
