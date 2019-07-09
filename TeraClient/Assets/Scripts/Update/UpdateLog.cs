using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class UpdateLog
{    
    private StreamWriter logWriter = null;

    public bool IsValid { get { return logWriter != null; } }

    public bool CreateLog(string filename)
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
            LogWriteLine(string.Format("UpdateLog Created! {0:G}", System.DateTime.Now));
        }
        catch (Exception)
        {
            logWriter = null;
        }
        return logWriter != null;
    }

    public void DestroyLog()
    {
        LogWriteLine("UpdateLog Destroyed!");
        if (logWriter != null)
        {
            logWriter.Close();
            logWriter.Dispose();
            logWriter = null;
        }
    }

    public void LogWriteLine(string logMsg)
    {
        if (logWriter != null)
        {
            var now = System.DateTime.Now;

            string logContent = HobaText.Format("{0}/{1} {2}:{3}:{4}-{5}: {6}", now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, logMsg);
            logWriter.WriteLine(logContent);
            logWriter.Flush();
        }
    }
}

