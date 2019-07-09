using System.IO;

/// <summary>
/// 文件夹操作类
/// </summary>
public class FolderUtil
{

    /// <summary>
    /// 替换整个文件夹
    /// </summary>
    /// <param name="sourceFolderName">源文件夹目录</param>
    /// <param name="destFolderName">目标文件夹目录</param>
    public static void CopyAndReplace(string sourceFolderName, string destFolderName)
    {
        //Delete(destFolderName);
        Copy(sourceFolderName, destFolderName);
    }

    /// <summary>
    /// 替换文件夹底下的文件
    /// </summary>
    /// <param name="sourceFolderName">源文件夹目录</param>
    /// <param name="destFolderName">目标文件夹目录</param>
    public static void CopyAndReplaceSub(string sourceFolderName, string destFolderName)
    {
        //DeleteSub(destFolderName);
        CopySub(sourceFolderName, destFolderName);
    }

    private static void Copy(string sourceFolderName, string destFolderName)
    {
        //UnityEngine.Debug.LogFormat("Copy Folder:{0} to {1}", sourceFolderName, destFolderName);
        if (Directory.Exists(destFolderName))
            Delete(destFolderName);
        Directory.CreateDirectory(destFolderName);

        foreach (var file in Directory.GetFiles(sourceFolderName))
        {
            string filePath = Path.Combine(destFolderName, Path.GetFileName(file));
            //UnityEngine.Debug.LogFormat("Copy File:{0}", filePath);
            File.Copy(file, filePath);
        }

        foreach (var dir in Directory.GetDirectories(sourceFolderName))
            Copy(dir, Path.Combine(destFolderName, Path.GetFileName(dir)));
    }
    private static void CopySub(string sourceFolderName, string destFolderName)
    {
        foreach (var dir in Directory.GetDirectories(sourceFolderName))
            Copy(dir, Path.Combine(destFolderName, Path.GetFileName(dir)));
    }

    public static void Delete(string folderName)
    {
        if (!Directory.Exists(folderName)) return;

        foreach (var file in Directory.GetFiles(folderName))
        {
            string filePath = Path.Combine(folderName, Path.GetFileName(file));
            //UnityEngine.Debug.LogFormat("Delete File:{0}", filePath);
            File.Delete(filePath);
        }

        foreach (var dir in Directory.GetDirectories(folderName))
            Delete(Path.Combine(folderName, Path.GetFileName(dir)));

        //UnityEngine.Debug.LogFormat("Delete Folder:{0}", folderName);
        Directory.Delete(folderName);
    }

    public static void DeleteSub(string folderName)
    {
        foreach (var dir in Directory.GetDirectories(folderName))
            Delete(Path.Combine(folderName, Path.GetFileName(dir)));
    }
}
