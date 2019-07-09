using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Threading;

public static class CStreamingAssetHelper
{
    public static string MakePath(string subPath)
    {
        return Path.Combine(Application.streamingAssetsPath, subPath);
    }

    public static string ReadFileText(string filePath)
    {
        string fullPath = MakePath(filePath);
        return File.ReadAllText(fullPath);
    }

    public static long CopyAssetFileToPath(String relativeFileName, String sourceDir, String destDir)
    {
        return AndroidUtil.CopyAssetFileToPath(relativeFileName, sourceDir, destDir);
    }                          


}
 