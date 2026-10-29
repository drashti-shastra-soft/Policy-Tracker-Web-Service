namespace PolicyTrackerWebService.Domain
{
    public class Pagination
    {
        public int PageSize { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }
        public int TotalRecord { get; set; }
    }
}
