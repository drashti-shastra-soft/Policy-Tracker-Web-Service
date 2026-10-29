using System;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IRepository
{
    public interface IStatementRepository
    {
        Domain.StatementLineLister GetAll(Domain.StatementLineLister mLister);

        Domain.StatementLine Get(int id);

        List<Domain.StatementLine> GetByBroker(int? brokerId);

        /// <summary>Existing line for broker + policy no + month (duplicate check).</summary>
        Domain.StatementLine Find(int brokerId, string policyNo, DateTime statementMonth, string customerName);

        Domain.StatementLine Upsert(Domain.StatementLine mLine);

        void Link(int lineId, int? policyId, string matchStatus, int modifiedBy);

        void Delete(int id);

        int DeleteMonth(int brokerId, DateTime statementMonth);

        List<DateTime> GetMonths(int? brokerId);
    }
}
