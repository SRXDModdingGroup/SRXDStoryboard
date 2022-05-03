using StoryboardSystem;

public class DummyLogger : ILogger {
    public void LogMessage(string message) { }

    public void LogWarning(string warning) { }

    public void LogError(string error) { }
}