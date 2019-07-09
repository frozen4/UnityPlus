// ReSharper disable once CheckNamespace
using System;
using System.IO;

namespace Common
{
	// 值越大，越详细
	public enum LogLevel
	{
		None = 0,
		Exception = 1,
		Error = 2,
		Warning = 3,
		Log = 4,
		All = 5,
	}

#if !SERVER_USE
    public class HobaDebuger
	{
        public static LogLevel GameLogLevel = LogLevel.None;

        public static void Log(object message)
		{
			if (GameLogLevel >= LogLevel.Log)
			{
				UnityEngine.Debug.Log(message);
            }
        }

		public static void LogFormat(string format, params object[] args)
		{
            Log(HobaText.Format(format, args));
		}

        public static void LogDebug(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        static public void LogWarning(object message)
		{
			if (GameLogLevel >= LogLevel.Warning)
			{
				UnityEngine.Debug.LogWarning(message);
            }
        }

		public static void LogWarningFormat(string format, params object[] args)
		{
            LogWarning(HobaText.Format(format, args));
		}

	    public static void LogError(object message)
		{
			if (GameLogLevel >= LogLevel.Error)
			{
				UnityEngine.Debug.LogError(message);
            }
		}
		
		public static void LogErrorFormat(string format, params object[] args)
		{
            LogError(HobaText.Format(format, args));
		}

        public static void LogLuaError(string luaErrorString)
        {
            if (string.IsNullOrEmpty(luaErrorString))
                LogError("LuaScriptException: Unknown Lua Error!");
            else
                LogError(HobaText.Format("LuaScriptException: {0}", luaErrorString));
        }

        public static void LogLuaError(string luaErrorString, string luaTraceBack)
        {
            if (string.IsNullOrEmpty(luaErrorString))
                LogError(HobaText.Format("LuaScriptException: Unknown Lua Error!\n{0}", luaTraceBack));
            else
                LogError(HobaText.Format("LuaScriptException: {0}\n{1}", luaErrorString, luaTraceBack));
        }

		static public void ClearDeveloperConsole()
		{
			// TODO:
		}


        static public void DrawLine(UnityEngine.Vector3 from, UnityEngine.Vector3 to, UnityEngine.Color color)
        {
            UnityEngine.Debug.DrawLine(from, to, color);
        }

    }
#endif


#if SERVER_USE

    public static class WriteToLog
    {
        public static string ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string LogfileName;
        public static string LogfileFullPath;
        private static object locker = new object();
        public static int LogfileLen = 0;
        public  const int MAX_LOG_FILE_LEN = 1024 ;
        public static int LogfileIndex = 1;
        public static void Log(string lines, string type = "LOG")
        {
            var text = string.Format("({0} {1} {2}) [{3}]: {4}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), ((DateTime.UtcNow - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalMilliseconds) ,type.ToUpper(), lines);
            lock(locker)
            {
                if( LogfileLen> MAX_LOG_FILE_LEN )
                {
                    LogfileIndex += 1;
                    LogfileLen = 0;
                    CreateLogFile();
                }

                File.AppendAllText(LogfileFullPath, text + Environment.NewLine);
                LogfileLen += text.Length;

                ConsoleColor bgColor= Console.BackgroundColor;
                ConsoleColor frColor = Console.ForegroundColor;
                if(type == "ERROR")
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if(type == "WARNING")
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if( type == "INFO")
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.WriteLine(text);
                Console.ResetColor();
            }

        }

        public static void CreateLogFile(string logname)
        {
        }

        public static void CreateLogFile()
        {
            CreateLogFile(LogfileName);
        }
    }
#endif
}