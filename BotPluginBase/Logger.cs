using System;
using System.Collections.Generic;
using System.Text;

namespace BotPluginBase
{
    public enum LogLevel
    {
        Default,
        Success,
        Info,
        Warning,
        Error
    }

    public static class Logger
    {
        public static void Log(string text, LogLevel level = LogLevel.Default)
        {
            setColor(level);
            Console.WriteLine(text);
        }

        private static void setColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogLevel.Default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
        }
    }
}
