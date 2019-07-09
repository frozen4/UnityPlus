using Common;
using System;
using System.IO;
using System.Text;

public class DynEffectCalculator
{
    /// <summary>
    /// 该类功能
    /// 1 配置数据WeatherTimeConfigXml加载入口
    /// 2 记录服务器开服时间
    /// 3 根据当前时间 开服时间 和 配置数据，计算当前处于那种效果阶段中
    /// </summary>
   
    private WeatherTimeConifg _WeatherTimeConfig = null;

    /// 开服时间间隔
    private DateTime _ServerOpenTime = DateTime.UtcNow.AddDays(0);

    /// 开服时间间隔，用于Debug
    //private DateTime _ServerOpenTimeForDebug = DateTime.UtcNow.AddDays(0);

    public int RegionLerpTime
    {
        get
        {
            return _WeatherTimeConfig.TimeData.RegionLerpTime;
        }
    }

    public void Init()
    {
        if (_WeatherTimeConfig == null)
        {
            _WeatherTimeConfig = new WeatherTimeConifg();
            string path = Path.Combine(EntryPoint.Instance.ConfigPath, "WeatherTimeConfigXml.xml");
            byte[] bytes = Util.ReadFile(path);
            if (bytes == null)
            {
                HobaDebuger.LogError("WeatherTimeConfigXml文件读取失败！ " + path);
            }
            bool isReadSuccess = _WeatherTimeConfig.ParseFromXmlString(Encoding.UTF8.GetString(bytes));
            if (!isReadSuccess)
            {
                HobaDebuger.LogError("WeatherTimeConfigXml文件解析失败！ " + path);
            }
        }
    }

    public void SetServerStartTime(string timeString, double currentServerTime)
    {
        _ServerOpenTime = Convert.ToDateTime(timeString);
        //_ServerOpenTimeForDebug = _ServerOpenTime;
    }

    public DynamicEffectType GetDynamicEffectType()
    {
        if (_WeatherTimeConfig == null)
            Init();

        var millSeconds = EntryPoint.GetServerTime() - (_ServerOpenTime - Main.DateTimeBegin).TotalMilliseconds;
        var config = _WeatherTimeConfig.TimeData;
        var timeSpanSeconds = (millSeconds / 1000) % (config.MorningLastTime + config.DayLastTime + config.DustLastTime + config.NightLastTime);

        if (timeSpanSeconds >= config.MorningLastTime + config.DayLastTime + config.DustLastTime)
            return DynamicEffectType.Night;

        if (timeSpanSeconds >= config.MorningLastTime + config.DayLastTime)
            return DynamicEffectType.Dusk;

        if (timeSpanSeconds >= config.MorningLastTime)
            return DynamicEffectType.Day;

        return DynamicEffectType.Morning;
    }

    public int GetDynamicEffectLerpTime(DynamicEffectType tpye)
    {
        var config = _WeatherTimeConfig.TimeData;

        if (tpye == DynamicEffectType.Morning)
        {
            return config.MorningLerpTime;
        }
        else if (tpye == DynamicEffectType.Day)
        {
            return config.DayLerpTime;
        }
        else if (tpye == DynamicEffectType.Dusk)
        {
            return config.DustLerpTime;
        }
        else if (tpye == DynamicEffectType.Night)
        {
            return config.NightLerpTime;
        }
        else 

        return 0;
    }

    public void Release()
    {
        _WeatherTimeConfig = null;
    }
}
