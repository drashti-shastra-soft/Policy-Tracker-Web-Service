using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Repository.Mappers;
using PolicyTrackerWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Repository
{
    public class PointsGridRepository : IPointsGridRepository
    {
        public Domain.PointsGridLister GetAll(Domain.PointsGridLister mLister)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = from g in context.PointsGrids
                            join b in context.Brokers on g.BrokerId equals b.Id
                            join c in context.Categories on g.CategoryId equals c.Id
                            where g.DeletedDate == null
                            select new { Grid = g, Broker = b.Name, Category = c.Name };

                var criteria = mLister.SearchCriteria;
                if (criteria != null)
                {
                    if (criteria.BrokerId > 0)
                        query = query.Where(x => x.Grid.BrokerId == criteria.BrokerId);

                    if (criteria.CategoryId > 0)
                        query = query.Where(x => x.Grid.CategoryId == criteria.CategoryId);

                    if (criteria.Status.IsNotNullOrEmpty())
                    {
                        var active = criteria.Status == "Active";
                        query = query.Where(x => x.Grid.IsActive == active);
                    }
                }

                var list = query.OrderBy(x => x.Broker).ThenBy(x => x.Category).ThenBy(x => x.Grid.AgeFrom).ToList()
                                .Select(x => x.Grid.ToDomain(x.Broker, x.Category)).ToList();

                mLister.Pagination = mLister.Pagination ?? new Domain.Pagination();
                mLister.List = PagingHelper.Apply(list, mLister.Pagination);
            }
            return mLister;
        }

        public List<Domain.PointsGrid> GetActive()
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.PointsGrids.Where(x => x.DeletedDate == null && x.IsActive).ToList()
                              .Select(x => x.ToDomain()).ToList();
            }
        }

        public Domain.PointsGrid Upsert(Domain.PointsGrid mPointsGrid)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efGrid = context.PointsGrids.FirstOrDefault(x => x.Id == mPointsGrid.Id);
                if (efGrid == null)
                {
                    efGrid = new Persistence.PointsGrid { CreatedBy = mPointsGrid.CreatedBy, CreatedDate = DateTime.Now };
                    context.PointsGrids.Add(efGrid);
                }

                mPointsGrid.ToEntity(efGrid);
                efGrid.LastModifiedBy = mPointsGrid.LastModifiedBy;
                efGrid.LastModifiedDate = DateTime.Now;

                context.SaveChanges();
                mPointsGrid.Id = efGrid.Id;
            }
            return mPointsGrid;
        }

        public Domain.PointsGrid Get(int id)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.PointsGrids.FirstOrDefault(x => x.Id == id && x.DeletedDate == null).ToDomain();
            }
        }

        public void Delete(int id, int deletedBy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efGrid = context.PointsGrids.FirstOrDefault(x => x.Id == id);
                if (efGrid == null) return;

                efGrid.DeletedBy = deletedBy;
                efGrid.DeletedDate = DateTime.Now;
                context.SaveChanges();
            }
        }
    }
}
