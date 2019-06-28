namespace DotNet.Clipboard
{
    using Models;
    using SQLite.CodeFirst;
    using System.Data.Entity;

    /// <summary>
    /// Represents the database context for this application.
    /// </summary>
    /// <seealso cref="System.Data.Entity.DbContext" />
    public class AppDbContext : DbContext
    {
        public virtual DbSet<Clip> Clips { get; set; }

        public AppDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<AppDbContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}
