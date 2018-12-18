using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


public class TextLogger
{
    private static TextLogger _Instance = null;
    private static readonly object _lock = new object();
    private StringBuilder _stringBuilder = new StringBuilder();
    private static string strPath = "";

    private TextLogger()
    {
    }

    public static string Path
    {
        set { 
            strPath = value;
            _Instance = null;
        }
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
                    _stringBuilder.Length = 0;
                    _stringBuilder.AppendFormat("[{0}]\t{1}", System.DateTime.Now.TimeOfDay, message);
                    sw.WriteLine(_stringBuilder.ToString());

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

