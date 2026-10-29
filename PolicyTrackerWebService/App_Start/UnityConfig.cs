using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Repository;
using PolicyTrackerWebService.Service;
using System.Web.Http;
using Unity;
using Unity.WebApi;

namespace PolicyTrackerWebService
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            //Service
            container.RegisterType<IAuthenticationService, AuthenticationService>();
            container.RegisterType<IBrokerService, BrokerService>();
            container.RegisterType<ICategoryService, CategoryService>();
            container.RegisterType<IExcelReaderService, ExcelReaderService>();
            container.RegisterType<IInsuranceCompanyService, InsuranceCompanyService>();
            container.RegisterType<IJWTokenService, JWTokenService>();
            container.RegisterType<IPayoutService, PayoutService>();
            container.RegisterType<IPointsGridService, PointsGridService>();
            container.RegisterType<IPolicyService, PolicyService>();
            container.RegisterType<IReportService, ReportService>();
            container.RegisterType<IStatementService, StatementService>();
            container.RegisterType<IUserService, UserService>();

            //Repository
            container.RegisterType<IBrokerRepository, BrokerRepository>();
            container.RegisterType<ICategoryRepository, CategoryRepository>();
            container.RegisterType<IInsuranceCompanyRepository, InsuranceCompanyRepository>();
            container.RegisterType<IPayoutRepository, PayoutRepository>();
            container.RegisterType<IPointsGridRepository, PointsGridRepository>();
            container.RegisterType<IPolicyRepository, PolicyRepository>();
            container.RegisterType<IStatementRepository, StatementRepository>();
            container.RegisterType<IUserRepository, UserRepository>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}
