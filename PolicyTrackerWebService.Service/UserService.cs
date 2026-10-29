using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Infrastructure.Utils;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PolicyTrackerWebService.Service
{
    public class UserService : IUserService
    {
        #region Declaration
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }
        #endregion

        #region Public Methods
        public Domain.UserLister GetAll(Domain.UserLister mLister)
        {
            return _repo.GetAll(mLister);
        }

        public Domain.User Upsert(Domain.User mUser)
        {
            if (mUser == null) throw new APIRequestFailedException("Object must have a value!");
            if (mUser.FirstName.IsNullOrEmpty()) throw new APIRequestFailedException("First Name is required!");
            if (mUser.EmailAddress.IsNullOrEmpty() || !Regex.IsMatch(mUser.EmailAddress.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new APIRequestFailedException("A valid Email Address is required!");
            if (mUser.RoleId <= 0) throw new APIRequestFailedException("Role is required!");
            if (_repo.EmailExists(mUser.EmailAddress, mUser.Id)) throw new APIRequestFailedException("This email address is already used by another user!");

            if (mUser.Id <= 0)
            {
                ValidatePassword(mUser.Password);
                mUser.Password = PasswordHasher.Hash(mUser.Password);
            }
            else if (mUser.Password.IsNotNullOrEmpty())
            {
                ValidatePassword(mUser.Password);
                _repo.ChangePassword(mUser.Id, PasswordHasher.Hash(mUser.Password), mUser.LastModifiedBy ?? mUser.CreatedBy);
            }

            return _repo.Upsert(mUser);
        }

        public Domain.User Get(int id)
        {
            return _repo.Get(id);
        }

        public void ChangePassword(int id, string currentPassword, string newPassword, int modifiedBy)
        {
            var mUser = _repo.Get(id);
            if (mUser == null) throw new APIRequestFailedException("User not found!");

            var withHash = _repo.GetBy(mUser.EmailAddress);
            if (!PasswordHasher.Verify(currentPassword, withHash.Password))
                throw new APIRequestFailedException("Current password is incorrect!");

            ValidatePassword(newPassword);
            _repo.ChangePassword(id, PasswordHasher.Hash(newPassword), modifiedBy);
        }

        public void Delete(int id, int deletedBy)
        {
            if (id == deletedBy) throw new APIRequestFailedException("You cannot delete your own account!");
            _repo.Delete(id, deletedBy);
        }

        public List<Domain.Role> GetRoles()
        {
            return _repo.GetRoles();
        }
        #endregion

        private static void ValidatePassword(string password)
        {
            if (password.IsNullOrEmpty() || password.Length < 8)
                throw new APIRequestFailedException("Password must be at least 8 characters!");
        }
    }
}
