using System.Collections.Generic;

namespace PolicyTrackerWebService.Domain
{
    public class PayoutLister
    {
        public List<Payout> List { get; set; } = new List<Payout>();

        public Payout SearchCriteria { get; set; } = new Payout();

        public Pagination Pagination { get; set; } = new Pagination();
    }
}
