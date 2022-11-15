using System.Data.Entity;

namespace Budget
{
    public class BudgetDb : DbContext
    {
        public BudgetDb() : base(am.DB.DBManager.Instance.ConnectionString){}

        public DbSet<EF.Account> Accounts { get; set; }
        public DbSet<EF.Category> Categories { get; set; }
        public DbSet<EF.Operation> Operations { get; set; }
    }
}
