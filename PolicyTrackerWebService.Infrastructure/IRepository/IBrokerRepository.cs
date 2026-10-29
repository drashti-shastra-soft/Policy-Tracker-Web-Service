using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IRepository
{
    public interface IBrokerRepository
    {
        Domain.BrokerLister GetAll(Domain.BrokerLister mLister);

        List<Domain.Broker> GetActive();

        Domain.Broker Upsert(Domain.Broker mBroker);

        Domain.Broker Get(int id);

        void Delete(int id, int deletedBy);
    }
}
