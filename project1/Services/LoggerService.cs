using System.Globalization;
using FoodExpress.Data;

namespace FoodExpress.Services
{
    /// <summary>Appends timestamped application events without propagating logging failures.</summary>
    public sealed class LoggerService
    {
        private readonly FileManager _files;

        public LoggerService(FileManager files) => _files = files;

        public void Log(string level, string eventName, string details)
        {
            try
            {
                string safeDetails = details.Replace('\r', ' ').Replace('\n', ' ');
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                _files.AppendLine(FileManager.LogsFile, $"{timestamp} | {level} | {eventName} | {safeDetails}");
            }
            catch
            {
            }
        }
    }
}
