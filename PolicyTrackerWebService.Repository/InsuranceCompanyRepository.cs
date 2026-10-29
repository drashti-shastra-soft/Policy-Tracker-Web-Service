using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.Utils;
using PolicyTrackerWebService.Repository.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Repository
{
    public class InsuranceCompanyRepository : IInsuranceCompanyRepository
    {
        public Domain.InsuranceCompanyLister GetAll(Domain.InsuranceCompanyLister mLister)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = context.InsuranceCompanies.Where(x => x.DeletedDate == null);

                var criteria = mLister.SearchCriteria;
                if (criteria != null)
                {
                    if (criteria.Status.IsNotNullOrEmpty())
                    {
                        var active = criteria.Status == "Active";
                        query = query.Where(x => x.IsActive == active);
                    }

                    if (criteria.Name.IsNotNullOrEmpty())
                        query = query.Where(x => x.Name.Contains(criteria.Name) || x.ShortName.Contains(criteria.Name));
                }

                var list = query.OrderBy(x => x.Name).ToList().Select(x => x.ToDomain()).ToList();

                mLister.Pagination = mLister.Pagination ?? new Domain.Pagination();
                mLister.List = PagingHelper.Apply(list, mLister.Pagination);
            }
            return mLister;
        }

        public List<Domain.InsuranceCompany> GetActive()
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.InsuranceCompanies.Where(x => x.DeletedDate == null && x.IsActive)
                              .OrderBy(x => x.Name).ToList()
                              .Select(x => x.ToDomain()).ToList();
            }
        }

        public Domain.InsuranceCompany Upsert(Domain.InsuranceCompany mInsuranceCompany)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efCompany = context.InsuranceCompanies.FirstOrDefault(x => x.Id == mInsuranceCompany.Id);
                if (efCompany == null)
                {
                    efCompany = new Persistence.InsuranceCompany { CreatedBy = mInsuranceCompany.CreatedBy, CreatedDate = DateTime.Now };
                    context.InsuranceCompanies.Add(efCompany);
                }

                mInsuranceCompany.ToEntity(efCompany);
                efCompany.LastModifiedBy = mInsuranceCompany.LastModifiedBy;
                efCompany.LastModifiedDate = DateTime.Now;

                context.SaveChanges();
                mInsuranceCompany.Id = efCompany.Id;
            }
            return mInsuranceCompany;
        }

        public Domain.InsuranceCompany Get(int id)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.InsuranceCompanies.FirstOrDefault(x => x.Id == id && x.DeletedDate == null).ToDomain();
            }
        }

        public void Delete(int id, int deletedBy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efCompany = context.InsuranceCompanies.FirstOrDefault(x => x.Id == id);
                if (efCompany == null) return;

                efCompany.DeletedBy = deletedBy;
                efCompany.DeletedDate = DateTime.Now;
                context.SaveChanges();
            }
        }
    }
}
