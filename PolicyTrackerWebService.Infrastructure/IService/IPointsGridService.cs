using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IPointsGridService
    {
        Domain.PointsGridLister GetAll(Domain.PointsGridLister mLister);

        List<Domain.PointsGrid> GetActive();

        Domain.PointsGrid Upsert(Domain.PointsGrid mPointsGrid);

        Domain.PointsGrid Get(int id);

        void Delete(int id, int deletedBy);
    }
}
