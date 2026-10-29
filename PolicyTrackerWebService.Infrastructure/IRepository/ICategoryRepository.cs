using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IRepository
{
    public interface ICategoryRepository
    {
        Domain.CategoryLister GetAll(Domain.CategoryLister mLister);

        List<Domain.Category> GetActive();

        Domain.Category Upsert(Domain.Category mCategory);

        Domain.Category Get(int id);

        void Delete(int id, int deletedBy);
    }
}
