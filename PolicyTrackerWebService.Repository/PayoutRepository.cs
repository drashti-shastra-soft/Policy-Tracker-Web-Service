using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Repository.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Repository
{
    public class PayoutRepository : IPayoutRepository
    {
        public List<Domain.Payout> Search(Domain.Payout criteria)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = from p in context.Payouts
                            join b in context.Brokers on p.BrokerId equals b.Id
                            where p.DeletedDate == null
                            select new { Payout = p, Broker = b.Name };

                if (criteria != null && criteria.BrokerId > 0)
                    query = query.Where(x => x.Payout.BrokerId == criteria.BrokerId);

                return query.OrderBy(x => x.Payout.StatementMonth == null)
                            .ThenBy(x => x.Payout.StatementMonth)
                            .ThenBy(x => x.Payout.ReceivedOn)
                            .ToList()
                            .Select(x =>
                            {
                                var m = x.Payout.ToDomain();
                                m.Broker = x.Broker;
                                return m;
                            }).ToList();
            }
        }

        public Domain.Payout Get(int id)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.Payouts.FirstOrDefault(x => x.Id == id && x.DeletedDate == null).ToDomain();
            }
        }

        public Domain.Payout Upsert(Domain.Payout mPayout)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efPayout = context.Payouts.FirstOrDefault(x => x.Id == mPayout.Id);
                if (efPayout == null)
                {
                    efPayout = new Persistence.Payout { CreatedBy = mPayout.CreatedBy, CreatedDate = DateTime.Now };
                    context.Payouts.Add(efPayout);
                }

                mPayout.ToEntity(efPayout);
                efPayout.LastModifiedBy = mPayout.LastModifiedBy;
                efPayout.LastModifiedDate = DateTime.Now;

                context.SaveChanges();
                mPayout.Id = efPayout.Id;
            }
            return mPayout;
        }

        public void Delete(int id, int deletedBy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efPayout = context.Payouts.FirstOrDefault(x => x.Id == id);
                if (efPayout == null) return;

                efPayout.DeletedBy = deletedBy;
                efPayout.DeletedDate = DateTime.Now;
                context.SaveChanges();
            }
        }
    }
}
