namespace BirthdayNotificationsBot.Logging;

public class AppLoggerProvider : ILoggerProvider
{   
    private string DirectoryToLogData {get; init;}

    public AppLoggerProvider(string dirToLogData)
    {
        DirectoryToLogData = dirToLogData;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new AppLogger(DirectoryToLogData);
    }

    public void Dispose() {}
}
