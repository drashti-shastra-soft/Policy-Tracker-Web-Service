using System.Data.Entity;

namespace PolicyTrackerWebService.Persistence
{
    /// <summary>
    /// Maps to the existing database created by Database\PolicyTrackerDB.sql.
    /// The schema is owned by the SQL script, so EF database initialisation is switched off.
    /// </summary>
    public class PolicyTrackerDBEntities : DbContext
    {
        static PolicyTrackerDBEntities()
        {
            Database.SetInitializer<PolicyTrackerDBEntities>(null);
        }

        public PolicyTrackerDBEntities()
            : base("name=PolicyTrackerDBEntities")
        {
        }

        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Broker> Brokers { get; set; }
        public virtual DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<PointsGrid> PointsGrids { get; set; }
        public virtual DbSet<Policy> Policies { get; set; }
        public virtual DbSet<StatementLine> StatementLines { get; set; }
        public virtual DbSet<Payout> Payouts { get; set; }
    }
}
