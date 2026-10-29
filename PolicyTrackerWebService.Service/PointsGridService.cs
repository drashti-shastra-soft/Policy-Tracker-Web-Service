using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Service
{
    public class PointsGridService : IPointsGridService
    {
        #region Declaration
        private readonly IPointsGridRepository _repo;

        public PointsGridService(IPointsGridRepository repo)
        {
            _repo = repo;
        }
        #endregion

        #region Public Methods
        public Domain.PointsGridLister GetAll(Domain.PointsGridLister mLister)
        {
            return _repo.GetAll(mLister);
        }

        public List<Domain.PointsGrid> GetActive()
        {
            return _repo.GetActive();
        }

        public Domain.PointsGrid Upsert(Domain.PointsGrid mPointsGrid)
        {
            if (mPointsGrid == null) throw new APIRequestFailedException("Object must have a value!");
            if (mPointsGrid.BrokerId <= 0) throw new APIRequestFailedException("Broker is required!");
            if (mPointsGrid.CategoryId <= 0) throw new APIRequestFailedException("Category is required!");
            if (mPointsGrid.AgeFrom < 0 || mPointsGrid.AgeTo < mPointsGrid.AgeFrom) throw new APIRequestFailedException("Age To must be greater than or equal to Age From!");
            if (mPointsGrid.Points < 0 || mPointsGrid.Points > 100) throw new APIRequestFailedException("Points must be between 0 and 100!");
            return _repo.Upsert(mPointsGrid);
        }

        public Domain.PointsGrid Get(int id)
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
