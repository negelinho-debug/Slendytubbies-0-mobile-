using System;
using System.IO;
using UnityEngine;

public static class RecordsRuntimeLog
{
    private static readonly object Gate = new object();
    private static string logPath;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void StartLogging()
    {
        try
        {
            logPath = Path.Combine(Application.persistentDataPath,
                "RecordsDebug.txt");
            File.AppendAllText(logPath,
                "\n===== Records launch " + DateTime.UtcNow.ToString("u") + " =====\n");
            Application.logMessageReceivedThreaded += OnLog;
        }
        catch { /* Logging must not stop the game. */ }
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        if (string.IsNullOrEmpty(logPath)) return;
        try
        {
            lock (Gate)
            {
                if (File.Exists(logPath) &&
                    new FileInfo(logPath).Length > 2 * 1024 * 1024)
                    File.WriteAllText(logPath, "[Previous log truncated]\n");
                File.AppendAllText(logPath,
                    "[" + DateTime.UtcNow.ToString("HH:mm:ss") + "] " +
                    type + ": " + message + "\n" +
                    (type == LogType.Error || type == LogType.Exception ||
                     type == LogType.Assert ? stack + "\n" : ""));
            }
        }
        catch { /* Do not recursively log an I/O error. */ }
    }
}
