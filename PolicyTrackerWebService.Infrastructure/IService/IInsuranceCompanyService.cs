using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IInsuranceCompanyService
    {
        Domain.InsuranceCompanyLister GetAll(Domain.InsuranceCompanyLister mLister);

        List<Domain.InsuranceCompany> GetActive();

        Domain.InsuranceCompany Upsert(Domain.InsuranceCompany mInsuranceCompany);

        Domain.InsuranceCompany Get(int id);

        void Delete(int id, int deletedBy);
    }
}
