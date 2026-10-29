namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IPayoutService
    {
        Domain.PayoutLister GetAll(Domain.PayoutLister mLister);

        Domain.Payout Get(int id);

        Domain.Payout Upsert(Domain.Payout mPayout);

        void Delete(int id, int deletedBy);
    }
}
