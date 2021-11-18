using System.Threading.Tasks;

namespace ConnectionManager.Common
{
    public interface ILogWriter
    {
        Task InitializeAsync(string name);
        Task WriteLineToLogAsync(string text);
        void WriteLineToLog(string text);
    }
}
