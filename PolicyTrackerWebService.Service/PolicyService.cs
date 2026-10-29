using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Infrastructure.Utils;
using PolicyTrackerWebService.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Service
{
    public class PolicyService : IPolicyService
    {
        #region Declaration
        private readonly IPolicyRepository _repo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IPointsGridRepository _gridRepo;
        private readonly IStatementRepository _statementRepo;

        public PolicyService(IPolicyRepository repo, ICategoryRepository categoryRepo, IPointsGridRepository gridRepo, IStatementRepository statementRepo)
        {
            _repo = repo;
            _categoryRepo = categoryRepo;
            _gridRepo = gridRepo;
            _statementRepo = statementRepo;
        }
        #endregion

        #region Public Methods
        public Domain.PolicyLister GetAll(Domain.PolicyLister mLister)
        {
            var criteria = mLister.SearchCriteria ?? new Domain.Policy();
            var list = Calculate(_repo.Search(criteria));

            if (criteria.StatementStatus.IsNotNullOrEmpty())
                list = list.Where(x => string.Equals(x.StatementStatus, criteria.StatementStatus, StringComparison.OrdinalIgnoreCase)).ToList();

            mLister.Pagination = mLister.Pagination ?? new Domain.Pagination();
            mLister.List = PagingHelper.Apply(list, mLister.Pagination);
            return mLister;
        }

        public Domain.Policy Get(int id)
        {
            var mPolicy = _repo.Get(id);
            if (mPolicy == null) return null;
            return Calculate(new List<Domain.Policy> { mPolicy }).First();
        }

        public Domain.Policy Upsert(Domain.Policy mPolicy)
        {
            Validate(mPolicy);

            if (mPolicy.Id <= 0)
            {
                mPolicy.IsActive = true;
                if (mPolicy.RenewalStatus.IsNullOrEmpty()) mPolicy.RenewalStatus = Enums.RenewalStatus.Due.ToText();
            }
            else
            {
                // keep links managed by the follow-up flow
                var existing = _repo.Get(mPolicy.Id);
                if (existing == null) throw new APIRequestFailedException("Policy not found!");
                mPolicy.RenewedPolicyId = existing.RenewedPolicyId;
                mPolicy.RenewedFromPolicyId = existing.RenewedFromPolicyId;
                mPolicy.IsActive = true;
            }

            return _repo.Upsert(mPolicy);
        }

        public void Delete(int id, int deletedBy)
        {
            _repo.Delete(id, deletedBy);
        }

        public Domain.RenewalFollowUp FollowUp(Domain.RenewalFollowUp m)
        {
            if (m == null) throw new APIRequestFailedException("Object must have a value!");

            var policy = _repo.Get(m.PolicyId);
            if (policy == null) throw new APIRequestFailedException("Policy not found!");

            if (!EnumHelpers.Texts<Enums.RenewalStatus>().Any(t => m.RenewalStatus.Is(ParseStatus(t))))
                throw new APIRequestFailedException("Please select the renewal outcome!");

            policy.LastModifiedBy = m.LastModifiedBy;
            if (m.Remarks.IsNotNullOrEmpty()) policy.Remarks = m.Remarks.Trim();

            if (m.RenewalStatus.Is(Enums.RenewalStatus.Quote_Sent))
            {
                policy.RenewalStatus = Enums.RenewalStatus.Quote_Sent.ToText();
                policy.QuoteSentOn = m.QuoteSentOn ?? DateTime.Today;
                policy.QuoteDetails = m.QuoteDetails;
                policy.LostReason = null;
            }
            else if (m.RenewalStatus.Is(Enums.RenewalStatus.Follow_Up) || m.RenewalStatus.Is(Enums.RenewalStatus.Due))
            {
                policy.RenewalStatus = m.RenewalStatus.Trim();
                policy.LostReason = null;
            }
            else if (m.RenewalStatus.Is(Enums.RenewalStatus.Lost))
            {
                if (m.LostReason.IsNullOrEmpty())
                    throw new APIRequestFailedException("Please select why the renewal was lost!");

                policy.RenewalStatus = Enums.RenewalStatus.Lost.ToText();
                policy.LostReason = m.LostReason;
            }
            else if (m.RenewalStatus.Is(Enums.RenewalStatus.Renewed))
            {
                ValidatePayment(m);
                m.RenewedPolicyId = SaveRenewedTerm(policy, m);

                policy.RenewalStatus = Enums.RenewalStatus.Renewed.ToText();
                policy.RenewedOn = m.RenewedOn ?? DateTime.Today;
                policy.RenewedPolicyId = m.RenewedPolicyId;
                policy.LostReason = null;
            }

            _repo.Upsert(policy);
            return m;
        }

        public List<Domain.Policy> GetDueList(DateTime month, int? brokerId, bool includeClosed)
        {
            var first = month.FirstOfMonth();
            var last = first.AddMonths(1).AddDays(-1);

            var list = Calculate(_repo.GetExpiring(first, last, brokerId));
            if (!includeClosed)
            {
                list = list.Where(x => !x.RenewalStatus.Is(Enums.RenewalStatus.Renewed) && !x.RenewalStatus.Is(Enums.RenewalStatus.Lost)).ToList();
            }
            return list.OrderBy(x => x.ExpiryDate).ThenBy(x => x.CustomerName).ToList();
        }

        public List<Domain.Policy> Calculate(List<Domain.Policy> policies)
        {
            if (policies == null || policies.Count == 0) return policies ?? new List<Domain.Policy>();

            var calculator = new CommissionCalculator(_categoryRepo.GetActive(), _gridRepo.GetActive(), _statementRepo.GetByBroker(null), DateTime.Today);
            foreach (var p in policies) calculator.Apply(p);
            return policies;
        }
        #endregion

        #region Private Methods
        private void Validate(Domain.Policy mPolicy)
        {
            if (mPolicy == null) throw new APIRequestFailedException("Object must have a value!");
            if (mPolicy.BrokerId <= 0) throw new APIRequestFailedException("Broker is required!");
            if (mPolicy.CustomerName.IsNullOrEmpty()) throw new APIRequestFailedException("Customer name is required!");
            if (mPolicy.StartDate.HasValue && mPolicy.ExpiryDate.HasValue && mPolicy.ExpiryDate <= mPolicy.StartDate)
                throw new APIRequestFailedException("Expiry date must be after the start date!");
            if (mPolicy.NetPremium < 0 || mPolicy.OdPremium < 0 || mPolicy.TpPremium < 0)
                throw new APIRequestFailedException("Premium cannot be negative!");
            if (mPolicy.PointsNet > 100 || mPolicy.PointsOd > 100 || mPolicy.PointsTp > 100)
                throw new APIRequestFailedException("Points are a percentage and cannot be above 100!");

            if (mPolicy.PolicyNo.IsNotNullOrEmpty())
            {
                var key = mPolicy.PolicyNo.NormalizePolicyNo();
                var duplicate = _repo.GetForMatching(mPolicy.BrokerId)
                                     .FirstOrDefault(x => x.Id != mPolicy.Id && x.PolicyNo.NormalizePolicyNo() == key);
                if (duplicate != null)
                    throw new APIRequestFailedException(string.Format("Policy number {0} already exists for {1}!", mPolicy.PolicyNo, duplicate.CustomerName));
            }
        }

        private static void ValidatePayment(Domain.RenewalFollowUp m)
        {
            if (m.PaymentMode.IsNullOrEmpty())
                throw new APIRequestFailedException("Payment mode is required to mark the policy as renewed!");

            if (!m.AmountPaid.HasValue || m.AmountPaid <= 0)
                throw new APIRequestFailedException("Amount paid is required to mark the policy as renewed!");

            if (m.PaymentMode.Is(Enums.PaymentMode.Cheque) && (m.ChequeNo.IsNullOrEmpty() || m.BankName.IsNullOrEmpty()))
                throw new APIRequestFailedException("Cheque number and bank are required for cheque payments!");

            if (m.NewStartDate.HasValue && m.NewExpiryDate.HasValue && m.NewExpiryDate <= m.NewStartDate)
                throw new APIRequestFailedException("New expiry date must be after the new start date!");
        }

        private int SaveRenewedTerm(Domain.Policy old, Domain.RenewalFollowUp m)
        {
            var term = old.RenewedPolicyId.HasValue ? _repo.Get(old.RenewedPolicyId.Value) : null;
            var isNew = term == null;

            if (isNew)
            {
                term = new Domain.Policy
                {
                    BrokerId = old.BrokerId,
                    BusinessType = Enums.BusinessType.Renewal.ToText(),
                    CategoryId = old.CategoryId,
                    ProductPlan = old.ProductPlan,
                    CustomerName = old.CustomerName,
                    MobileNo = old.MobileNo,
                    EmailAddress = old.EmailAddress,
                    EldestAge = old.EldestAge.HasValue ? old.EldestAge + 1 : null,
                    VehicleNo = old.VehicleNo,
                    PreviousPolicyNo = old.PolicyNo,
                    PointsOd = old.PointsOd,
                    PointsTp = old.PointsTp,
                    RenewalStatus = Enums.RenewalStatus.Due.ToText(),
                    RenewedFromPolicyId = old.Id,
                    IsActive = true,
                    CreatedBy = m.LastModifiedBy,
                };
            }

            var start = m.NewStartDate ?? (old.ExpiryDate.HasValue ? old.ExpiryDate.Value.AddDays(1) : (m.RenewedOn ?? DateTime.Today));
            term.StartDate = start;
            term.ExpiryDate = m.NewExpiryDate ?? start.AddYears(1).AddDays(-1);
            term.PolicyNo = m.NewPolicyNo.TrimOrNull() ?? term.PolicyNo;
            term.InsuranceCompanyId = m.InsuranceCompanyId ?? old.InsuranceCompanyId;
            term.SumInsured = m.SumInsured ?? old.SumInsured;
            term.OdPremium = m.OdPremium ?? term.OdPremium;
            term.TpPremium = m.TpPremium ?? term.TpPremium;
            term.GstAmount = m.GstAmount ?? term.GstAmount;
            term.NetPremium = m.NetPremium ?? term.NetPremium ?? (m.AmountPaid - (m.GstAmount ?? 0));
            term.IndicativePremium = old.IndicativePremium;
            term.PaymentMode = m.PaymentMode;
            term.ChequeNo = m.ChequeNo;
            term.BankName = m.BankName;
            term.UtrNo = m.UtrNo;
            term.AmountPaid = m.AmountPaid;
            term.PaymentDate = m.PaymentDate ?? m.RenewedOn ?? DateTime.Today;
            term.SubmittedToBrokerOn = m.SubmittedToBrokerOn;
            term.BrokerStaff = m.BrokerStaff;
            term.ProofRef = m.ProofRef;
            term.PolicyCopyReceived = m.PolicyCopyReceived;
            term.LastModifiedBy = m.LastModifiedBy;

            if (term.PolicyNo.IsNotNullOrEmpty())
            {
                var key = term.PolicyNo.NormalizePolicyNo();
                var duplicate = _repo.GetForMatching(term.BrokerId).FirstOrDefault(x => x.Id != term.Id && x.PolicyNo.NormalizePolicyNo() == key);
                if (duplicate != null)
                    throw new APIRequestFailedException(string.Format("Policy number {0} already exists for {1}!", term.PolicyNo, duplicate.CustomerName));
            }

            return _repo.Upsert(term).Id;
        }

        private static Enums.RenewalStatus ParseStatus(string text)
        {
            return (Enums.RenewalStatus)Enum.Parse(typeof(Enums.RenewalStatus), text.Replace(" ", "_"));
        }
        #endregion
    }
}
