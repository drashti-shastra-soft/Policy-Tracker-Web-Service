using System.Collections.Generic;

namespace PolicyTrackerWebService.Domain
{
    public class PointsGridLister
    {
        public List<PointsGrid> List { get; set; } = new List<PointsGrid>();

        public PointsGrid SearchCriteria { get; set; } = new PointsGrid();

        public Pagination Pagination { get; set; } = new Pagination();
    }
}
