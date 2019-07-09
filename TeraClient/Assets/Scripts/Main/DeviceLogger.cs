using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using LuaInterface;

public class DeviceLogger
{
    public static DeviceLogger _Instance = null;
    private static readonly object writeAndroidSDCardLock = new object();
    private string strPath = "";

    private DeviceLogger()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        strPath = Path.Combine(Environment.CurrentDirectory, "DeviceLog.txt");
        if (File.Exists(strPath))
            File.Delete(strPath);
#elif UNITY_ANDROID || UNITY_IOS
        strPath = Path.Combine(Application.persistentDataPath, "DeviceLog.txt");
        if (File.Exists(strPath))
            File.Delete(strPath);
#endif
    }

    public static DeviceLogger Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new DeviceLogger();
            return _Instance;
        }
    }

    public void WriteLog(string message)
    {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_WIN
        //string strPath = Application.persistentDataPath + "/M1TempLog.txt";

        lock (writeAndroidSDCardLock)
        {
            try
            {
                StreamWriter sw;
                if (File.Exists(strPath))
                    sw = File.AppendText(strPath);
                else
                    sw = File.CreateText(strPath);

                if (sw != null)
                {
                    var sb = HobaText.GetStringBuilder();
                    sb.AppendFormat("[{0}]\t{1}", System.DateTime.Now.TimeOfDay, message);
                    sw.WriteLine(sb.ToString());

                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }
        
#endif

    }

    public void WriteLogFormat(string format, params string[] args)
    {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_WIN
        //string strPath = Application.persistentDataPath + "/M1TempLog.txt";

        lock (writeAndroidSDCardLock)
        {
            try
            {
                StreamWriter sw;
                if (File.Exists(strPath))
                    sw = File.AppendText(strPath);
                else
                    sw = File.CreateText(strPath);

                if (sw != null)
                {
                    var sb = HobaText.GetStringBuilder();
                    sb.AppendFormat("[{0}]\t", System.DateTime.Now.TimeOfDay);
                    sb.AppendFormat(format, args);
                    sw.WriteLine(sb.ToString());

                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }

#endif

    }
}

