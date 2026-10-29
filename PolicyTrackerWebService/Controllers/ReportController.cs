using PolicyTrackerWebService.Infrastructure.IService;
using System;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [Authorize, RoutePrefix("api/Report")]
    public class ReportController : BaseApiController
    {
        private readonly IReportService _service;

        public ReportController(IReportService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("Dashboard")]
        public HttpResponseMessage Dashboard(DateTime? month = null, int? brokerId = null)
        {
            return Execute(() => _service.GetDashboard(month ?? DateTime.Today, brokerId));
        }

        [HttpGet]
        [Route("Reconciliation")]
        public HttpResponseMessage Reconciliation(int? brokerId = null)
        {
            return Execute(() => _service.GetReconciliation(brokerId));
        }
    }
}
