using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace System
{
    static class LoggerHelper
    {
        private static Dictionary<Type, string> _categories = new Dictionary<Type, string>();

        public static ILoggerFactory LoggerFactory;

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

        public static string LogDebug(this string message, object caller, params object[] args)
        {
            Log(caller, message, LogLevel.Debug, null, args);

            return message;
        }

        public static string LogTrace(this string message, object caller, params object[] args)
        {
            Log(caller, message, LogLevel.Trace, args: args);

            return message;
        }

        public static string LogDebugWithFormat(this string message, object caller, string format)
        {
            Log(caller, format, LogLevel.Debug, null, message);

            return message;
        }

        public static string LogTraceWithFormat(this string message, object caller, string format)
        {
            Log(caller, format, LogLevel.Trace, null, message);

            return message;
        }

        public static string LogError(this string message, object caller, Exception ex = null, params object[] args)
        {
            Log(caller, message, LogLevel.Error, ex, args);

            return message;
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
    }
}
