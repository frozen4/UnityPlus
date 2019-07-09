using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using Common;
using System.Collections.Generic;

public static class CLogExtension
{
	public static bool IsIncluded(LogLevel logLevel, LogType logType)
	{
		LogLevel inputLevel;
		switch (logType)
		{
			case LogType.Error:
				inputLevel = LogLevel.Error;
				break;
			case LogType.Assert:
				inputLevel = LogLevel.Error;
				break;
			case LogType.Warning:
				inputLevel = LogLevel.Warning;
				break;
			case LogType.Log:
				inputLevel = LogLevel.Log;
				break;
			case LogType.Exception:
				inputLevel = LogLevel.Exception;
				break;
			default:
				inputLevel = LogLevel.All;
				break;
		}

		return (int)inputLevel <= (int)logLevel;
	}

    public static string GenerateDeviceInfo()
    {
        var sb = HobaText.GetStringBuilder();
        sb.AppendLine();
        sb.AppendFormat("locale: {0}\n", CPlatformConfig.GetLocale());
        sb.AppendFormat("platform: {0}\n", CPlatformConfig.GetPlatForm());
        sb.AppendFormat("deviceModel: {0}\n", SystemInfo.deviceModel);
        sb.AppendFormat("deviceName: {0}\n", SystemInfo.deviceName);
        sb.AppendFormat("deviceType: {0}\n", SystemInfo.deviceType);
        sb.AppendFormat("deviceUniqueIdentifier: {0}\n", SystemInfo.deviceUniqueIdentifier);
        sb.AppendFormat("operatingSystem: {0}\n", SystemInfo.operatingSystem);
        sb.AppendFormat("processorCount: {0}\n", SystemInfo.processorCount);
        sb.AppendFormat("processorType: {0}\n", SystemInfo.processorType);
        sb.AppendFormat("processorFrequency: {0}\n", SystemInfo.processorFrequency);
        sb.AppendFormat("graphicsDeviceName: {0}\n", SystemInfo.graphicsDeviceName);
        sb.AppendFormat("systemMemorySize: {0}\n", SystemInfo.systemMemorySize);
        sb.AppendFormat("graphicsMemorySize: {0}\n", SystemInfo.graphicsMemorySize);

        return sb.ToString();
    }

    public static string GenerateApplicationInfo()
    {
        var sb = HobaText.GetStringBuilder();

        sb.AppendLine();
        sb.AppendFormat("productName: {0}\n", Application.productName);
        sb.AppendFormat("bundleIdentifier: {0}\n", Application.identifier);
        sb.AppendFormat("appVersion: {0}\n", Application.version);

        return sb.ToString();
    }
}

public class CLogFile
{
    private static String _LogDir;
    private static string _FilePath;
    private static readonly object _LogFileLock = new object();
    /// 如果记录到 Error 或更高级别的 Log，就创建此文件
    private static String _ErrorFlagFile;

    // log cache for debug
    private static string[] _LogDebugFormats = null;
    private const int MaxCacheCount = 20;

    public struct LogCacheItem
    {
        public int Type;
        public string Content;

        public LogCacheItem(int type, string content)
        {
            Type = type;
            Content = content;
        }
    }
    private static Queue<LogCacheItem> _LogCacheQueue = null;

    public static void Init()
	{
        CUnityHelper.RegisterLogCallback(LogCallback);

        CreateLogFiles();

        var logString = HobaText.Format("[systeminfo] {0}\n[applicationInfo] {1}", CLogExtension.GenerateDeviceInfo(), CLogExtension.GenerateApplicationInfo());
        WriteLogImp(logString);

	    if (EntryPoint.Instance.GameCustomConfigParams.EnableLogCache4Debug && _LogDebugFormats == null)
	    {
            _LogDebugFormats = new []
            {
                "<color='#ff0000'>[Error] {0} </color>\n",  // 0 LogType.Error
	            "<color='#FFB90F'>[Assert] {0} </color>\n", // 1 LogType.Assert
	            "<color='#ffff00'>[Warning] {0} </color>\n",// 2 LogType.Warning
	            "<color='#ffffff'>[Log] {0} </color>\n",    // 3 LogType.Log
	            "<color='#FF8247'>[Exception] {0} </color>\n", // 4 LogType.Exception
	            "<color='#8B8B00'>[Unknown] {0} </color>\n", // 未知类型
	        };
        }
    }

    private static void LogCallback(string condition, string stackTrace, LogType type)
    {
        // 将Log写入文件
        if (EntryPoint.Instance.IsToWriteLogFile && CLogExtension.IsIncluded(EntryPoint.Instance.WriteLogLevel, type))
            WriteUnityLog2File(condition, stackTrace, type);

        // 将Log缓存，以便在Debug面板显示
        if (EntryPoint.Instance.GameCustomConfigParams.EnableLogCache4Debug)
        {
            CacheLog4Debug(type, condition);
            if (type != LogType.Log && type != LogType.Warning && !string.IsNullOrEmpty(stackTrace))
                CacheLog4Debug(type, stackTrace);
        }
    }

    private static void CreateLogFiles()
    {
        string docDir = EntryPoint.Instance.DocPath;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
        _LogDir = Path.Combine(docDir, "Logs");  
#else 
        DateTime now = DateTime.Now;
        string subDir = HobaText.Format("Logs-{0}_{1}_{2}_{3}", now.Month, now.Day, now.Hour, now.Minute);
        _LogDir = Path.Combine(docDir, subDir);
#endif

        _FilePath =  Path.Combine(_LogDir, "gamelog.txt");
		_ErrorFlagFile =  Path.Combine(_LogDir, "has-error");

        try
        {
            if (File.Exists(_ErrorFlagFile))
                File.Delete(_ErrorFlagFile);
            if (File.Exists(_FilePath))
                File.Delete(_FilePath);

            if (!Directory.Exists(_LogDir))
                Directory.CreateDirectory(_LogDir);

            using (File.Create(_FilePath))
            {
                //LuaDLL.HOBA_LogString("Game Log Path: " + _FilePath);
            }
 
        }
        catch(Exception e)
        {
            DeviceLogger.Instance.WriteLog("CLogFile.Init error: " + e.Message);
        }
    }

    private static void WriteUnityLog2File(string condition, string stackTrace, LogType type)
    {
        string log = null;
        switch (type)
		{
			case LogType.Log:
                log = HobaText.Format("{0}", condition);
				break;	
			case LogType.Warning:
                log = HobaText.Format("[warning] {0}", condition);
				break;
			case LogType.Exception:
                log = HobaText.Format("[exception] {0}:\n{1}", condition, stackTrace);
				break;
			case LogType.Error:
                log = HobaText.Format("[error] {0}:\n{1}", condition, stackTrace);
				break;
			default:
                log = HobaText.Format("[unknownerror] {0}", condition);
				break;
		}

        WriteLogImp(log);
    }

    private static void WriteLogImp(string message)
    {
        lock (_LogFileLock)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(_FilePath))
                {
                    sw.WriteLine(HobaText.Format("{0:G}: {1}", System.DateTime.Now, message));
                    sw.Close();
                }
            }
            catch (Exception e)
            {

            }
        }
    }

    private static void CacheLog4Debug(LogType type, string str)
    {
        if (_LogCacheQueue == null)
            _LogCacheQueue = new Queue<LogCacheItem>();

        var typeIdx = (int)type;
        if (typeIdx < (int)LogType.Error || typeIdx > (int)LogType.Exception)
            typeIdx = 5;  // 未知类型

        if (_LogCacheQueue.Count >= MaxCacheCount)
            _LogCacheQueue.Dequeue();

        var log = HobaText.Format(_LogDebugFormats[typeIdx], str.Substring(0, Math.Min(512, str.Length)));
        _LogCacheQueue.Enqueue(new LogCacheItem(typeIdx, log));
    }

    public static void ShowGameLogs4Debug(int logType, GameObject textGo, GameObject contentViewGo)
    {
        if(_LogCacheQueue == null && _LogCacheQueue.Count == 0) return;
        var logs = string.Empty;
        foreach (var v in _LogCacheQueue)
        {
            if ((logType == 1) // 全部显示
                || (logType == 2 && v.Type == (int) LogType.Log) // 显示Log
                || (logType == 3 && v.Type == (int) LogType.Warning) // 显示Warning
                || (logType == 4 && ((v.Type == (int) LogType.Error) || (v.Type == (int) LogType.Exception))) // 显示Error & Exception
            )
            {
                logs = HobaText.Format("{0}{1}", logs, v.Content);
            }
        }

        var textComp = textGo.GetComponent<Text>();
        textComp.text = logs;

        var viewTrans = contentViewGo.GetComponent<RectTransform>();
        var sizeDelta = viewTrans.sizeDelta;
        sizeDelta.y = textComp.preferredHeight + 5;
        viewTrans.sizeDelta = sizeDelta;
    }
}
