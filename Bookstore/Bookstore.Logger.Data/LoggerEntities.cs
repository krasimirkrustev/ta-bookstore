namespace Bookstore.Logger.Data
{
    using System.Data.Entity;

    public class LoggerEntities : DbContext
    {
        public LoggerEntities()
            : base("LoggerEntities")
        {
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<LoggerEntities, Migrations.Configuration>("LoggerEntities"));
        }

        public DbSet<Model.Log> Logs { get; set; } 
    }
}