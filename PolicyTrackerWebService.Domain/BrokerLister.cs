using System.Collections.Generic;

namespace PolicyTrackerWebService.Domain
{
    public class BrokerLister
    {
        public List<Broker> List { get; set; } = new List<Broker>();

        public Broker SearchCriteria { get; set; } = new Broker();

        public Pagination Pagination { get; set; } = new Pagination();
    }
}
