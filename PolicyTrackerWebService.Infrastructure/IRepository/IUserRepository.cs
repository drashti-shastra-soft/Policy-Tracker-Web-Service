using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IRepository
{
    public interface IUserRepository
    {
        Domain.UserLister GetAll(Domain.UserLister mLister);

        Domain.User Upsert(Domain.User mUser);

        Domain.User Get(int id);

        Domain.User GetBy(string emailAddress);

        bool EmailExists(string emailAddress, int excludeId);

        void ChangePassword(int id, string passwordHash, int modifiedBy);

        void Delete(int id, int deletedBy);

        List<Domain.Role> GetRoles();
    }
}
