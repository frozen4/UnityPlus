using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class U3DFuncWrap
{
    static U3DFuncWrap m_Instance = null;
    static Dictionary<string, int> m_statisticsDic = new Dictionary<string, int>();
    private StreamWriter logWriter = null;

    public static U3DFuncWrap Instance()
    {
        if(m_Instance == null)
            m_Instance = new U3DFuncWrap();
        return m_Instance;
    }

    public void AddStatisticsCount(string key)
    {
        int value = 1;
        if (!m_statisticsDic.ContainsKey(key))
        {
            m_statisticsDic.Add(key, value);
        }
        else
        {
            value = GetStatisticsCount(key);
            value++;
            m_statisticsDic[key] = value;
        }
    }

    private int GetStatisticsCount(string key)
    {
        int value = 0;
        if (m_statisticsDic.ContainsKey(key))
        {
            value = m_statisticsDic[key];
        }

        return value;
    }

    private bool WriteDownStatisticsInfoFile()
    {
        bool bRet = true;
        CreateFile("StatisticsInfo.txt");

        List<string> Keys = new List<string>(m_statisticsDic.Keys);
        for(int i = 0; i < Keys.Count; ++i)
        {
            LogWriteLine(Keys[i] + " Call Times = " + m_statisticsDic[Keys[i]].ToString());
        }
        DestroyFile();
        return bRet;
    }

    private bool CreateFile(string filename)
    {
        string strLogPath = filename;
        if (FileOperate.IsFileExist(strLogPath))
        {
            FileOperate.DeleteFile(strLogPath);
        }
        try
        {
            FileOperate.MakeDir(strLogPath);

            logWriter = File.CreateText(strLogPath);
            LogWriteLine("StatisticsInfo Created!");
        }
        catch (Exception)
        {
            logWriter = null;
        }
        return logWriter != null;
    }

    private void DestroyFile()
    {
        LogWriteLine("UpdateLog Destroyed!");
        if (logWriter != null)
        {
            logWriter.Close();
            logWriter.Dispose();
        }
    }

    private void LogWriteLine(string logMsg)
    {
        if (logWriter != null)
        {
            logWriter.WriteLine(logMsg);
            logWriter.Flush();
        }
    }

    public void OnDebugCmd(string cmd)
    {
        WriteDownStatisticsInfoFile();
    }

    private void Dispose()
    {
        DestroyFile();
        m_statisticsDic.Clear();
        m_statisticsDic = null;
    }
}
