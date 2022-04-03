using BepInEx.Logging;
using StoryboardSystem;

namespace SRXDStoryboard; 

public class Logger : ILogger {
    private ManualLogSource logSource;
    
    public Logger(ManualLogSource logSource) => this.logSource = logSource;

    public void LogMessage(string message) => logSource.LogMessage(message);

    public void LogWarning(string warning) => logSource.LogWarning(warning);
}