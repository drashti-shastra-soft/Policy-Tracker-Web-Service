using PolicyTrackerWebService.Infrastructure.IService;
using System;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [Authorize, RoutePrefix("api/Statement")]
    public class StatementController : BaseApiController
    {
        private readonly IStatementService _service;

        public StatementController(IStatementService service)
        {
            _service = service;
        }

        /// <summary>Imports the broker's commission Excel (base64) and auto-matches it to policies.</summary>
        [HttpPost]
        [Route("Import")]
        public HttpResponseMessage Import(Domain.StatementImport mImport)
        {
            return Execute(() =>
            {
                mImport.CreatedBy = CurrentUserId;
                return _service.Import(mImport);
            });
        }

        [HttpPost]
        [Route("GetAll")]
        public HttpResponseMessage GetAll(Domain.StatementLineLister mLister)
        {
            return Execute(() => _service.GetAll(mLister ?? new Domain.StatementLineLister()));
        }

        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            return Execute(() => _service.Get(id));
        }

        [HttpPost]
        [Route("Link")]
        public HttpResponseMessage Link(Domain.StatementLink mLink)
        {
            return Execute(() =>
            {
                mLink.LastModifiedBy = CurrentUserId;
                return _service.Link(mLink);
            });
        }

        [HttpPost]
        [Route("{id:int}/CreatePolicy")]
        public HttpResponseMessage CreatePolicy(int id)
        {
            return Execute(() => _service.CreatePolicyFromLine(id, CurrentUserId));
        }

        [HttpPost]
        [Route("Rematch/{brokerId:int}")]
        public HttpResponseMessage Rematch(int brokerId)
        {
            return Execute(() => _service.Rematch(brokerId, CurrentUserId));
        }

        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            return ExecuteNoContent(() => _service.Delete(id));
        }

        [HttpDelete]
        [Route("Month")]
        public HttpResponseMessage DeleteMonth(int brokerId, DateTime month)
        {
            if (!IsAdmin) return AdminOnly();
            return Execute(() => _service.DeleteMonth(brokerId, month));
        }

        [HttpGet]
        [Route("Months")]
        public HttpResponseMessage Months(int? brokerId = null)
        {
            return Execute(() => _service.GetMonths(brokerId));
        }
    }
}
