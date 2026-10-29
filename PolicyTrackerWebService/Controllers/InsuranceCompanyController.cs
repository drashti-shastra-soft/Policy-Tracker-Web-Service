using PolicyTrackerWebService.Infrastructure.IService;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [Authorize, RoutePrefix("api/InsuranceCompany")]
    public class InsuranceCompanyController : BaseApiController
    {
        private readonly IInsuranceCompanyService _service;

        public InsuranceCompanyController(IInsuranceCompanyService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("GetAll")]
        public HttpResponseMessage GetAll(Domain.InsuranceCompanyLister mLister)
        {
            return Execute(() => _service.GetAll(mLister ?? new Domain.InsuranceCompanyLister()));
        }

        [HttpGet]
        [Route("GetActive")]
        public HttpResponseMessage GetActive()
        {
            return Execute(() => _service.GetActive());
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Upsert(Domain.InsuranceCompany mInsuranceCompany)
        {
            if (!IsAdmin) return AdminOnly();
            return Execute(() =>
            {
                if (mInsuranceCompany.Id <= 0) mInsuranceCompany.CreatedBy = CurrentUserId;
                mInsuranceCompany.LastModifiedBy = CurrentUserId;
                return _service.Upsert(mInsuranceCompany);
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
