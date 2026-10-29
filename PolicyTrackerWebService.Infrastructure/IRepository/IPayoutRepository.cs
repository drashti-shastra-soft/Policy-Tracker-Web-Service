using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IRepository
{
    public interface IPayoutRepository
    {
        List<Domain.Payout> Search(Domain.Payout criteria);

        Domain.Payout Get(int id);

        Domain.Payout Upsert(Domain.Payout mPayout);

        void Delete(int id, int deletedBy);
    }
}
