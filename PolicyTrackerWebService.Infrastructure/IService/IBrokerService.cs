using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IBrokerService
    {
        Domain.BrokerLister GetAll(Domain.BrokerLister mLister);

        List<Domain.Broker> GetActive();

        Domain.Broker Upsert(Domain.Broker mBroker);

        Domain.Broker Get(int id);

        void Delete(int id, int deletedBy);
    }
}
