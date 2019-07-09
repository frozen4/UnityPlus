using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


public class TextLogger
{
    public static TextLogger _Instance = null;
    private static readonly object _lock = new object();
    private static string strPath = "";

    private TextLogger()
    {
    }

    public static string Path
    {
        set { strPath = value; }
        get { return strPath; }
    }

    public static TextLogger Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new TextLogger();
            return _Instance;
        }
    }

    public void WriteLine(string message)
    {
        lock (_lock)
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
    }
}

