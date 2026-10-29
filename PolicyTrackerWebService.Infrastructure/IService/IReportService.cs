using System;

namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IReportService
    {
        Domain.DashboardSummary GetDashboard(DateTime month, int? brokerId);

        Domain.ReconciliationReport GetReconciliation(int? brokerId);
    }
}
