using System.Collections.Generic;
using Common;
using System.IO;
using System;

// ReSharper disable once CheckNamespace
namespace Template
{
    public class Path
    {
        public static string BasePath;
        public static string BinPath = "Data/";
        public static string LocalePath = "Data/";

        public static bool IsLocaleDataPath(string filePath)
        {
            return filePath.Contains("_LocalString.data");
        }

        public static string GetFullBinPath()
        {
            return System.IO.Path.Combine(BasePath, BinPath);
        }

        public static string GetFullLocalePath()
        {
            return System.IO.Path.Combine(BasePath, LocalePath);
        }
    }

    public interface ITemplateDataManager
    {
        Dictionary<int, byte[]> GetTemplateDataMap();
        byte[] GetTemplateData(int tid);

        void Clear();
    }

    public static class TemplateDataManagerCollection
    {
        public static Dictionary<string, ITemplateDataManager> TemplateDataManagerMap = new Dictionary<string, ITemplateDataManager>();
        public static Dictionary<string, string[]> SpecialDataPathsMap = new Dictionary<string,string[]>();
        public static List<string> PrelaodDataPaths = new List<string>();

        static TemplateDataManagerCollection() { }

        public static int GetTotalMemory()
        {
            int total = 0;
            foreach (var kv in TemplateDataManagerMap)
            {
                total += ((BaseDataManager)kv.Value).GetTotalDataMemory();
            }
            return total;
        }

        public static ITemplateDataManager GetTemplateDataManager(string name)
        {
            ITemplateDataManager manager;
            if (!TemplateDataManagerMap.TryGetValue(name, out manager))
            {
                manager = new BaseDataManager();
                string[] pathArray;
                if (SpecialDataPathsMap.TryGetValue(name, out pathArray) && pathArray != null)
                {
                    for (int i = 0; i < pathArray.Length; ++i)
                    {
                        ((BaseDataManager)manager).LoadFromBin(pathArray[i]);
                    }
                }
                else
                {
                    var path = string.Format("{0}.data", name);
                    ((BaseDataManager)manager).LoadFromBin(path);
                }
                TemplateDataManagerMap.Add(name, manager);
            }

            return manager;
        }

        public static void AddSpecialTemplateDataPath(string name, string[] filePathNameArray)
        {
            if (!SpecialDataPathsMap.ContainsKey(name))
                SpecialDataPathsMap.Add(name, filePathNameArray);
        }

        public static void AddPreloadTemplateDataPath(string name)
        {
            if (!PrelaodDataPaths.Contains(name))
                PrelaodDataPaths.Add(name);
        }
    }

    public class BaseDataManager : ITemplateDataManager
    {
        public void Clear()
        {
            TemplateDataMap.Clear();
        }

        public int GetTotalDataMemory()
        {
            int total = 0;
            foreach(var item in TemplateDataMap)
            {
                total += item.Value.Length;
            }
            return total;
        }

        protected readonly Dictionary<int, byte[]> TemplateDataMap = new Dictionary<int, byte[]>();

        protected float HeaderVersion = 0;
        protected int HeaderData = 0;

        public Dictionary<int, byte[]> GetTemplateDataMap()
        {
            return TemplateDataMap;
        }

        public float GetHeaderVersion()
        {
            return HeaderVersion;
        }

        private void ReadHeader(BinaryReader reader, out float version, out int data)
        {
            version = reader.ReadSingle();
            data = reader.ReadInt32();
        }

        public byte[] GetTemplateData(int tid)
        {
            byte[] v;
            if (TemplateDataMap.TryGetValue(tid, out v))
                return v;
            else
                return null;
        }

        public void LoadFromBin(string filePathName)
        {
            var path = Template.Path.IsLocaleDataPath(filePathName) ? Path.GetFullLocalePath() : Path.GetFullBinPath();
            var fullPathName = System.IO.Path.Combine(path, filePathName);

#if UNITY_IPHONE
			fullPathName = fullPathName.Replace("file://", "");
#endif

            byte[] allbytes = null;
            try
            {
#if UNITY_EDITOR
                allbytes = File.ReadAllBytes(fullPathName);
#else
                allbytes = Util.ReadFile(fullPathName);
#endif
            }
            catch (Exception e)
            {
                HobaDebuger.LogWarningFormat("Exception in LoadFromBin: {0}!", e.Message);
                allbytes = null;
            }

            if (allbytes == null)
                return;

            MemoryStream memStream = new MemoryStream(allbytes);
            BinaryReader br = new BinaryReader(memStream);
            try
            {
                var totalSize = 0;
                //memStream.Seek(8, SeekOrigin.Current);
                ReadHeader(br, out HeaderVersion, out HeaderData);
                totalSize = 8;
                while (totalSize < memStream.Length)
                {
                    var tid = br.ReadInt32();
                    var size = br.ReadInt32();
                    byte[] pb = new byte[size];
                    memStream.Read(pb, 0, size);
                    if (TemplateDataMap.ContainsKey(tid))
                    {
                        throw new DataException(HobaText.Format("ÖØ¸´µÄ {0} ID({1})", fullPathName, tid));
                    }
                    TemplateDataMap.Add(tid, pb);
                    totalSize += (pb.Length + 2 * sizeof(int));
                }
            }
            catch (EndOfStreamException e)
            {
                HobaDebuger.LogErrorFormat("The length of data file {0} is error, {1}", fullPathName, e.Message);
            }
            catch (IOException)
            {
                HobaDebuger.LogErrorFormat("IOException raised in file {0}!", fullPathName);
            }
            finally
            {
                br.Close();
                memStream.Close();
            }
        }
    }
}