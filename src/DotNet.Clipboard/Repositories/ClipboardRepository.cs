namespace DotNet.Clipboard.Repositories
{
    using DotNetToolkit.Repository;
    using DotNetToolkit.Repository.Configuration.Options;
    using Models;
    using System.Threading.Tasks;

    public class ClipboardRepository : RepositoryBase<Clip, int>, IClipboardRepository
    {
        public ClipboardRepository(IRepositoryOptions options) : base(options) { }

        public Task CompactDatabaseAsync()
        {
            return ExecuteSqlCommandAsync("vacuum;");
        }

        public string GetDatabasePath()
        {
#if !DEBUG
            return new System.Data.SQLite.SQLiteConnectionStringBuilder(Infrastructure.Utils.GetConnectionString()).DataSource; 
#else
            return string.Empty;
#endif
        }
    }
}
