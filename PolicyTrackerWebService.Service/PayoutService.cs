using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Infrastructure.Utils;
using System;
using System.Linq;

namespace PolicyTrackerWebService.Service
{
    public class PayoutService : IPayoutService
    {
        #region Declaration
        private readonly IPayoutRepository _repo;
        private readonly IStatementRepository _statementRepo;
        private readonly IBrokerRepository _brokerRepo;

        public PayoutService(IPayoutRepository repo, IStatementRepository statementRepo, IBrokerRepository brokerRepo)
        {
            _repo = repo;
            _statementRepo = statementRepo;
            _brokerRepo = brokerRepo;
        }
        #endregion

        #region Public Methods
        public Domain.PayoutLister GetAll(Domain.PayoutLister mLister)
        {
            var criteria = mLister.SearchCriteria ?? new Domain.Payout();
            var payouts = _repo.Search(criteria);
            var lines = _statementRepo.GetByBroker(criteria.BrokerId > 0 ? criteria.BrokerId : (int?)null);
            var brokers = _brokerRepo.GetActive().ToDictionary(b => b.Id, b => b.TdsRate);

            foreach (var p in payouts)
            {
                if (!p.StatementMonth.HasValue) continue;

                var month = p.StatementMonth.Value.FirstOfMonth();
                var commission = lines.Where(l => l.BrokerId == p.BrokerId && l.StatementMonth == month).Sum(l => l.Value);
                var receivedForMonth = payouts.Where(x => x.BrokerId == p.BrokerId && x.StatementMonth.HasValue && x.StatementMonth.Value.FirstOfMonth() == month).Sum(x => x.AmountReceived);
                var rate = p.TdsRate ?? (brokers.ContainsKey(p.BrokerId) ? brokers[p.BrokerId] : 0m);

                p.CommissionPerStatement = commission;
                p.Tds = (commission * rate / 100m).RoundMoney();
                p.ShortBy = commission - receivedForMonth;
                p.Unexplained = p.ShortBy - p.Tds;
                p.PercentReceived = commission > 0 ? Math.Round(receivedForMonth / commission * 100m, 1) : (decimal?)null;
            }

            mLister.Pagination = mLister.Pagination ?? new Domain.Pagination();
            mLister.List = PagingHelper.Apply(payouts, mLister.Pagination);
            return mLister;
        }

        public Domain.Payout Get(int id)
        {
            return _repo.Get(id);
        }

        public Domain.Payout Upsert(Domain.Payout mPayout)
        {
            if (mPayout == null) throw new APIRequestFailedException("Object must have a value!");
            if (mPayout.BrokerId <= 0) throw new APIRequestFailedException("Broker is required!");
            if (mPayout.AmountReceived <= 0) throw new APIRequestFailedException("Amount received must be more than zero!");
            if (mPayout.TdsRate < 0 || mPayout.TdsRate > 30) throw new APIRequestFailedException("TDS rate must be between 0 and 30%!");
            if (!mPayout.StatementMonth.HasValue && mPayout.Remarks.IsNullOrEmpty() && mPayout.PaidByFirm.IsNullOrEmpty())
                throw new APIRequestFailedException("Select the statement month, or add who paid / remarks for an unallocated payment!");

            return _repo.Upsert(mPayout);
        }

        public void Delete(int id, int deletedBy)
        {
            _repo.Delete(id, deletedBy);
        }
        #endregion
    }
}
