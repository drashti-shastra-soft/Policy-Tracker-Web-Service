using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Infrastructure.Utils
{
    public static class PagingHelper
    {
        /// <summary>Same paging rules as the existing listers: Take &lt;= 0 means "all".</summary>
        public static List<T> Apply<T>(IEnumerable<T> source, Domain.Pagination pagination)
        {
            var list = source as IList<T> ?? source.ToList();

            pagination.TotalRecord = list.Count;

            if (pagination.Take <= 0)
            {
                pagination.Take = pagination.PageSize > 0 ? pagination.PageSize : pagination.TotalRecord;
                if (pagination.Take <= 0) pagination.Take = pagination.TotalRecord;
            }

            if (pagination.Skip < 0 || pagination.Skip >= Math.Max(pagination.TotalRecord, 1))
                pagination.Skip = 0;

            pagination.CurrentPage = pagination.Take > 0 ? (pagination.Skip / pagination.Take) + 1 : 1;
            pagination.TotalPage = pagination.Take > 0 ? Math.Max(1, (int)Math.Ceiling((double)pagination.TotalRecord / pagination.Take)) : 1;

            return list.Skip(pagination.Skip).Take(pagination.Take > 0 ? pagination.Take : list.Count).ToList();
        }
    }
}
