using System;
using System.Security;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class CPlatformConfig
{
    private static string _PlatformString = null;
    private static string _LocaleString = null;
    private static string _BaseVersion = null;

    public static string GetPlatForm()
    {
        if (_PlatformString == null)
        {
#if UNITY_IPHONE
				_PlatformString = "iOS";
#elif UNITY_ANDROID
				_PlatformString = "Android";
#else
            _PlatformString = "Windows";
#endif //
        }

        return _PlatformString;
    }

    public static string GetLocale()
    {
        if (_LocaleString == null)
        {
            TextAsset textAsset = (TextAsset)Resources.Load("Locale", typeof(TextAsset));
            if (textAsset == null)
            {
                _LocaleString = "cn";
            }
            else
            {
                try
                {
                    StreamReader sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(textAsset.text)));
                    string str;
                    str = sr.ReadLine();
                    if (str != null)
                    {
                        _LocaleString = str.ToLower();
                    }
                    sr.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        return _LocaleString;
    }

    public static string GetBaseVersion()
    {
        if (_BaseVersion == null)
        {
            TextAsset textAsset = (TextAsset)Resources.Load("BaseVersion", typeof(TextAsset));
            if (textAsset == null)
            {
                _BaseVersion = "1.0.0";
            }
            else
            {
                _BaseVersion = textAsset.text; 
            }
        }

        return _BaseVersion;
    }
}

