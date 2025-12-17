using Vintagestory.API.Common;

using System;
using System.IO;

namespace IndustrialRevolution;

public class ModLogger
{
    private readonly string logPath;
    private readonly object lockObj = new object();

    public ModLogger(ICoreAPI api, string modName)
    {
        string logDir = Path.Combine(api.DataBasePath, "Logs");
        Directory.CreateDirectory(logDir);
        logPath = Path.Combine(logDir, $"{modName}.log");
    }

    public void Info(string message) => Log("INFO", message);
    public void Warning(string message) => Log("WARN", message);
    public void Error(string message) => Log("ERROR", message);
    public void Debug(string message) => Log("DEBUG", message);

    private void Log(string level, string message)
    {
        lock (lockObj)
        {
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(logPath, entry + Environment.NewLine);
        }
    }
}
