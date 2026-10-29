using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Infrastructure.Utils;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Service
{
    public class InsuranceCompanyService : IInsuranceCompanyService
    {
        #region Declaration
        private readonly IInsuranceCompanyRepository _repo;

        public InsuranceCompanyService(IInsuranceCompanyRepository repo)
        {
            _repo = repo;
        }
        #endregion

        #region Public Methods
        public Domain.InsuranceCompanyLister GetAll(Domain.InsuranceCompanyLister mLister)
        {
            return _repo.GetAll(mLister);
        }

        public List<Domain.InsuranceCompany> GetActive()
        {
            return _repo.GetActive();
        }

        public Domain.InsuranceCompany Upsert(Domain.InsuranceCompany mInsuranceCompany)
        {
            if (mInsuranceCompany == null) throw new APIRequestFailedException("Object must have a value!");
            if (mInsuranceCompany.Name.IsNullOrEmpty()) throw new APIRequestFailedException("Company name is required!");
            return _repo.Upsert(mInsuranceCompany);
        }

        public Domain.InsuranceCompany Get(int id)
        {
            return _repo.Get(id);
        }

        public void Delete(int id, int deletedBy)
        {
            _repo.Delete(id, deletedBy);
        }
        #endregion
    }
}
