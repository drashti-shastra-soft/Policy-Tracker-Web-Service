using System.Collections.Generic;

namespace PolicyTrackerWebService.Domain
{
    public class StatementLineLister
    {
        public List<StatementLine> List { get; set; } = new List<StatementLine>();

        public StatementLine SearchCriteria { get; set; } = new StatementLine();

        public Pagination Pagination { get; set; } = new Pagination();

        public decimal TotalValue { get; set; }
    }
}
