using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.Utils;
using PolicyTrackerWebService.Repository.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        public Domain.CategoryLister GetAll(Domain.CategoryLister mLister)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = context.Categories.Where(x => x.DeletedDate == null);

                var criteria = mLister.SearchCriteria;
                if (criteria != null)
                {
                    if (criteria.Status.IsNotNullOrEmpty())
                    {
                        var active = criteria.Status == "Active";
                        query = query.Where(x => x.IsActive == active);
                    }

                    if (criteria.Name.IsNotNullOrEmpty())
                        query = query.Where(x => x.Name.Contains(criteria.Name));
                }

                var list = query.OrderBy(x => x.Name).ToList().Select(x => x.ToDomain()).ToList();

                mLister.Pagination = mLister.Pagination ?? new Domain.Pagination();
                mLister.List = PagingHelper.Apply(list, mLister.Pagination);
            }
            return mLister;
        }

        public List<Domain.Category> GetActive()
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.Categories.Where(x => x.DeletedDate == null && x.IsActive)
                              .OrderBy(x => x.Name).ToList()
                              .Select(x => x.ToDomain()).ToList();
            }
        }

        public Domain.Category Upsert(Domain.Category mCategory)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efCategory = context.Categories.FirstOrDefault(x => x.Id == mCategory.Id);
                if (efCategory == null)
                {
                    efCategory = new Persistence.Category { CreatedBy = mCategory.CreatedBy, CreatedDate = DateTime.Now };
                    context.Categories.Add(efCategory);
                }

                mCategory.ToEntity(efCategory);
                efCategory.LastModifiedBy = mCategory.LastModifiedBy;
                efCategory.LastModifiedDate = DateTime.Now;

                context.SaveChanges();
                mCategory.Id = efCategory.Id;
            }
            return mCategory;
        }

        public Domain.Category Get(int id)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.Categories.FirstOrDefault(x => x.Id == id && x.DeletedDate == null).ToDomain();
            }
        }

        public void Delete(int id, int deletedBy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efCategory = context.Categories.FirstOrDefault(x => x.Id == id);
                if (efCategory == null) return;

                efCategory.DeletedBy = deletedBy;
                efCategory.DeletedDate = DateTime.Now;
                context.SaveChanges();
            }
        }
    }
}
