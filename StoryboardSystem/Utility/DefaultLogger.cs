using System;

namespace StoryboardSystem; 

internal class DefaultLogger : ILogger {
    public void LogMessage(string message) => Console.WriteLine(message);

    public void LogWarning(string warning) => Console.WriteLine(warning);

    public void LogError(string error) => Console.WriteLine(error);
}