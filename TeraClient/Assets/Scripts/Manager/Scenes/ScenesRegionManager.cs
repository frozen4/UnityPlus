using Common;
using MapRegion;
using System;

public class ScenesRegionManager : Singleton<ScenesRegionManager>
{
    /// <summary>
    /// 功能定位
    /// 场景分块信息管理，包括：
    /// 1 分块数据文件的读取、解析
    /// 2 对外提供当前主角所在Region的查询接口
    /// </summary>
    

    private FileMapRegion _StaticLightMapRegionFile = null;
    private bool _HasStaticLightMapRegion = false;

    private FileMapRegion _BlockLoadingRegionFile = null;
    private bool _HasBlockLoadingRegionFile = false;

    public int CurLightmapRegionID
    {
        get
        {
            if (null == Main.HostPalyer) return 0;
            if (null == _StaticLightMapRegionFile) return 0;
            if (!_HasStaticLightMapRegion) return 0;
            return _StaticLightMapRegionFile.InWhichLightRegion(Main.HostPalyer.position.x, Main.HostPalyer.position.z);
        }
    }

    public int CurBlockReigionID
    {
        get
        {
            if (null == Main.HostPalyer) return 0;
            if (null == _BlockLoadingRegionFile) return 0;
            if (!_HasBlockLoadingRegionFile) return 0;
            return _BlockLoadingRegionFile.InWhichLightRegion(Main.HostPalyer.position.x, Main.HostPalyer.position.z);
        }
    }

    public void Init(string lightRegionName)
    {
        _StaticLightMapRegionFile = new FileMapRegion();
        _HasStaticLightMapRegion = false;

        _BlockLoadingRegionFile = new FileMapRegion();
        _HasBlockLoadingRegionFile = false;
        string fileName = HobaText.Format("{0}/Maps/{1}.lightregion", EntryPoint.Instance.ResPath, lightRegionName);
        try
        {
            byte[] region_data = Util.ReadFile(fileName);
            _HasStaticLightMapRegion = _StaticLightMapRegionFile.ReadFromMemory(region_data, true);
        }
        catch (Exception)
        {
            _HasStaticLightMapRegion = false;
        }

        fileName = HobaText.Format("{0}/Maps/{1}.blockregion", EntryPoint.Instance.ResPath, lightRegionName);
        try
        {
            byte[] regionData = Util.ReadFile(fileName);
            _HasBlockLoadingRegionFile = _BlockLoadingRegionFile.ReadFromMemory(regionData, true);
        }
        catch (Exception)
        {
            _HasBlockLoadingRegionFile = false;
        }
    }

    public void Clear()
    {
        _StaticLightMapRegionFile = null;
        _HasStaticLightMapRegion = false;
        _BlockLoadingRegionFile = null;
        _HasBlockLoadingRegionFile = false;
    }
}
