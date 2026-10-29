using System;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IRepository
{
    public interface IPolicyRepository
    {
        /// <summary>Filtered list (no paging) – calculations and paging are applied in the service.</summary>
        List<Domain.Policy> Search(Domain.Policy criteria);

        Domain.Policy Get(int id);

        Domain.Policy Upsert(Domain.Policy mPolicy);

        void Delete(int id, int deletedBy);

        /// <summary>Policies of a broker used for statement matching.</summary>
        List<Domain.Policy> GetForMatching(int brokerId);

        /// <summary>Policies expiring between the two dates.</summary>
        List<Domain.Policy> GetExpiring(DateTime from, DateTime to, int? brokerId);
    }
}
