using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;

public class LocationChanger
{
    public enum LANGUAGE
    {
        CN = 0,
        KR = 1,
        TW = 2,
        EN = 3,
    }

    [MenuItem("Tools/切换语言/天朝CN", false, 11)]
    public static void Check2CN()
    {
        ChangeLanguage(LANGUAGE.CN);
    }

    [MenuItem("Tools/切换语言/韩国KR", false, 12)]
    public static void Check2KR()
    {
        ChangeLanguage(LANGUAGE.KR);
    }

    [MenuItem("Tools/切换语言/宝岛TW", false, 13)]
    public static void Check2TW()
    {
        ChangeLanguage(LANGUAGE.TW);
    }

    [MenuItem("Tools/切换语言/国际EN", false, 14)]
    public static void Check2EN()
    {
        ChangeLanguage(LANGUAGE.EN);
    }

    public static void ChangeLanguage(LANGUAGE lg)
    {
        var filePath = Path.Combine(Application.dataPath, "../Library/Caches/updateres/userlanguage.txt");

        if(File.Exists(filePath)) File.Delete(filePath);

        FileStream fs = new FileStream(filePath, FileMode.Create);

        var strContent = "";
        switch(lg)
        {
            case LANGUAGE.CN:
                strContent = "CN";
                break;
            case LANGUAGE.KR:
                strContent = "KR";
                break;
            case LANGUAGE.TW:
                strContent = "TW";
                break;
            case LANGUAGE.EN:
                strContent = "EN";
                break;
        }

        if(!string.IsNullOrEmpty(strContent))
        {
            byte[] data = System.Text.Encoding.Default.GetBytes(strContent);
            fs.Write(data, 0, data.Length);
        }

        fs.Flush();
        fs.Close();
    }
}
