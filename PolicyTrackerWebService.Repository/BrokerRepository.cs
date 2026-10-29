using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.Utils;
using PolicyTrackerWebService.Repository.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Repository
{
    public class BrokerRepository : IBrokerRepository
    {
        public Domain.BrokerLister GetAll(Domain.BrokerLister mLister)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = context.Brokers.Where(x => x.DeletedDate == null);

                var criteria = mLister.SearchCriteria;
                if (criteria != null)
                {
                    if (criteria.Status.IsNotNullOrEmpty())
                    {
                        var active = criteria.Status == "Active";
                        query = query.Where(x => x.IsActive == active);
                    }

                    if (criteria.Name.IsNotNullOrEmpty())
                        query = query.Where(x => x.Name.Contains(criteria.Name) || x.PosCode.Contains(criteria.Name));
                }

                var list = query.OrderBy(x => x.Name).ToList().Select(x => x.ToDomain()).ToList();

                mLister.Pagination = mLister.Pagination ?? new Domain.Pagination();
                mLister.List = PagingHelper.Apply(list, mLister.Pagination);
            }
            return mLister;
        }

        public List<Domain.Broker> GetActive()
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.Brokers.Where(x => x.DeletedDate == null && x.IsActive)
                              .OrderBy(x => x.Name).ToList()
                              .Select(x => x.ToDomain()).ToList();
            }
        }

        public Domain.Broker Upsert(Domain.Broker mBroker)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efBroker = context.Brokers.FirstOrDefault(x => x.Id == mBroker.Id);
                if (efBroker == null)
                {
                    efBroker = new Persistence.Broker { CreatedBy = mBroker.CreatedBy, CreatedDate = DateTime.Now };
                    context.Brokers.Add(efBroker);
                }

                mBroker.ToEntity(efBroker);
                efBroker.LastModifiedBy = mBroker.LastModifiedBy;
                efBroker.LastModifiedDate = DateTime.Now;

                context.SaveChanges();
                mBroker.Id = efBroker.Id;
            }
            return mBroker;
        }

        public Domain.Broker Get(int id)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.Brokers.FirstOrDefault(x => x.Id == id && x.DeletedDate == null).ToDomain();
            }
        }

        public void Delete(int id, int deletedBy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efBroker = context.Brokers.FirstOrDefault(x => x.Id == id);
                if (efBroker == null) return;

                efBroker.DeletedBy = deletedBy;
                efBroker.DeletedDate = DateTime.Now;
                context.SaveChanges();
            }
        }
    }
}
