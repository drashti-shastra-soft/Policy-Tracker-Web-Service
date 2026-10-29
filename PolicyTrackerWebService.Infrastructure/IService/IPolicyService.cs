using System;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IPolicyService
    {
        Domain.PolicyLister GetAll(Domain.PolicyLister mLister);

        Domain.Policy Get(int id);

        Domain.Policy Upsert(Domain.Policy mPolicy);

        void Delete(int id, int deletedBy);

        Domain.RenewalFollowUp FollowUp(Domain.RenewalFollowUp mFollowUp);

        /// <summary>Policies expiring in the month that are not renewed / lost yet.</summary>
        List<Domain.Policy> GetDueList(DateTime month, int? brokerId, bool includeClosed);

        /// <summary>Applies expected points, commission and statement status to a list.</summary>
        List<Domain.Policy> Calculate(List<Domain.Policy> policies);
    }
}
