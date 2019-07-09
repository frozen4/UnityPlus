using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace GameDataChecker
{
    public static class Utility
    {
        //遍历目录下的文件
        public static void ListFiles(FileSystemInfo info, List<string> fileNameList)
        {
            if (!info.Exists) return;

            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录
            if (dir == null) return;

            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件
                if (file != null)
                    fileNameList.Add(file.FullName);
                else
                    ListFiles(files[i], fileNameList);
            }
        }

        public static void ListFiles(FileSystemInfo info, string ext, List<string> fileNameList)
        {
            if (!info.Exists) return;

            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录
            if (dir == null) return;

            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件
                if (file != null)
                {
                    if (ext == "*" || Path.GetExtension(file.Name) == ext)
                        fileNameList.Add(file.FullName);
                }
                else
                {
                    ListFiles(files[i], ext, fileNameList);
                }
            }
        }

    }
}