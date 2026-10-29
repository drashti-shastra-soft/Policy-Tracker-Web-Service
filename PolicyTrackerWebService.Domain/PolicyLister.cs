using System.Collections.Generic;

namespace PolicyTrackerWebService.Domain
{
    public class PolicyLister
    {
        public List<Policy> List { get; set; } = new List<Policy>();

        public Policy SearchCriteria { get; set; } = new Policy();

        public Pagination Pagination { get; set; } = new Pagination();
    }
}
