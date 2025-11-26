using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ElProfesorKudo.Firebase.Common
{
    public enum LogLevel { Debug, Info, Warning, Error }

    public static class CustomLogger
    {
        private static bool _enableLogging = true;

        private static LogLevel _minimumLogLevel = LogLevel.Debug;

        private static HashSet<string> _enabledClasses = new HashSet<string>();

        public static void EnableFor(params string[] classNames)
        {
            foreach (string c in classNames)
            {
                _enabledClasses.Add(c);
            }
        }

        public static void DisableFor(params string[] classNames)
        {
            foreach (string c in classNames)
            {
                _enabledClasses.Remove(c);
            }
        }

        public static void LogDebug(string message) { Log(message, LogLevel.Debug); }
        public static void LogInfo(string message) { Log(message, LogLevel.Info); }
        public static void LogWarning(string message) { Log(message, LogLevel.Warning); }
        public static void LogError(string message) { Log(message, LogLevel.Error); }

        private static void Log(string message, LogLevel level)
        {
            if (!_enableLogging || level < _minimumLogLevel) return;

            string className = GetCallingClassName();
            if (_enabledClasses.Count > 0 && !_enabledClasses.Contains(className)) return;

            string finalMessage = $"[{className}] {message}";

            switch (level)
            {
                case LogLevel.Debug: UnityEngine.Debug.Log(finalMessage); break;
                case LogLevel.Info: UnityEngine.Debug.Log(finalMessage); break;
                case LogLevel.Warning: UnityEngine.Debug.LogWarning(finalMessage); break;
                case LogLevel.Error: UnityEngine.Debug.LogError(finalMessage); break;
            }
        }

        private static string GetCallingClassName()
        {
            StackTrace stackTrace = new StackTrace();
            for (int i = 2; i < stackTrace.FrameCount; i++) // skip Logger internal frames
            {
                var method = stackTrace.GetFrame(i).GetMethod();
                var declaringType = method.DeclaringType;
                if (declaringType != null && declaringType != typeof(CustomLogger))
                {
                    return declaringType.Name;
                }
            }
            return "Unknown";
        }
    }
}
