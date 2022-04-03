namespace StoryboardSystem; 

public interface ILogger {
    void LogMessage(string message);

    void LogWarning(string warning);
}