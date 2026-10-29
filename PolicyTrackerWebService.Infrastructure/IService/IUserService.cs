using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IUserService
    {
        Domain.UserLister GetAll(Domain.UserLister mLister);

        Domain.User Upsert(Domain.User mUser);

        Domain.User Get(int id);

        void ChangePassword(int id, string currentPassword, string newPassword, int modifiedBy);

        void Delete(int id, int deletedBy);

        List<Domain.Role> GetRoles();
    }
}
