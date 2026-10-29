using System.Collections.Generic;

namespace PolicyTrackerWebService.Domain
{
    public class InsuranceCompanyLister
    {
        public List<InsuranceCompany> List { get; set; } = new List<InsuranceCompany>();

        public InsuranceCompany SearchCriteria { get; set; } = new InsuranceCompany();

        public Pagination Pagination { get; set; } = new Pagination();
    }
}
