using System;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IStatementService
    {
        Domain.StatementImportResult Import(Domain.StatementImport mImport);

        Domain.StatementLineLister GetAll(Domain.StatementLineLister mLister);

        Domain.StatementLine Get(int id);

        Domain.StatementLine Link(Domain.StatementLink mLink);

        /// <summary>Creates a policy from an unmatched statement line and links it.</summary>
        Domain.Policy CreatePolicyFromLine(int lineId, int createdBy);

        /// <summary>Re-runs auto matching for unmatched lines.</summary>
        int Rematch(int brokerId, int modifiedBy);

        void Delete(int id);

        int DeleteMonth(int brokerId, DateTime statementMonth);

        List<DateTime> GetMonths(int? brokerId);
    }
}
