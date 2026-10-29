using PolicyTrackerWebService.Infrastructure.IService;
using System.Net.Http;
using System.Web.Http;

namespace PolicyTrackerWebService.Controllers
{
    [Authorize, RoutePrefix("api/User")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("GetAll")]
        public HttpResponseMessage GetAll(Domain.UserLister mLister)
        {
            if (!IsAdmin) return AdminOnly();
            return Execute(() => _service.GetAll(mLister ?? new Domain.UserLister()));
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Upsert(Domain.User mUser)
        {
            if (!IsAdmin) return AdminOnly();
            return Execute(() =>
            {
                if (mUser.Id <= 0) mUser.CreatedBy = CurrentUserId;
                mUser.LastModifiedBy = CurrentUserId;
                return _service.Upsert(mUser);
            });
        }

        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            if (!IsAdmin && id != CurrentUserId) return AdminOnly();
            return Execute(() => _service.Get(id));
        }

        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!IsAdmin) return AdminOnly();
            return ExecuteNoContent(() => _service.Delete(id, CurrentUserId));
        }

        [HttpGet]
        [Route("Roles")]
        public HttpResponseMessage Roles()
        {
            return Execute(() => _service.GetRoles());
        }

        [HttpPost]
        [Route("ChangePassword")]
        public HttpResponseMessage ChangePassword(Domain.ChangePassword mChangePassword)
        {
            return ExecuteNoContent(() =>
            {
                if (mChangePassword == null || mChangePassword.NewPassword != mChangePassword.ConfirmPassword)
                    throw new Infrastructure.CustomException.APIRequestFailedException("New password and confirm password do not match!");

                _service.ChangePassword(CurrentUserId, mChangePassword.CurrentPassword, mChangePassword.NewPassword, CurrentUserId);
            });
        }
    }
}
