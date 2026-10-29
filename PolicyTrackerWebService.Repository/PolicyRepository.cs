using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.Utils;
using PolicyTrackerWebService.Repository.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Repository
{
    public class PolicyRepository : IPolicyRepository
    {
        private class PolicyRow
        {
            public Persistence.Policy Policy { get; set; }
            public string Broker { get; set; }
            public string Category { get; set; }
            public bool? IsMotor { get; set; }
            public string InsuranceCompany { get; set; }
        }

        private static IQueryable<PolicyRow> BaseQuery(Persistence.PolicyTrackerDBEntities context)
        {
            return from p in context.Policies
                   join b in context.Brokers on p.BrokerId equals b.Id
                   join c in context.Categories on p.CategoryId equals (int?)c.Id into cj
                   from c in cj.DefaultIfEmpty()
                   join i in context.InsuranceCompanies on p.InsuranceCompanyId equals (int?)i.Id into ij
                   from i in ij.DefaultIfEmpty()
                   where p.DeletedDate == null
                   select new PolicyRow
                   {
                       Policy = p,
                       Broker = b.Name,
                       Category = c.Name,
                       IsMotor = c.IsMotor,
                       InsuranceCompany = i.Name
                   };
        }

        private static Domain.Policy Map(PolicyRow row)
        {
            var m = row.Policy.ToDomain();
            m.Broker = row.Broker;
            m.Category = row.Category;
            m.IsMotor = row.IsMotor ?? false;
            m.InsuranceCompany = row.InsuranceCompany;
            return m;
        }

        public List<Domain.Policy> Search(Domain.Policy criteria)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = BaseQuery(context);

                if (criteria != null)
                {
                    if (criteria.BrokerId > 0)
                        query = query.Where(x => x.Policy.BrokerId == criteria.BrokerId);

                    if (criteria.CategoryId.HasValue && criteria.CategoryId > 0)
                        query = query.Where(x => x.Policy.CategoryId == criteria.CategoryId);

                    if (criteria.InsuranceCompanyId.HasValue && criteria.InsuranceCompanyId > 0)
                        query = query.Where(x => x.Policy.InsuranceCompanyId == criteria.InsuranceCompanyId);

                    if (criteria.BusinessType.IsNotNullOrEmpty())
                        query = query.Where(x => x.Policy.BusinessType == criteria.BusinessType);

                    if (criteria.RenewalStatus.IsNotNullOrEmpty())
                    {
                        if (criteria.RenewalStatus.Is(Enums.RenewalStatus.Due))
                            query = query.Where(x => x.Policy.RenewalStatus == null || x.Policy.RenewalStatus == "" || x.Policy.RenewalStatus == criteria.RenewalStatus);
                        else
                            query = query.Where(x => x.Policy.RenewalStatus == criteria.RenewalStatus);
                    }

                    if (criteria.SearchText.IsNotNullOrEmpty())
                    {
                        var text = criteria.SearchText.Trim();
                        query = query.Where(x => x.Policy.CustomerName.Contains(text)
                                              || x.Policy.PolicyNo.Contains(text)
                                              || x.Policy.PreviousPolicyNo.Contains(text)
                                              || x.Policy.MobileNo.Contains(text)
                                              || x.Policy.VehicleNo.Contains(text));
                    }

                    if (criteria.CustomerName.IsNotNullOrEmpty())
                        query = query.Where(x => x.Policy.CustomerName.Contains(criteria.CustomerName));

                    if (criteria.PolicyNo.IsNotNullOrEmpty())
                        query = query.Where(x => x.Policy.PolicyNo.Contains(criteria.PolicyNo) || x.Policy.PreviousPolicyNo.Contains(criteria.PolicyNo));

                    if (criteria.ExpiryFrom.HasValue)
                        query = query.Where(x => x.Policy.ExpiryDate >= criteria.ExpiryFrom);

                    if (criteria.ExpiryTo.HasValue)
                        query = query.Where(x => x.Policy.ExpiryDate <= criteria.ExpiryTo);

                    if (criteria.StartFrom.HasValue)
                        query = query.Where(x => x.Policy.StartDate >= criteria.StartFrom);

                    if (criteria.StartTo.HasValue)
                        query = query.Where(x => x.Policy.StartDate <= criteria.StartTo);
                }

                return query.OrderByDescending(x => x.Policy.StartDate ?? x.Policy.ExpiryDate)
                            .ThenBy(x => x.Policy.CustomerName)
                            .ToList()
                            .Select(Map)
                            .ToList();
            }
        }

        public Domain.Policy Get(int id)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var row = BaseQuery(context).FirstOrDefault(x => x.Policy.Id == id);
                return row == null ? null : Map(row);
            }
        }

        public Domain.Policy Upsert(Domain.Policy mPolicy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efPolicy = context.Policies.FirstOrDefault(x => x.Id == mPolicy.Id);
                if (efPolicy == null)
                {
                    efPolicy = new Persistence.Policy { CreatedBy = mPolicy.CreatedBy, CreatedDate = DateTime.Now };
                    context.Policies.Add(efPolicy);
                }

                mPolicy.ToEntity(efPolicy);
                efPolicy.LastModifiedBy = mPolicy.LastModifiedBy;
                efPolicy.LastModifiedDate = DateTime.Now;

                context.SaveChanges();
                mPolicy.Id = efPolicy.Id;
            }
            return mPolicy;
        }

        public void Delete(int id, int deletedBy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efPolicy = context.Policies.FirstOrDefault(x => x.Id == id);
                if (efPolicy == null) return;

                efPolicy.DeletedBy = deletedBy;
                efPolicy.DeletedDate = DateTime.Now;

                // release any statement lines linked to this policy
                foreach (var line in context.StatementLines.Where(x => x.PolicyId == id).ToList())
                {
                    line.PolicyId = null;
                    line.MatchStatus = Enums.MatchStatus.Unmatched.ToText();
                    line.LastModifiedBy = deletedBy;
                    line.LastModifiedDate = DateTime.Now;
                }

                context.SaveChanges();
            }
        }

        public List<Domain.Policy> GetForMatching(int brokerId)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return BaseQuery(context).Where(x => x.Policy.BrokerId == brokerId).ToList().Select(Map).ToList();
            }
        }

        public List<Domain.Policy> GetExpiring(DateTime from, DateTime to, int? brokerId)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = BaseQuery(context).Where(x => x.Policy.ExpiryDate >= from && x.Policy.ExpiryDate <= to);
                if (brokerId.HasValue && brokerId > 0)
                    query = query.Where(x => x.Policy.BrokerId == brokerId);

                return query.OrderBy(x => x.Policy.ExpiryDate).ToList().Select(Map).ToList();
            }
        }
    }
}
