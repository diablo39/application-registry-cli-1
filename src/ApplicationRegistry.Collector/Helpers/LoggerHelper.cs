using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace System
{
    static class LoggerHelper
    {
        private static Dictionary<Type, string> _categories = new Dictionary<Type, string>();

        public static ILoggerFactory LoggerFactory = new LoggerFactory();

        public static void SetCategoryNameForCaller(object caller, string categoryName)
        {
            var callerType = caller.GetType();

            if (_categories.ContainsKey(callerType))
            {
                _categories[callerType] = categoryName;
            }
            else
            {
                
                _categories.Add(callerType, categoryName);
            }
        }

        public static void LogWarning(this string message, object caller, params object[] args)
        {
            Log(caller, message, LogLevel.Warning, null, args);
        }

        public static void LogInfo(this string message, object caller, params object[] args)
        {
            Log(caller, message, LogLevel.Information, null, args);
        }

        public static void LogDebug(this string message, object caller, params object[] args)
        {
            Log(caller, message, LogLevel.Debug, null, args);
        }

        public static void LogTrace(this string message, object caller, params object[] args)
        {
            Log(caller, message, LogLevel.Trace, args: args);
        }

        public static void LogDebugWithFormat(this string message, object caller, string format)
        {
            Log(caller, format, LogLevel.Debug, null, message);
        }

        public static void LogTraceWithFormat(this string message, object caller, string format)
        {
            Log(caller, format, LogLevel.Trace, null, message);
        }

        public static void LogErrorWithFormat(this string message, object caller, string format)
        {
            Log(caller, format, LogLevel.Error, null, message);
        }

        public static void LogError(this string message, object caller, Exception ex = null, params object[] args)
        {
            Log(caller, message, LogLevel.Error, ex, args);
        }

        public static void LogError(this string message, object caller, params object[] args)
        {
            Log(caller, message, LogLevel.Error,args);
        }

        public static void LogCritical(this string message, object caller, Exception ex = null, params object[] args)
        {
            Log(caller, message, LogLevel.Critical, ex, args);
        }

        private static void Log(object caller, string message, LogLevel logLevel, Exception ex = null, params object[] args)
        {
            var categoryDefined = _categories.TryGetValue(caller.GetType(), out string categoryName);
            if (!categoryDefined)
            {
                categoryName = caller.GetType().Name;
            }

            var logger = LoggerFactory.CreateLogger(categoryName);

            logger.Log(logLevel, ex, message, args);
        }

        private static void Log(object caller, string message, LogLevel logLevel, params object[] args)
        {
            var categoryDefined = _categories.TryGetValue(caller.GetType(), out string categoryName);
            if (!categoryDefined)
            {
                categoryName = caller.GetType().Name;
            }

            var logger = LoggerFactory.CreateLogger(categoryName);

            logger.Log(logLevel, message, args);
        }
    }
}
