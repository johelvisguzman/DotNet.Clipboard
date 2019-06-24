namespace DotNet.Clipboard.Repositories
{
    using DotNetToolkit.Repository;
    using Models;
    using System.Threading.Tasks;

    public interface IClipboardRepository : IRepository<Clip, int>
    {
        Task CompactDatabaseAsync();
        string GetDatabasePath();
    }
}
