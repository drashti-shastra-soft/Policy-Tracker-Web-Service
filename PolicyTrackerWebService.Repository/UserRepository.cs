using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.Utils;
using PolicyTrackerWebService.Repository.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Repository
{
    public class UserRepository : IUserRepository
    {
        public Domain.UserLister GetAll(Domain.UserLister mLister)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = from u in context.Users
                            join r in context.Roles on u.RoleId equals r.Id
                            where u.DeletedDate == null
                            select new { User = u, RoleName = r.Name };

                var criteria = mLister.SearchCriteria;
                if (criteria != null)
                {
                    if (criteria.Status.IsNotNullOrEmpty())
                    {
                        var active = criteria.Status == "Active";
                        query = query.Where(x => x.User.IsActive == active);
                    }

                    if (criteria.RoleId > 0)
                        query = query.Where(x => x.User.RoleId == criteria.RoleId);

                    if (criteria.FirstName.IsNotNullOrEmpty())
                        query = query.Where(x => (x.User.FirstName + " " + x.User.LastName).Contains(criteria.FirstName));

                    if (criteria.EmailAddress.IsNotNullOrEmpty())
                        query = query.Where(x => x.User.EmailAddress.Contains(criteria.EmailAddress));
                }

                var list = query.OrderBy(x => x.User.FirstName).ToList()
                                .Select(x => x.User.ToDomain(x.RoleName))
                                .ToList();

                mLister.Pagination = mLister.Pagination ?? new Domain.Pagination();
                mLister.List = PagingHelper.Apply(list, mLister.Pagination);
            }
            return mLister;
        }

        public Domain.User Upsert(Domain.User mUser)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efUser = context.Users.FirstOrDefault(x => x.Id == mUser.Id);
                if (efUser == null)
                {
                    efUser = new Persistence.User
                    {
                        CreatedBy = mUser.CreatedBy,
                        CreatedDate = DateTime.Now,
                        PasswordHash = mUser.Password,
                    };
                    context.Users.Add(efUser);
                }

                efUser.FirstName = mUser.FirstName.Trim();
                efUser.LastName = mUser.LastName;
                efUser.EmailAddress = mUser.EmailAddress.Trim().ToLowerInvariant();
                efUser.MobileNo = mUser.MobileNo;
                efUser.RoleId = mUser.RoleId;
                efUser.IsActive = mUser.IsActive;
                efUser.LastModifiedBy = mUser.LastModifiedBy;
                efUser.LastModifiedDate = DateTime.Now;

                context.SaveChanges();
                mUser.Id = efUser.Id;
                mUser.Password = null;
            }
            return mUser;
        }

        public Domain.User Get(int id)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var row = (from u in context.Users
                           join r in context.Roles on u.RoleId equals r.Id
                           where u.Id == id && u.DeletedDate == null
                           select new { User = u, RoleName = r.Name }).FirstOrDefault();

                return row == null ? null : row.User.ToDomain(row.RoleName);
            }
        }

        /// <summary>Returns the user with the stored password hash (for authentication only).</summary>
        public Domain.User GetBy(string emailAddress)
        {
            var email = (emailAddress ?? string.Empty).Trim().ToLowerInvariant();
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var row = (from u in context.Users
                           join r in context.Roles on u.RoleId equals r.Id
                           where u.EmailAddress == email && u.DeletedDate == null
                           select new { User = u, RoleName = r.Name }).FirstOrDefault();

                if (row == null) return null;

                var mUser = row.User.ToDomain(row.RoleName);
                mUser.Password = row.User.PasswordHash;
                return mUser;
            }
        }

        public bool EmailExists(string emailAddress, int excludeId)
        {
            var email = (emailAddress ?? string.Empty).Trim().ToLowerInvariant();
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.Users.Any(x => x.EmailAddress == email && x.Id != excludeId && x.DeletedDate == null);
            }
        }

        public void ChangePassword(int id, string passwordHash, int modifiedBy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efUser = context.Users.FirstOrDefault(x => x.Id == id);
                if (efUser == null) return;

                efUser.PasswordHash = passwordHash;
                efUser.LastModifiedBy = modifiedBy;
                efUser.LastModifiedDate = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void Delete(int id, int deletedBy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efUser = context.Users.FirstOrDefault(x => x.Id == id);
                if (efUser == null) return;

                efUser.DeletedBy = deletedBy;
                efUser.DeletedDate = DateTime.Now;
                context.SaveChanges();
            }
        }

        public List<Domain.Role> GetRoles()
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.Roles.OrderBy(x => x.Id).ToList()
                              .Select(x => new Domain.Role { Id = x.Id, Name = x.Name })
                              .ToList();
            }
        }
    }
}
