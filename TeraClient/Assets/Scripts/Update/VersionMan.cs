using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

public struct ELEMENT_VER
{
    public int iVer0;
    public int iVer1;
    public int iVer2;
    public int iVer3;   //不显示，表示文件号

    public ELEMENT_VER(int ver0, int ver1, int ver2, int ver3)
    {
        iVer0 = ver0;
        iVer1 = ver1;
        iVer2 = ver2;
        iVer3 = ver3;
    }

    public void Clear()
    {
        iVer0 = 0;
        iVer1 = 0;
        iVer2 = 0;
        iVer3 = 0;
    }

    public void Set(int ver0, int ver1, int ver2, int ver3)
    {
        iVer0 = ver0;
        iVer1 = ver1;
        iVer2 = ver2;
        iVer3 = ver3;
    }

    public bool IsValid()
    {
        return iVer0 >= 0 && iVer1 >= 0 && iVer2 >= 0 && iVer3 >= 0;
    }

    public string ToShortString()
    {
        return HobaText.Format("{0}.{1}.{2}", iVer0, iVer1, iVer2);
    }

    public override string ToString()
    {
        return HobaText.Format("{0}.{1}.{2}.{3}", iVer0, iVer1, iVer2, iVer3);
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(ELEMENT_VER))
            return false;
        ELEMENT_VER? ver = obj as ELEMENT_VER?;
        if (!ver.HasValue)
            return false;

        if (iVer0 == ver.Value.iVer0 && iVer1 == ver.Value.iVer1 && iVer2 == ver.Value.iVer2 && iVer3 == ver.Value.iVer3)
            return true;
        return false;
    }

    public override int GetHashCode()
    {
        return iVer0 * 1000 + iVer1 * 100 + iVer2 * 10 + iVer3;
    }

    public bool Parse(string str)
    {
        try 
        {
            string[] arr = str.Split('.');
            if (arr.Length == 3)
            {
                iVer0 = Int32.Parse(arr[0]);
                iVer1 = Int32.Parse(arr[1]);
                iVer2 = Int32.Parse(arr[2]);
                iVer3 = 0;
            }
            else if (arr.Length == 4)
            {
                iVer0 = Int32.Parse(arr[0]);
                iVer1 = Int32.Parse(arr[1]);
                iVer2 = Int32.Parse(arr[2]);
                iVer3 = Int32.Parse(arr[3]);
            }
        }
        catch(Exception)
        {
            return false;
        }

        return true;
    }

    public static bool operator == (ELEMENT_VER ver1, ELEMENT_VER ver2)
    {
        if (ver1.iVer0 == ver2.iVer0 && ver1.iVer1 == ver2.iVer1 && ver1.iVer2 == ver2.iVer2 && ver1.iVer3 == ver2.iVer3)
            return true;
        return false;
    }

    public static bool operator != (ELEMENT_VER ver1, ELEMENT_VER ver2)
    {
        if (ver1.iVer0 != ver2.iVer0 || ver1.iVer1 != ver2.iVer1 || ver1.iVer2 != ver2.iVer2 || ver1.iVer3 != ver2.iVer3)
            return true;
        return false;
    }

    public static bool operator < (ELEMENT_VER ver1, ELEMENT_VER ver2)
    {
        if (ver1.iVer0 < ver2.iVer0)
            return true;

        if (ver1.iVer0 == ver2.iVer0)
        {
            if (ver1.iVer1 < ver2.iVer1)
                return true;

            if (ver1.iVer1 == ver2.iVer1)
            {
                if (ver1.iVer2 < ver2.iVer2)
                    return true;

                if (ver1.iVer2 == ver2.iVer2)
                {
                    if (ver1.iVer3 < ver2.iVer3)
                        return true;
                }
            }
        }

        return false;
    }

    public static bool operator > (ELEMENT_VER ver1, ELEMENT_VER ver2)
    {
        if (ver1.iVer0 > ver2.iVer0)
            return true;

        if (ver1.iVer0 == ver2.iVer0)
        {
            if (ver1.iVer1 > ver2.iVer1)
                return true;

            if (ver1.iVer1 == ver2.iVer1)
            {
                if (ver1.iVer2 > ver2.iVer2)
                    return true;

                if (ver1.iVer2 == ver2.iVer2)
                {
                    if (ver1.iVer3 > ver2.iVer3)
                        return true;
                }
            }
        }

        return false;
    }
}

public struct VER_PAIR : IEquatable<VER_PAIR>
{
    public ELEMENT_VER VerFrom;
    public ELEMENT_VER VerTo;
    public string md5;
    public long size;
    public bool Equals(VER_PAIR other)
    {
        //throw new NotImplementedException();
        if (!(this.VerFrom.Equals(other.VerFrom)))
            return false;
        if (!(this.VerTo.Equals(other.VerTo)))
            return false;
        if (!(this.md5.Equals(other.md5)))
            return false;
        if (!(this.size.Equals(other.size)))
            return false;
        else
            return true;
    }
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (!(obj is VER_PAIR))
            return false;
        return Equals((VER_PAIR)obj);

    }
    public override int GetHashCode()
    {
        return size.GetHashCode() ^ md5.GetHashCode() ^ VerTo.GetHashCode() ^ VerFrom.GetHashCode();
    }
}

public sealed class VersionMan
{
    public VersionMan()
    {
        _ProjectName = "";
        _Loaded = false;

        _VerLatest = new ELEMENT_VER(0, 0, 0, 0);
        _VerSperate = new ELEMENT_VER(0, 0, 0, 0);
        _VersionPairs = new List<VER_PAIR>();
    }

    public bool LoadVersions(StreamReader reader)
    {
        _Loaded = false;

        if (reader == null)
            return false;

        string strLine;
        bool success = false;
        try
        {
            char[] split = new char[] {' ', '\t'};
            
            
            //第一行
            {
                strLine = reader.ReadLine();
                if (strLine == null)
                    return false;

                //Version:	1.0.1.0/1.0.0.0
                {
                    var arr = strLine.Split(split);
                    if (arr.Length >= 2 && arr[0] == "Version:")
                    {
                        var arr1 = arr[1].Split('/');
                        success = (arr1.Length >= 2 && _VerLatest.Parse(arr1[0]) && _VerSperate.Parse(arr1[1]));
                        if (!success)
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            //第二行
            {
                strLine = reader.ReadLine();
                if (strLine == null)
                    return false;

                //Project:	jason-cn
                {
                    var arr = strLine.Split(split);
                    if (arr.Length >= 2 && arr[0] == "Project:")
                    {
                        _ProjectName = arr[1];
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            //后续版本
            //1.0.0.0-1.0.1.0	297E9DD6EC4CC92C409295C4B89BBAC2 33697
            {
                _VersionPairs.Clear();
                VER_PAIR tmpPair = new VER_PAIR() {
                    VerFrom = new ELEMENT_VER(0,0,0,0),
                    VerTo = new ELEMENT_VER(0,0,0,0),
                    md5 = "",
                    size = 0 };

                strLine = reader.ReadLine();
                while (strLine != null)
                {
                    if (strLine.Length == 0)
                    {
                        strLine = reader.ReadLine();
                        continue;
                    }

                    var arr = strLine.Split(split);
                    if (arr.Length < 3)
                        return false;

                    var arr1 = arr[0].Split('-');
                    if (arr1.Length < 2)
                        return false;

                    success = (tmpPair.VerFrom.Parse(arr1[0]) && tmpPair.VerTo.Parse(arr1[1]));
                    if (!success)
                        return false;

                    tmpPair.md5 = arr[1];
                    tmpPair.size = Int64.Parse(arr[2]);

                    _VersionPairs.Add(tmpPair);

                    //next line
                    strLine = reader.ReadLine();
                } 
            }

        }
        catch(IOException)
        {
            return false;
        }

        _Loaded = true;
        return true;
    }

    public bool CanAutoUpdate(ELEMENT_VER ver)
    {
        return VerSeperate < ver;
    }

    public bool FindVersionPair(ELEMENT_VER curVer, out VER_PAIR verPair)
    {
        if (_VersionPairs.Count == 0 ||
            curVer == _VerLatest ||
            curVer > _VerLatest ||
            curVer < _VerSperate)
        {
            verPair = new VER_PAIR();
            return false;
        }

        //找起始点版本
        ELEMENT_VER verOld = new ELEMENT_VER(-1, 0, 0, 0);
        for(int i = 0; i < _VersionPairs.Count; ++i)
        {
            var pair = _VersionPairs[i];
            if (curVer == pair.VerFrom)
            {
                verOld = pair.VerFrom;
                break;
            }
        }

        if(verOld.iVer0 < 0)        //没有找到
        {
            verPair = new VER_PAIR();
            return false;

        }

        //找最高的目标版本
        int iVer = -1;
        ELEMENT_VER verNew = _VerSperate;
        for(int i=0; i<_VersionPairs.Count; ++i)
        {
            if (_VersionPairs[i].VerFrom != verOld)
                continue;

            if (_VersionPairs[i].VerTo > verNew)
            {
                iVer = i;
                verNew = _VersionPairs[i].VerTo;
            }
        }

        if(iVer < 0)        //没有找到
        {
            verPair = new VER_PAIR();
            return false;
        }
        else
        {
            verPair = _VersionPairs[iVer];
            return true;
        }
    }

    public long CalcSize(ELEMENT_VER verFrom, ELEMENT_VER verTo)
    {
        if (verFrom == verTo || verFrom > verTo)
            return 0;

        VER_PAIR verPair;
        if (!FindVersionPair(verFrom, out verPair))
            return -1;

        long sizeOverAll = 0;
        while( verPair.VerTo == verTo || verPair.VerTo < verTo)
        {
            sizeOverAll += verPair.size;

            if (!FindVersionPair(verPair.VerTo, out verPair))
                break;
        }
        return sizeOverAll;
    }

    public int CalcPackCount(ELEMENT_VER verFrom, ELEMENT_VER verTo)
    {
        if (verFrom == verTo || verFrom > verTo)
            return 0;

        VER_PAIR verPair;
        if (!FindVersionPair(verFrom, out verPair))
            return 0;

        int packCount = 0;
        while (verPair.VerTo == verTo || verPair.VerTo < verTo)
        {
            ++packCount;

            if (!FindVersionPair(verPair.VerTo, out verPair))
                break;
        }
        return packCount;
    }

    public bool IsLoaded { get { return _Loaded; } }
    public string ProjectName { get { return _ProjectName; } }
    public ELEMENT_VER VerLastest { get { return _VerLatest; } }
    public ELEMENT_VER VerSeperate { get { return _VerSperate; } }

    private string _ProjectName;
    private ELEMENT_VER _VerLatest;         //最新版本
    private ELEMENT_VER _VerSperate;        //基础版本
    private bool _Loaded;
    private List<VER_PAIR> _VersionPairs;

}

