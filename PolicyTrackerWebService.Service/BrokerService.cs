using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Infrastructure.Utils;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Service
{
    public class BrokerService : IBrokerService
    {
        #region Declaration
        private readonly IBrokerRepository _repo;

        public BrokerService(IBrokerRepository repo)
        {
            _repo = repo;
        }
        #endregion

        #region Public Methods
        public Domain.BrokerLister GetAll(Domain.BrokerLister mLister)
        {
            return _repo.GetAll(mLister);
        }

        public List<Domain.Broker> GetActive()
        {
            return _repo.GetActive();
        }

        public Domain.Broker Upsert(Domain.Broker mBroker)
        {
            if (mBroker == null) throw new APIRequestFailedException("Object must have a value!");
            if (mBroker.Name.IsNullOrEmpty()) throw new APIRequestFailedException("Broker name is required!");
            if (mBroker.TdsRate < 0 || mBroker.TdsRate > 30) throw new APIRequestFailedException("TDS rate must be between 0 and 30%!");
            return _repo.Upsert(mBroker);
        }

        public Domain.Broker Get(int id)
        {
            return _repo.Get(id);
        }

        public void Delete(int id, int deletedBy)
        {
            _repo.Delete(id, deletedBy);
        }
        #endregion
    }
}
