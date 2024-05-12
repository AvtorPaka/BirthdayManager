
namespace BirthdayNotificationsBot.Logging;

public class AppLogger : ILogger, IDisposable
{   
    private string DirectoryToLogData {get; init;}

    private static readonly object _lock = new object();

    public AppLogger(string dirToLogData)
    {
        DirectoryToLogData = dirToLogData;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return this;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (_lock)
        {
            string pathToLogData = $"{DirectoryToLogData}_{DateTime.Now.ToString("dd-MM-yyyy")}.txt";
            File.AppendAllLines(pathToLogData, new string[]{$"{state}"});
        }
    }

    public void Dispose() {}
}