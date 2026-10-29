using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Infrastructure.Utils;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Service
{
    public class CategoryService : ICategoryService
    {
        #region Declaration
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }
        #endregion

        #region Public Methods
        public Domain.CategoryLister GetAll(Domain.CategoryLister mLister)
        {
            return _repo.GetAll(mLister);
        }

        public List<Domain.Category> GetActive()
        {
            return _repo.GetActive();
        }

        public Domain.Category Upsert(Domain.Category mCategory)
        {
            if (mCategory == null) throw new APIRequestFailedException("Object must have a value!");
            if (mCategory.Name.IsNullOrEmpty()) throw new APIRequestFailedException("Category name is required!");
            return _repo.Upsert(mCategory);
        }

        public Domain.Category Get(int id)
        {
            return _repo.Get(id);
        }

        public void Delete(int id, int deletedBy)
        {
            _repo.Delete(id, deletedBy);
        }
        #endregion
    }
}
