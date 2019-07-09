using System;
using System.Collections.Generic;
using System.Text;
using Mono.Xml;
using System.Security;
using UnityEngine;
using System.Collections;
using System.IO;


enum ServerState
{
    Good = 0,//良好
    Normal = 1,//一般
    Busy = 2,//火爆
    Unuse = 3,//不可登录
};

//JSON
[Serializable]
public class ServerListData
{
    public List<ServerItem> serverlist;
}

[Serializable]
public class ServerItem
{
    public int zoneId;
    public string zoneName;
    public string ip;
    public int port;
    public bool recommend;
    public bool show;
    public int state;
    public bool newFlag;
    public bool roleCreateDisenable;
}

public class ServerInfo
{
    public string name;
    public string ip;
    public int port;
    public int state;
    public bool recommend;
    public bool show;
    public int zoneId;
    public bool newFlag;
    public bool roleCreateDisable;

    public static bool ParseFromXmlString(string content, List<ServerInfo> serverList)
    {
        

        bool bRet = false;
        serverList.Clear();
        try
        {
            string text = content; // Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(content));

            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(text);

            SecurityElement root = parser.ToXml();

            if (root.Tag == "ServerList")
            {
                for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
                {
                    SecurityElement ele = (SecurityElement)root.Children[i];
                    if (ele.Tag != "Server")
                        continue;

                    bool bVal;
                    ServerInfo server = new ServerInfo();
                    server.state = 0;
                    server.zoneId = int.Parse(ele.Attribute("zoneId"));
                    server.name = ele.Attribute("name");
                    server.ip = ele.Attribute("ip");
                    server.port = int.Parse(ele.Attribute("port"));
                    if (bool.TryParse(ele.Attribute("recommend"), out bVal))
                        server.recommend = bVal;
                    else
                        server.recommend = false;


                    if (bool.TryParse(ele.Attribute("show"), out bVal))
                        server.show = bVal;
                    else
                        server.show = false;

                    if (bool.TryParse(ele.Attribute("newFlag"), out bVal))
                        server.newFlag = bVal;
                    else
                        server.newFlag = false;

                    if (bool.TryParse(ele.Attribute("roleCreateDisenable"), out bVal))
                        server.roleCreateDisable = bVal;
                    else
                        server.roleCreateDisable = false;

                    serverList.Add(server);
                }
                bRet = true;
            }

            parser.Clear();
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("ServerInfo ParseFromXmlString Exception {0}", e.Message));
        }
        return bRet;
    }

    public static bool ParseFromJsonString(string content, List<ServerInfo> serverList)
    {
        bool bRet = false;
        serverList.Clear();
        try
        {
            string text = content; // Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(content));
            //text = "{" + "\"serverlist\":" + text + "}";

            ServerListData data = JsonUtility.FromJson<ServerListData>(text);
            if (data == null)
                return bRet;

            for (int i = 0; i < data.serverlist.Count; ++i)
            {
                var item = data.serverlist[i];
                serverList.Add(new ServerInfo()
                {
                    name = item.zoneName,
                    ip = item.ip,
                    port = item.port,
                    recommend = item.recommend,
                    show = item.show,
                    state = item.state,
                    zoneId = item.zoneId,
                    newFlag = item.newFlag,
                    roleCreateDisable = item.roleCreateDisenable,
                });
            }

            bRet = true;
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("ServerInfo ParseFromJsonString Exception {0}", e.Message));
        }
        return bRet;
    }
}

[Serializable]
public class AccountRoleData
{
    public string account;
    public List<AccountRoleItem> roleZonesInfos;
    public List<OrderZoneInfo> orderZonesInfos;
    public List<ServerItem> testZonesInfos;
    public string return_code;
}

[Serializable]
public class AccountRoleItem
{
    public int roleId;
    public int level;
    public string name;
    public int iconState;
    public int profession;
    public int zoneId;
    public int zoneFlag;
}

[Serializable]
public class OrderZoneInfo
{
    public int zoneId;
}

public class AccountRoleInfo
{
    public int roleId;
    public int level;
    public string name;
    public int profession;
    public int customSet;
    public int zoneId;
    //public int zoneState;

    private static int orderZoneId = 0; // 账号预约服务器ID，默认必须为0
    public static int OrderZoneId { get { return orderZoneId; } }

    public static bool ParseFromJsonString(string account, string content, List<AccountRoleInfo> accountRoleList, List<ServerInfo> testServerList)
    {
        bool bRet = false;
        accountRoleList.Clear();
        testServerList.Clear();
        orderZoneId = 0;
        try
        {
            string text = content; // Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(content));
            //text = "{" + "\"serverlist\":" + text + "}";

            var data = JsonUtility.FromJson<AccountRoleData>(text);
            if (data == null || data.return_code == null || data.roleZonesInfos == null)
            {
                DeviceLogger.Instance.WriteLog(HobaText.Format("AccountRoleInfo ParseFromJsonString failed, some data got null, account:{0}", account));
                return bRet;
            }

            if (!data.return_code.Equals("SUCCESS"))
            {
                DeviceLogger.Instance.WriteLog(HobaText.Format("AccountRoleInfo ParseFromJsonString return code:{0}, account:{1}", data.return_code, account));
                //return bRet;
            }

            for (int i = 0; i < data.roleZonesInfos.Count; ++i)
            {
                var item = data.roleZonesInfos[i];
                accountRoleList.Add(new AccountRoleInfo()
                {
                    roleId = item.roleId,
                    level = item.level,
                    name = item.name,
                    profession = item.profession,
                    customSet = item.iconState,
                    zoneId = item.zoneId,
                    //zoneState = item.zoneFlag
                });
            }

            if (data.orderZonesInfos != null && data.orderZonesInfos.Count > 0)
                orderZoneId = data.orderZonesInfos[0].zoneId;

            if (data.testZonesInfos != null)
            {
                for (int i = 0; i < data.testZonesInfos.Count; ++i)
                {
                    var item = data.testZonesInfos[i];
                    testServerList.Add(new ServerInfo()
                    {
                        name = item.zoneName,
                        ip = item.ip,
                        port = item.port,
                        recommend = item.recommend,
                        show = item.show,
                        state = item.state,
                        zoneId = item.zoneId,
                        newFlag = item.newFlag,
                        roleCreateDisable = item.roleCreateDisenable,
                    });
                }
            }

            bRet = true;
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("AccountRoleInfo ParseFromJsonString Exception {0}", e.Message));
        }
        return bRet;
    }

    public static void ResetOrderZoneId()
    {
        orderZoneId = 0;
    }
}

//--------------------------------

//--------------------------------
//静态配置，从XML文件读取
//--------------------------------
public class ServerConfigParams
{
    private string DynamicServerUrl = string.Empty;
    private string DynamicAccountRoleUrl = string.Empty;
    private string CustomPicUploadUrl = string.Empty;
    private string CustomPicDownloadUrl = string.Empty;
    private string PurchaseVerifyUrl = string.Empty;

    public void DefaultValue()
    {
        DynamicServerUrl = string.Empty;  //"http://10.35.49.145:5000/api/Zones/ServerList";
        DynamicAccountRoleUrl = string.Empty;
        CustomPicUploadUrl = string.Empty;
        CustomPicDownloadUrl = string.Empty;
        PurchaseVerifyUrl = string.Empty;   //http://10.35.49.145:5000/api/iap/verify
    }

    public string GetDynamicServerUrl()
    {
        return DynamicServerUrl;
    }

    public string GetDynamicAccountRoleUrl()
    {
        return DynamicAccountRoleUrl;
    }

    public string GetCustomPicUploadUrl()
    {
        return CustomPicUploadUrl;
    }

    public string GetCustomPicDownloadUrl()
    {
        return CustomPicDownloadUrl;
    }
    public string GetPurchaseVerifyUrl()
    {
        return PurchaseVerifyUrl;
    }

    public bool ParseFromXmlString(string content)
    {
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);
            SecurityElement root = parser.ToXml();
            if (root.Tag == "ServerConfig")
            {
                for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
                {
                    SecurityElement ele = (SecurityElement)root.Children[i];
                    switch (ele.Tag)
                    {
                        case "DynamicServer":
                            DynamicServerUrl = ele.Attribute("url");
                            break;
                        case "DynamicAccountRole":
                            DynamicAccountRoleUrl = ele.Attribute("url");
                            break;
                        case "CustomPicUploadUrl":
                            CustomPicUploadUrl = ele.Attribute("url");
                            break;
                        case "CustomPicDownloadUrl":
                            CustomPicDownloadUrl = ele.Attribute("url");
                            break;
                        case "PurchaseVerifyUrl":
                            PurchaseVerifyUrl = ele.Attribute("url");
                            break;
                    }
                }
            }
            parser.Clear();
            return true;
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("Exception {0}", e.Message));
        }
        return false;
    }
}

public class UpdateConfigParams            //更新所需要的url, 从Resources/UpdateConfig.xml 读取
{
    private string UpdateServerUrl;
    private string UpdateServerUrl2;
    private string UpdateServerUrl3;
    private string ClientServerUrl;

    public void DefaultValue()
    {
        UpdateServerUrl = ""; // "http://111.205.196.85/meteorite/patches/";
        UpdateServerUrl2 = "";
        UpdateServerUrl3 = "";
        ClientServerUrl = ""; //"http://172.16.90.13/clientserver/";
    }

    public string GetUpdateServerUrl()
    {
        return UpdateServerUrl;
    }

    public string GetUpdateServerUrl2()
    {
        return UpdateServerUrl2;
    }

    public string GetUpdateServerUrl3()
    {
        return UpdateServerUrl3;
    }

    public string GetClientServerUrl()
    {
        return ClientServerUrl;
    }

    public bool ParseFromXmlString(string content, string strLocale)
    {
        bool bRet = false;
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);

            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                SecurityElement eleLocale = (SecurityElement)root.Children[i];
                if (eleLocale.Tag != strLocale)
                    continue;
                for (int j = 0; j < (eleLocale.Children != null ? eleLocale.Children.Count : 0); ++j)
                {
                    SecurityElement ele = (SecurityElement)eleLocale.Children[j];
                    switch (ele.Tag)
                    {
                        case "UpdateServer":
                            UpdateServerUrl = ele.Attribute("url");
                            break;
                        case "UpdateServer2":
                            UpdateServerUrl2 = ele.Attribute("url");
                            break;
                        case "UpdateServer3":
                            UpdateServerUrl3 = ele.Attribute("url");
                            break;
                        case "ClientServer":
                            ClientServerUrl = ele.Attribute("url");
                            break;
                    }
                }
            }
            parser.Clear();
            bRet = true;
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("Exception {0}", e.Message));
        }
        return bRet;
    }
}

public class UpdateStringConfigParams       //程序中更新配置的字符串, 从Resources/UpdateStringConfig.xml 读取
{
    public string UpdateState_None;
    public string UpdateState_Success;
    public string UpdateState_UpdateVersionFileErr;
    public string UpdateState_OpenPackFileErr;
    public string UpdateState_DiskSpaceFullErr;
    public string UpdateState_GetLocalVersionErr;
    public string UpdateState_GetNewVersionErr;
    public string UpdateState_NetworkErr;
    public string UpdateState_NetConnectionErr;
    public string UpdateState_NetUnstable;
    public string UpdateState_DownloadCheck;
    public string UpdateState_LocalVersionTooOldErr;
    public string UpdateState_ServerMaintananceErr;
    public string UpdateState_AutoUpdateErr;
    public string UpdateState_UnknownErr;
    public string UpdateState_DownloadingErrAutoRetry;
    public string UpdateStatus_Start;
    public string UpdateStatus_TryGetLocalVersion;
    public string UpdateStatus_TryCheckFreeSpace;
    public string UpdateStatus_TryGetNewVersion;
    public string UpdateStatus_TryDNSResolving;
    public string UpdateStatus_SuccessGetVersions;
    public string UpdateStatus_BeginUpdate;
    public string UpdateStatus_LastWritePackErr;
    public string UpdateStatus_Downloading;
    public string UpdateStatus_WritingPack;
    public string UpdateStatus_DownloadingServerList;
    public string UpdateStatus_CheckingExistPack;
    public string UpdateStatus_PrepareWritingPack;
    public string UpdateString_CurrentVersion;
    public string UpdateString_ServerVersion;
    public string UpdateString_Yes;
    public string UpdateString_No;
    public string UpdateString_Ok;
    public string UpdateString_Cancel;
    public string UpdateString_HasErrorNotWifi;
    public string UpdateString_HasErrorRetry;
    public string UpdateString_HasError;
    public string UpdateString_HasFatalErrorNeedReinstall;
    public string UpdateString_HasFatalErrorNeedCleanUp;
    public string UpdateString_HasFatalError;
    public string UpdateString_PrepareForFirstTimeUse;
    public string UpdateString_EnsureEnoughSpaceMB;
    public string UpdateString_TextUpdate;
    public string UpdateString_FileSize;
    public string UpdateString_DownloadSpeed;
    public string UpdateString_WifiTips;
    public string UpdateString_VideoTitle;
    public string UpdateString_PictureTitle;

    public string PlatformSDKString_InitFailedTitle;
    public string PlatformSDKString_ErrorCode;
    public string PlatformSDKString_UnknownErr;
    public string PlatformSDKString_NetworkFailure;
    public string PlatformSDKString_ServerTimeout;
    public string PlatformSDKString_InitializationFailed;

    public string SystemRequire_Memory;
    public string SystemRequire_Space;


    public void DefaultValue()
    {
        UpdateState_None = "";
        SystemRequire_Memory = "System memory is less than {0} MB. Game may not work properly，Continue?";
        SystemRequire_Space = "System free storage is less than {0} MB. Game may not work properly";
        UpdateState_Success = "Starting Game, Please Wait...";
        UpdateState_UpdateVersionFileErr = "Update Version File Error";
        UpdateState_OpenPackFileErr = "Open Pack File Error";
        UpdateState_DiskSpaceFullErr = "Disk Space Not Enough";
        UpdateState_GetLocalVersionErr = "Get Local Version Error";
        UpdateState_GetNewVersionErr = "Can Not Get Server Version. Please Check Network Connection";
        UpdateState_NetworkErr = "Network Error";
        UpdateState_NetConnectionErr = "Network Connection Error";
        UpdateState_NetUnstable = "Network Downloading is Unstable, Please Try Later...";
        UpdateState_DownloadCheck = "Need Updating, estimated download size is ";
        UpdateState_LocalVersionTooOldErr = "Local Version Too Old";
        UpdateState_ServerMaintananceErr = "Can Not Update to Server Version"; // "服务器无法找到升级版本";
        UpdateState_AutoUpdateErr = "Auto Update Error";
        UpdateState_UnknownErr = "Unknown Error";
        UpdateState_DownloadingErrAutoRetry = "Downloading Error, AutoRetry Later...";
        UpdateStatus_Start = "Update Start...";
        UpdateStatus_TryGetLocalVersion = "Getting Local Version...";
        UpdateStatus_TryCheckFreeSpace = "Checking Free Space...";
        UpdateStatus_TryGetNewVersion = "Getting Server Version...";
        UpdateStatus_TryDNSResolving = "DNS Resolving...";
        UpdateStatus_SuccessGetVersions = "Get Versions Success";
        UpdateStatus_BeginUpdate = "Begin Update...";
        UpdateStatus_LastWritePackErr = "Writing Pack Err Last Time. Please Restart Application to Update"; // "上次写包错误，请重新运行程序更新";
        UpdateStatus_Downloading = "Downloading";
        UpdateStatus_WritingPack = "Writing Pack";
        UpdateStatus_DownloadingServerList = "Downloading ServerList";
        UpdateStatus_CheckingExistPack = "Checking Exist Pack, Please Wait...";
        UpdateStatus_PrepareWritingPack = "Preparing Writing Pack, Please Wait...";
        UpdateString_CurrentVersion = "Current Version";
        UpdateString_ServerVersion = "Server Version";
        UpdateString_Yes = "Yes";
        UpdateString_No = "No";
        UpdateString_Ok = "Ok";
        UpdateString_Cancel = "Cancel";
        UpdateString_HasErrorNotWifi = "No Wifi Network, Continue Downloading?";
        UpdateString_HasErrorRetry = "Has Error, Retry?";
        UpdateString_HasError = "Has Error";
        UpdateString_HasFatalErrorNeedReinstall = "Has Fatal Error, App Needs Reinstall!";
        UpdateString_HasFatalErrorNeedCleanUp = "Has Fatal Error, Update Needs Cleaning Up!";
        UpdateString_HasFatalError = "Has Fatal Error";
        UpdateString_PrepareForFirstTimeUse = "Prepare For The First Time Use...";
        UpdateString_EnsureEnoughSpaceMB = "Please Ensure Device Has Enough Storage Space";
        UpdateString_TextUpdate = "Updating，Do not close...";
        UpdateString_FileSize = "File Size:";
        UpdateString_DownloadSpeed = "Download Speed:";
        UpdateString_WifiTips = "Download In WIFI Environment Recommended";
        UpdateString_VideoTitle = "Video";
        UpdateString_PictureTitle = "Guide";

        PlatformSDKString_InitFailedTitle = "Error";
        PlatformSDKString_ErrorCode = "(ErrorCode：{0})";
        PlatformSDKString_UnknownErr = "Platform Unknown Error.";
        PlatformSDKString_NetworkFailure = "Network unstable, Please check the network status before trying again.";
        PlatformSDKString_ServerTimeout = "Response timeout, Please try again.";
        PlatformSDKString_InitializationFailed = "Initialization failed, Please close the app and restart.";
    }

    public bool ParseFromXmlString(string content)
    {
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);

            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                SecurityElement ele = (SecurityElement)root.Children[i];
                switch (ele.Tag)
                {
                    case "UpdateState_Success":
                        this.UpdateState_Success = ele.Attribute("text");
                        break;
                    case "UpdateState_UpdateVersionFileErr":
                        this.UpdateState_UpdateVersionFileErr = ele.Attribute("text");
                        break;
                    case "UpdateState_OpenPackFileErr":
                        this.UpdateState_OpenPackFileErr = ele.Attribute("text");
                        break;
                    case "UpdateState_DiskSpaceFullErr":
                        this.UpdateState_DiskSpaceFullErr = ele.Attribute("text");
                        break;
                    case "UpdateState_GetLocalVersionErr":
                        this.UpdateState_GetLocalVersionErr = ele.Attribute("text");
                        break;
                    case "UpdateState_GetNewVersionErr":
                        this.UpdateState_GetNewVersionErr = ele.Attribute("text");
                        break;
                    case "UpdateState_NetworkErr":
                        this.UpdateState_NetworkErr = ele.Attribute("text");
                        break;
                    case "UpdateState_NetConnectionErr":
                        this.UpdateState_NetConnectionErr = ele.Attribute("text");
                        break;
                    case "UpdateState_NetUnstable":
                        this.UpdateState_NetUnstable = ele.Attribute("text");
                        break;
                    case "UpdateState_DownloadCheck":
                        this.UpdateState_DownloadCheck = ele.Attribute("text");
                        break;
                    case "UpdateState_LocalVersionTooOldErr":
                        this.UpdateState_LocalVersionTooOldErr = ele.Attribute("text");
                        break;
                    case "UpdateState_ServerMaintananceErr":
                        this.UpdateState_ServerMaintananceErr = ele.Attribute("text");
                        break;
                    case "UpdateState_AutoUpdateErr":
                        this.UpdateState_AutoUpdateErr = ele.Attribute("text");
                        break;
                    case "UpdateState_UnknownErr":
                        this.UpdateState_UnknownErr = ele.Attribute("text");
                        break;
                    case "UpdateState_DownloadingErrAutoRetry":
                        this.UpdateState_DownloadingErrAutoRetry = ele.Attribute("text");
                        break;
                    case "UpdateStatus_Start":
                        this.UpdateStatus_Start = ele.Attribute("text");
                        break;
                    case "UpdateStatus_TryGetLocalVersion":
                        this.UpdateStatus_TryGetLocalVersion = ele.Attribute("text");
                        break;
                    case "UpdateStatus_TryCheckFreeSpace":
                        this.UpdateStatus_TryCheckFreeSpace = ele.Attribute("text");
                        break;
                    case "UpdateStatus_TryGetNewVersion":
                        this.UpdateStatus_TryGetNewVersion = ele.Attribute("text");
                        break;
                    case "UpdateStatus_TryDNSResolving":
                        this.UpdateStatus_TryDNSResolving = ele.Attribute("text");
                        break;
                    case "UpdateStatus_SuccessGetVersions":
                        this.UpdateStatus_SuccessGetVersions = ele.Attribute("text");
                        break;
                    case "UpdateStatus_BeginUpdate":
                        this.UpdateStatus_BeginUpdate = ele.Attribute("text");
                        break;
                    case "UpdateStatus_LastWritePackErr":
                        this.UpdateStatus_LastWritePackErr = ele.Attribute("text");
                        break;
                    case "UpdateStatus_Downloading":
                        this.UpdateStatus_Downloading = ele.Attribute("text");
                        break;
                    case "UpdateStatus_WritingPack":
                        this.UpdateStatus_WritingPack = ele.Attribute("text");
                        break;
                    case "UpdateStatus_DownloadingServerList":
                        this.UpdateStatus_DownloadingServerList = ele.Attribute("text");
                        break;
                    case "UpdateStatus_CheckingExistPack":
                        this.UpdateStatus_CheckingExistPack = ele.Attribute("text");
                        break;
                    case "UpdateStatus_PrepareWritingPack":
                        this.UpdateStatus_PrepareWritingPack = ele.Attribute("text");
                        break;
                    case "UpdateString_CurrentVersion":
                        this.UpdateString_CurrentVersion = ele.Attribute("text");
                        break;
                    case "UpdateString_ServerVersion":
                        this.UpdateString_ServerVersion = ele.Attribute("text");
                        break;
                    case "UpdateString_Yes":
                        this.UpdateString_Yes = ele.Attribute("text");
                        break;
                    case "UpdateString_No":
                        this.UpdateString_No = ele.Attribute("text");
                        break;
                    case "UpdateString_Ok":
                        this.UpdateString_Ok = ele.Attribute("text");
                        break;
                    case "UpdateString_Cancel":
                        this.UpdateString_Cancel = ele.Attribute("text");
                        break;
                    case "UpdateString_HasErrorNotWifi":
                        this.UpdateString_HasErrorNotWifi = ele.Attribute("text");
                        break;
                    case "UpdateString_HasErrorRetry":
                        this.UpdateString_HasErrorRetry = ele.Attribute("text");
                        break;
                    case "UpdateString_HasError":
                        this.UpdateString_HasError = ele.Attribute("text");
                        break;
                    case "UpdateString_HasFatalErrorNeedReinstall":
                        this.UpdateString_HasFatalErrorNeedReinstall = ele.Attribute("text");
                        break;
                    case "UpdateString_HasFatalErrorNeedCleanUp":
                        this.UpdateString_HasFatalErrorNeedCleanUp = ele.Attribute("text");
                        break;
                    case "UpdateString_HasFatalError":
                        this.UpdateString_HasFatalError = ele.Attribute("text");
                        break;
                    case "UpdateString_PrepareForFirstTimeUse":
                        this.UpdateString_PrepareForFirstTimeUse = ele.Attribute("text");
                        break;
                    case "UpdateString_EnsureEnoughSpaceMB":
                        this.UpdateString_EnsureEnoughSpaceMB = ele.Attribute("text");
                        break;
                    case "UpdateString_TextUpdate":
                        this.UpdateString_TextUpdate = ele.Attribute("text");
                        break;
                    case "UpdateString_FileSize":
                        this.UpdateString_FileSize = ele.Attribute("text");
                        break;
                    case "UpdateString_DownloadSpeed":
                        this.UpdateString_DownloadSpeed = ele.Attribute("text");
                        break;
                    case "UpdateStatus_WifiTips":
                        this.UpdateString_WifiTips = ele.Attribute("text");
                        break;
                    case "UpdateString_VideoTitle":
                        this.UpdateString_VideoTitle = ele.Attribute("text");
                        break;
                    case "UpdateString_PictureTitle":
                        this.UpdateString_PictureTitle = ele.Attribute("text");
                        break;
                    case "SystemRequire_Memory":
                        this.SystemRequire_Memory = ele.Attribute("text");
                        break;
                    case "SystemRequire_Space":
                        this.SystemRequire_Space = ele.Attribute("text");
                        break;
                    case "PlatformSDKString_InitFailedTitle":
                        this.PlatformSDKString_InitFailedTitle = ele.Attribute("text");
                        break;
                    case "PlatformSDKString_ErrorCode":
                        this.PlatformSDKString_ErrorCode = ele.Attribute("text");
                        break;
                    case "PlatformSDKString_UnknownErr":
                        this.PlatformSDKString_UnknownErr = ele.Attribute("text");
                        break;
                    case "PlatformSDKString_NetworkFailure":
                        this.PlatformSDKString_NetworkFailure = ele.Attribute("text");
                        break;
                    case "PlatformSDKString_ServerTimeout":
                        this.PlatformSDKString_ServerTimeout = ele.Attribute("text");
                        break;
                    case "PlatformSDKString_InitializationFailed":
                        this.PlatformSDKString_InitializationFailed = ele.Attribute("text");
                        break;
                    default:
                        break;
                }
            }
            parser.Clear();
            return true;
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("Exception {0}", e.Message));
        }

        return false;
    }
}

public class UpdatePromotionParams       //更新阶段推广的内容列表, 从Resources/UpdatePromotionConfig.xml 读取
{
    public List<string> VideoPaths = new List<string>();
    public List<string> PicturePaths = new List<string>();

    public bool ParseFromXmlString(string content)
    {
        bool ret = false;
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);

            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                SecurityElement ele = (SecurityElement)root.Children[i];
                switch (ele.Tag)
                {
                    case "Video":
                        VideoPaths.Add(ele.Attribute("path"));
                        break;
                    case "Picture":
                        PicturePaths.Add(ele.Attribute("path"));
                        break;
                }
            }
            parser.Clear();
            ret = true;
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("Exception {0}", e.Message));
        }
        return ret;
    }
}

public class DebugSettingParams
{
    public bool SkipUpdate;             //跳过更新
    public bool LocalData;              //忽略更新包中的内容，读本地Data
    public bool LocalLua;                //忽略更新包中的内容，读本地Lua
    public bool ShortCut;               //启用快捷键
    public bool Is1080P;                //开启1080P  
    public bool FullScreen;             //全屏
    public int FPSLimit;                //FPS限制             

    public DebugSettingParams()
    {
        DefaultValue();
    }

    public void DefaultValue()
    {
        // #if UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS
        //         SkipUpdate = true;
        // #else
        SkipUpdate = true;
        //#endif
        LocalData = false;
        LocalLua = false;
        ShortCut = false;
        Is1080P = false;
        FullScreen = false;
        FPSLimit = -1;
    }

    public bool ParseFromXmlString(string content)
    {
        bool bRet = false;
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);

            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                SecurityElement ele = (SecurityElement)root.Children[i];
                switch (ele.Tag)
                {
                    case "SkipUpdate":
                        {
                            bool bVal;
                            if (bool.TryParse(ele.Attribute("value"), out bVal))
                                SkipUpdate = bVal;
                            else
                                SkipUpdate = false;
                        }
                        break;
                    case "LocalData":
                        {
                            bool bVal;
                            if (bool.TryParse(ele.Attribute("value"), out bVal))
                                LocalData = bVal;
                            else
                                LocalData = false;
                        }
                        break;
                    case "LocalLua":
                        {
                            bool bVal;
                            if (bool.TryParse(ele.Attribute("value"), out bVal))
                                LocalLua = bVal;
                            else
                                LocalLua = false;
                        }
                        break;
                    case "ShortCut":
                        {
                            bool bVal;
                            if (bool.TryParse(ele.Attribute("value"), out bVal))
                                ShortCut = bVal;
                            else
                                ShortCut = false;
                        }
                        break;
                    case "Is1080P":
                        {
                            bool bVal;
                            if (bool.TryParse(ele.Attribute("value"), out bVal))
                                Is1080P = bVal;
                            else
                                Is1080P = false;
                        }
                        break;
                    case "FullScreen":
                        {
                            bool bVal;
                            if (bool.TryParse(ele.Attribute("value"), out bVal))
                                FullScreen = bVal;
                            else
                                FullScreen = false;
                        }
                        break;
                    case "FPSLimit":
                        {
                            int iVal;
                            if (int.TryParse(ele.Attribute("value"), out iVal))
                                FPSLimit = iVal;
                            else
                                FPSLimit = -1;
                        }
                        break;
                }
            }
            parser.Clear();
            bRet = true;
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("Exception {0}", e.Message));
        }
        return bRet;
    }
}

public class GameCustomConfigParams
{
    public class CAreaCost
    {
        public float NavMeshGroundCost = 1.5f;
        public float NavMeshGrassCost = 0.8f;
        public float NavMeshRoadCost = 0.05f;
        public float MinDistance = 5f;
    }
    //     public string GCloud_AppID = "1795143106";
    //     public string GCloud_AppKey = "bd68d4aee813b9cba867ce4683b94d2e";
    //     public string LGVC_ServerAddr = "http://lgvc-provisioning-test.line-apps.com";
    //     public string LGVC_AppName = "tera";
    //     public string LGVC_UserKey = "1234";
    public bool EnableLogCache4Debug = true;

    public float PingInterval = 10;
    public int MaxProcessProtocol = 60;
    public int MaxProcessSpecialProtocol = 20;

    public bool ShaderWarmUp = false;
    public bool UseSound = true;
    public bool UseVoice = false;
    public float PowerSavingCount = 180;

    public int UISfxPrefabCacheLifeTime = 20;
    public int GImageModelRTNum = 2;
    public int GImageModelCameraNum = 5;
    public int GImageModelCameraPoolTime = 180;
    public string Authorization = "";
    public CAreaCost DefaultAreaCost = new CAreaCost();
    public Dictionary<string, CAreaCost> DicAreaCost = new Dictionary<string, CAreaCost>();
    public List<string> PreloadBundles = new List<string>();

    public CAreaCost GetAreaCost(string name)
    {
        string key = name.ToLower();

        CAreaCost cost;
        if (DicAreaCost.TryGetValue(key, out cost))
            return cost;
        return null;
    }

    public void DefaultValue()
    {
        //         GCloud_AppID = "1795143106";
        //         GCloud_AppKey = "bd68d4aee813b9cba867ce4683b94d2e";
        //         LGVC_ServerAddr = "http://lgvc-provisioning-test.line-apps.com";
        //         LGVC_AppName = "tera";
        //         LGVC_UserKey = "1234";

        EnableLogCache4Debug = true;
        PingInterval = 10;
        MaxProcessProtocol = 60;
        MaxProcessSpecialProtocol = 20;

        ShaderWarmUp = false;
        UseSound = true;
        UseVoice = true; 
        PowerSavingCount = 180;

        UISfxPrefabCacheLifeTime = 20;
        GImageModelRTNum = 2;
        GImageModelCameraNum = 5;
        //GImageModelRTPoolTime = 180;
        GImageModelCameraPoolTime = 180;

        Authorization = "";

        DefaultAreaCost.NavMeshGroundCost = 1.5f;
        DefaultAreaCost.NavMeshGrassCost = 0.8f;
        DefaultAreaCost.NavMeshRoadCost = 0.05f;
        DefaultAreaCost.MinDistance = 5f;

        DicAreaCost.Clear();
        PreloadBundles.Clear();
    }

    public bool ParseFromXmlString(string content)
    {
        bool bRet = false;
        try
        {
            string text = content; // Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(content));

            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(text);

            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                //uint value = 0;
                int ivalue = 0;
                float fvalue = 0;
                SecurityElement ele = (SecurityElement)root.Children[i];
                switch (ele.Tag)
                {
                    //                     case "GCloud_AppID":
                    //                         this.GCloud_AppID = ele.Attribute("value");
                    //                         break;
                    //                     case "GCloud_AppKey":
                    //                         this.GCloud_AppKey = ele.Attribute("value");
                    //                         break;
                    //                     case "LGVC_ServerAddr":
                    //                         this.LGVC_ServerAddr = ele.Attribute("value");
                    //                         break;
                    //                     case "LGVC_AppName":
                    //                         this.LGVC_AppName = ele.Attribute("value");
                    //                         break;
                    //                     case "LGVC_UserKey":
                    //                         this.LGVC_UserKey = ele.Attribute("value");
                    //                         break;
                    case "EnableLogCache":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            EnableLogCache4Debug = (ivalue == 1);
                        else
                            Common.HobaDebuger.LogWarningFormat("EnableLogCache Parse Error!");
                        break;
                    case "PingInverval":
                        if (float.TryParse(ele.Attribute("value"), out fvalue))
                            this.PingInterval = fvalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("PingInverval Parse Error!");
                        break;
                    case "ShaderWarmUp":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.ShaderWarmUp = ivalue != 0;
                        else
                            Common.HobaDebuger.LogWarningFormat("ShaderWarmUp Parse Error!");
                        break;
                    case "MaxProcessProtocol":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.MaxProcessProtocol = ivalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("MaxProcessProtocol Parse Error!");
                        break;
                    case "MaxProcessSpecialProtocol":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.MaxProcessSpecialProtocol = ivalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("MaxProcessSpecialProtocol Parse Error!");
                        break;
                    case "UseSound":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.UseSound = ivalue != 0;
                        else
                            Common.HobaDebuger.LogWarningFormat("UseSound Parse Error!");
                        break;
                    case "UseVoice":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.UseVoice = ivalue != 0;
                        else
                            Common.HobaDebuger.LogWarningFormat("UseVoice Parse Error!");
                        break;
                    case "PowerSavingCount":
                        if (float.TryParse(ele.Attribute("value"), out fvalue))
                            this.PowerSavingCount = fvalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("PowerSavingCount Parse Error!");
                        break;
                    case "UISfxPrefabCacheLifeTime":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.UISfxPrefabCacheLifeTime = ivalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("UISfxPrefabCacheLifeTime Parse Error!");
                        break;
                    case "GImageModelRTNum":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.GImageModelRTNum = ivalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("GImageModelRTNum Parse Error!");
                        break;
                    case "GImageModelCameraNum":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.GImageModelCameraNum = ivalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("GImageModelCameraNum Parse Error!");
                        break;
                    case "GImageModelCameraPoolTime":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.GImageModelCameraPoolTime = ivalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("GImageModelCameraPoolTime Parse Error!");
                        break;
                    case "PreloadBundles":
                        ParsePreloadBundles(PreloadBundles, ele);
                        break;
                    case "NavMeshAreaCost":
                        ParseNavMeshAreaCost(DicAreaCost, ele);
                        break;
                    case "Authorization":
                        this.Authorization = ele.Attribute("value");
                        break;
                }
            }
            parser.Clear();
            bRet = true;
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("GameCustomConfigParams Read Exception: {0}", e.Message);
            DeviceLogger.Instance.WriteLog(HobaText.Format("GameCustomConfigParams Read Exception: {0}", e.Message));
        }

        var areaCost = GetAreaCost("default");
        if (areaCost != null)
        {
            DefaultAreaCost.NavMeshGroundCost = areaCost.NavMeshGroundCost;
            DefaultAreaCost.NavMeshGrassCost = areaCost.NavMeshGrassCost;
            DefaultAreaCost.NavMeshRoadCost = areaCost.NavMeshRoadCost;
            DefaultAreaCost.MinDistance = areaCost.MinDistance;
        }

        return bRet;
    }

    private void ParsePreloadBundles(List<string> bundleList, SecurityElement root)
    {
        for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
        {
            SecurityElement ele = (SecurityElement)root.Children[i];
            if (ele.Tag != "bundle")
                continue;

            string name = ele.Attribute("name");
            if (bundleList.Contains(name))
                continue;

            bundleList.Add(name);
        }
    }

    private void ParseNavMeshAreaCost(Dictionary<string, CAreaCost> dicAreaCost, SecurityElement root)
    {
        for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
        {
            SecurityElement ele = (SecurityElement)root.Children[i];
            if (ele.Tag != "area")
                continue;

            string name = ele.Attribute("name").ToLower();
            if (dicAreaCost.ContainsKey(name))
                continue;

            CAreaCost areaCost = new CAreaCost();
            float v;
            if (float.TryParse(ele.Attribute("ground"), out v))
                areaCost.NavMeshGroundCost = v;


            if (float.TryParse(ele.Attribute("grass"), out v))
                areaCost.NavMeshGrassCost = v;

            if (float.TryParse(ele.Attribute("road"), out v))
                areaCost.NavMeshRoadCost = v;

            if (float.TryParse(ele.Attribute("mindistance"), out v))
                areaCost.MinDistance = v;

            dicAreaCost.Add(name, areaCost);
        }
    }
}

public class PlayerFollowCameraConfig
{
    public float CamMinOffsetDist;
    public float CamMaxOffsetDist;
    public float CamDefaultOffsetDist;
    public float CamDistSpeedOffset;

    public float DefaultPitchDegDest;
    public float DefaultPitchDegDestOffset;
    public float DefaultYawDegDest;

    public float CamMaxYawSpeed;
    public float CamMinYawSpeed;

    public float CamMaxPitchSpeed;
    public float CamMinPitchSpeed;

    public float CamYawRecoverSpeed;
    public float CamYawRecoverSpeedSlow;
    public float CamPitchRecoverSpeed;

    public float CamDefOffsetDist25D;
    public float CamDefPitchDegDest25D;

    public float CamRollSensitivity;
    public float UIModelRotateSensitivity;

    public float FightLogkLookAtPointRate;
    public float FightLockYawSpeed;
    public float UnLockDistRate;

    public float BossCamPosRecoverDuration;
    public float BossCamLookAtPosRecoverDuration;
    public float BossCamFOVRecoverDuration;

    public void DefaultValue()
    {
        CamMinOffsetDist = 3.0f;
        CamMaxOffsetDist = 20.0f;
        CamDefaultOffsetDist = 6f;

        DefaultPitchDegDest = -14.25f;
        DefaultPitchDegDestOffset = 15f;
        DefaultYawDegDest = 180f;

        CamMaxYawSpeed = 60.0f;
        CamMinYawSpeed = 0.0f;

        CamMaxPitchSpeed = 40.0f;
        CamMinPitchSpeed = 0.0f;

        CamYawRecoverSpeed = 450f;
        CamYawRecoverSpeedSlow = 200f;
        CamPitchRecoverSpeed = 250f;
        CamDistSpeedOffset = 10f;

        CamDefOffsetDist25D = 16f;
        CamDefPitchDegDest25D = -30.0f;

        CamRollSensitivity = 1f;
        UIModelRotateSensitivity = 1f;

        FightLogkLookAtPointRate = 0.3f;
        FightLockYawSpeed = 400f;
        UnLockDistRate = 2f;

        BossCamPosRecoverDuration = 1.5f;
        BossCamLookAtPosRecoverDuration = 1f;
        BossCamFOVRecoverDuration = 0.5f;
    }

    public float GetMaxPitch()
    {
        return DefaultPitchDegDest + DefaultPitchDegDestOffset;
    }
    public float GetMinPitch()
    {
        return DefaultPitchDegDest - DefaultPitchDegDestOffset;
    }
    //相机到主角的最小水平距离
    public float GetCamToPlayerMinDist()
    {
        return CamMinOffsetDist * Mathf.Cos(GetMaxPitch() * Mathf.Deg2Rad);
    }

    public bool ParseFromXmlString(string content)
    {
        bool ret = true;
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);

            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                SecurityElement ele = (SecurityElement)root.Children[i];
                float temp;
                switch (ele.Tag)
                {
                    case "CamMinOffsetDist":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamMinOffsetDist = temp;
                        else
                            WriteErrorLog("CamMinOffsetDist");
                        break;

                    case "CamMaxOffsetDist":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamMaxOffsetDist = temp;
                        else
                            WriteErrorLog("CamMaxOffsetDist");
                        break;

                    case "CamDefaultOffsetDist":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamDefaultOffsetDist = temp;
                        else
                            WriteErrorLog("CamDefaultOffsetDist");
                        break;

                    case "DefaultPitchDegDest":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            DefaultPitchDegDest = temp;
                        else
                            WriteErrorLog("DefaultPitchDegDest");
                        break;

                    case "DefaultPitchDegDestOffset":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            DefaultPitchDegDestOffset = temp;
                        else
                            WriteErrorLog("DefaultPitchDegDestOffset");
                        break;

                    case "DefaultYawDegDest":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            DefaultYawDegDest = temp;
                        else
                            WriteErrorLog("DefaultYawDegDest");
                        break;

                    case "CamMaxYawSpeed":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamMaxYawSpeed = temp;
                        else
                            WriteErrorLog("CamMaxYawSpeed");
                        break;

                    case "CamMinYawSpeed":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamMinYawSpeed = temp;
                        else
                            WriteErrorLog("CamMinYawSpeed");
                        break;

                    case "CamMaxPitchSpeed":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamMaxPitchSpeed = temp;
                        else
                            WriteErrorLog("CamMaxPitchSpeed");
                        break;

                    case "CamMinPitchSpeed":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamMinPitchSpeed = temp;
                        else
                            WriteErrorLog("CamMinPitchSpeed");
                        break;

                    case "CamYawRecoverSpeed":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamYawRecoverSpeed = temp;
                        else
                            WriteErrorLog("CamYawRecoverSpeed");
                        break;

                    case "CamYawRecoverSpeedSlow":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamYawRecoverSpeedSlow = temp;
                        else
                            WriteErrorLog("CamYawRecoverSpeedSlow");
                        break;

                    case "CamPitchRecoverSpeed":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamPitchRecoverSpeed = temp;
                        else
                            WriteErrorLog("CamPitchRecoverSpeed");
                        break;

                    case "CamDistSpeedOffset":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamDistSpeedOffset = temp;
                        else
                            WriteErrorLog("CamDistSpeedOffset");
                        break;

                    case "CamDefOffsetDist25D":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamDefOffsetDist25D = temp;
                        else
                            WriteErrorLog("CamDefOffsetDist25D");
                        break;

                    case "CamDefPitchDegDest25D":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamDefPitchDegDest25D = temp;
                        else
                            WriteErrorLog("CamDefPitchDegDest25D");
                        break;

                    case "CamRollSensitivity":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            CamRollSensitivity = temp;
                        else
                            WriteErrorLog("CamRollSensitivity");
                        break;

                    case "UIModelRotateSensitivity":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            UIModelRotateSensitivity = temp;
                        else
                            WriteErrorLog("UIModelRotateSensitivity");
                        break;
                        
                    case "FightLogkLookAtPointRate":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            FightLogkLookAtPointRate = temp;
                        else
                            WriteErrorLog("FightLogkLookAtPointRate");
                        break;

                    case "FightLockYawSpeed":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            FightLockYawSpeed = temp;
                        else
                            WriteErrorLog("FightLockYawSpeed");
                        break;

                    case "UnLockDistRate":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            UnLockDistRate = temp;
                        else
                            WriteErrorLog("UnLockDistRate");
                        break;

                    case "BossCamPosRecoverDuration":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            BossCamPosRecoverDuration = temp;
                        else
                            WriteErrorLog("BossCamPosRecoverDuration");
                        break;

                    case "BossCamLookAtPosRecoverDuration":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            BossCamLookAtPosRecoverDuration = temp;
                        else
                            WriteErrorLog("BossCamLookAtPosRecoverDuration");
                        break;

                    case "BossCamFOVRecoverDuration":
                        if (float.TryParse(ele.Attribute("value"), out temp))
                            BossCamFOVRecoverDuration = temp;
                        else
                            WriteErrorLog("BossCamFOVRecoverDuration");
                        break;

                    default:
                        ret = false;
                        Common.HobaDebuger.LogErrorFormat("PlayerFollowCameraConfig Unknow Field: {0}", ele.Tag);
                        break;
                }
            }
            parser.Clear();
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("PlayerFollowCameraConfig Read Exception: {0}", e.Message);
        }
        return ret;
    }

    private void WriteErrorLog(string field)
    {
        Common.HobaDebuger.LogErrorFormat("PlayerFollowCameraConfig Parse Exception: {0}", field);
    }
}

public class PlayerNearCameraConfig
{
    public class NearCamStageCfg
    {
        public NearCamStage _Stage;             //阶段
        public float _FOV;                      //视角广度
        public Vector2 _HPDivide;               //高度调节和对应垂直角度的分界值，x为Height, y为Pitch
        public Vector2 _PitchLimit;             //垂直角度限制，x为最小，y为最大
        public Vector2 _DistanceLimit;          //相机距离限制，x为最小，y为最大
        public Vector2 _HeightOffsetLimit;      //相机高度调节限制，x为最小，y为最大

        public NearCamStageCfg(NearCamStage stage)
        {
            _Stage = stage;
        }
    }
    public class NearCamProfCfg
    {
        private int _Prof;
        private bool _IsOnRide;
        private Vector3 _DefaultParams;
        private List<NearCamStageCfg> StageDatas;

        public int Prof { get { return _Prof; } }
        public bool IsOnRide { get { return _IsOnRide; } }
        public Vector3 DefaultParams { get { return _DefaultParams; } }           //默认参数，x为Yaw，y为Pitch，z为Distance

        private NearCamProfCfg(int prof, bool bOnRide)
        {
            _Prof = prof;
            _IsOnRide = bOnRide;
            StageDatas = new List<NearCamStageCfg>(3);
            NearCamStageCfg temp = new NearCamStageCfg(NearCamStage.Body);
            StageDatas.Add(temp);
            temp = new NearCamStageCfg(NearCamStage.HalfBody);
            StageDatas.Add(temp);
            temp = new NearCamStageCfg(NearCamStage.Chest);
            StageDatas.Add(temp);
        }

        public NearCamStageCfg GetCurStageData(NearCamStage stage)
        {
            for (int i = 0; i < StageDatas.Count; i++)
            {
                if (StageDatas[i]._Stage == stage)
                {
                    return StageDatas[i];
                }
            }
            return null;
        }

        public bool SetStageData(NearCamStageCfg data)
        {
            bool ret = false;
            if (data != null)
            {
                for (int i = 0; i < StageDatas.Count; i++)
                {
                    if (StageDatas[i]._Stage == data._Stage)
                    {
                        StageDatas[i] = data;
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        public void SetDefaultParams(float fYaw, float fPitch, float fDist)
        {
            _DefaultParams = new Vector3(fYaw, fPitch, fDist);
        }

        public static NearCamProfCfg GetDefaultProfData(int prof, bool bRdie)
        {
            var prof_data = new NearCamProfCfg(prof, bRdie);
            prof_data.SetDefaultParams(0f, 30f, 2f);
            for (int i = 0; i < prof_data.StageDatas.Count; i++)
            {
                NearCamStageCfg stage_data = prof_data.StageDatas[i];
                switch (stage_data._Stage)
                {
                    case NearCamStage.Body:
                        stage_data._FOV = 60f;
                        stage_data._HPDivide = new Vector2(1.25f, 0f);
                        stage_data._HeightOffsetLimit = new Vector2(1.25f, 1.25f);
                        stage_data._DistanceLimit = new Vector2(1.6f, 2f);
                        stage_data._PitchLimit = new Vector2(-89f, 89f);
                        break;
                    case NearCamStage.HalfBody:
                        stage_data._FOV = 50f;
                        stage_data._HPDivide = new Vector2(1.3f, 0f);
                        stage_data._HeightOffsetLimit = new Vector2(1.3f, 1.3f);
                        stage_data._DistanceLimit = new Vector2(1.2f, 1.6f);
                        stage_data._PitchLimit = new Vector2(-89f, 89f);
                        break;
                    case NearCamStage.Chest:
                        stage_data._FOV = 38f;
                        stage_data._HPDivide = new Vector2(1.66f, 0f);
                        stage_data._HeightOffsetLimit = new Vector2(1.66f, 1.66f);
                        stage_data._DistanceLimit = new Vector2(0.8f, 1.2f);
                        stage_data._PitchLimit = new Vector2(-89f, 89f);
                        break;
                }
                prof_data.StageDatas[i] = stage_data;
            }
            return prof_data;
        }
    }

    private NearCamProfCfg _ProfCfgOnRide;
    private NearCamProfCfg _ProfCfgNotOnRide;

    public void DefaultValue(int prof)
    {
        _ProfCfgOnRide = NearCamProfCfg.GetDefaultProfData(prof, true);
        _ProfCfgNotOnRide = NearCamProfCfg.GetDefaultProfData(prof, false);
    }

    public NearCamProfCfg GetProfCfg(bool bOnRide)
    {
        return bOnRide ? _ProfCfgOnRide : _ProfCfgNotOnRide;
    }

    public bool ParseFromXmlString(string content, int prof)
    {
        bool ret = true;
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);
            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                SecurityElement prof_ele = (SecurityElement)root.Children[i];
                int prof_temp = 0;
                if (int.TryParse(prof_ele.Attribute("Prof"), out prof_temp))
                {
                    if (prof_temp == prof)
                    {
                        bool is_ride_temp = false;
                        if (bool.TryParse(prof_ele.Attribute("IsOnRide"), out is_ride_temp))
                        {
                            NearCamProfCfg prof_cfg_temp = is_ride_temp ? _ProfCfgOnRide : _ProfCfgNotOnRide;
                            foreach (SecurityElement ele in prof_ele.Children)
                            {
                                switch (ele.Tag)
                                {
                                    case "DefaultParams":
                                        float yaw, pitch, dist;
                                        if (!float.TryParse(ele.Attribute("Yaw"), out yaw))
                                        {
                                            yaw = 0f;
                                            WriteErrorLog("DefaultParams", "Yaw");
                                        }

                                        if (!float.TryParse(ele.Attribute("Pitch"), out pitch))
                                        {
                                            pitch = 0f;
                                            WriteErrorLog("DefaultParams", "Pitch");
                                        }

                                        if (!float.TryParse(ele.Attribute("Distance"), out dist))
                                        {
                                            dist = 1.8f;
                                            WriteErrorLog("DefaultParams", "Distance");
                                        }

                                        prof_cfg_temp.SetDefaultParams(yaw, pitch, dist);
                                        break;
                                    case "StageData":
                                        int stage;
                                        if (int.TryParse(ele.Attribute("Stage"), out stage) && Enum.IsDefined(typeof(NearCamStage), stage))
                                        {
                                            NearCamStageCfg stage_data = new NearCamStageCfg((NearCamStage)stage);
                                            float fov;
                                            if (float.TryParse(ele.Attribute("FOV"), out fov))
                                                stage_data._FOV = fov;
                                            else
                                                WriteErrorLog("StageData", "FOV");

                                            foreach (SecurityElement limit in ele.Children)
                                            {
                                                switch (limit.Tag)
                                                {
                                                    case "HPDivide":
                                                        float height_d, pitch_d;
                                                        if (float.TryParse(limit.Attribute("HeightOffset"), out height_d))
                                                            stage_data._HPDivide.x = height_d;
                                                        else
                                                            WriteErrorLog("HPDivide", "HeightOffset");

                                                        if (float.TryParse(limit.Attribute("Pitch"), out pitch_d))
                                                            stage_data._HPDivide.y = pitch_d;
                                                        else
                                                            WriteErrorLog("HPDivide", "Pitch");
                                                        break;
                                                    case "PitchLimit":
                                                        float pitch_min, pitch_max;
                                                        if (float.TryParse(limit.Attribute("Min"), out pitch_min))
                                                            stage_data._PitchLimit.x = pitch_min;
                                                        else
                                                            WriteErrorLog("PitchLimit", "Min");

                                                        if (float.TryParse(limit.Attribute("Max"), out pitch_max))
                                                            stage_data._PitchLimit.y = pitch_max;
                                                        else
                                                            WriteErrorLog("PitchLimit", "Max");
                                                        break;
                                                    case "DistanceLimit":
                                                        float dist_min, dist_max;
                                                        if (float.TryParse(limit.Attribute("Min"), out dist_min))
                                                            stage_data._DistanceLimit.x = dist_min;
                                                        else
                                                            WriteErrorLog("DistanceLimit", "Min");

                                                        if (float.TryParse(limit.Attribute("Max"), out dist_max))
                                                            stage_data._DistanceLimit.y = dist_max;
                                                        else
                                                            WriteErrorLog("DistanceLimit", "Max");
                                                        break;
                                                    case "HeightOffsetLimit":
                                                        float heigh_min, height_max;
                                                        if (float.TryParse(limit.Attribute("Min"), out heigh_min))
                                                            stage_data._HeightOffsetLimit.x = heigh_min;
                                                        else
                                                            WriteErrorLog("HeightOffsetLimit", "Min");

                                                        if (float.TryParse(limit.Attribute("Max"), out height_max))
                                                            stage_data._HeightOffsetLimit.y = height_max;
                                                        else
                                                            WriteErrorLog("HeightOffsetLimit", "Max");
                                                        break;

                                                    default:
                                                        WriteUnknowLog(ele.Tag, limit.Tag);
                                                        break;
                                                }
                                            }

                                            prof_cfg_temp.SetStageData(stage_data);
                                            break;
                                        }
                                        else
                                            WriteErrorLog(ele.Tag, "Stage");
                                        break;

                                    default:
                                        WriteUnknowLog(prof_ele.Tag, ele.Tag);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            ret = false;
                            WriteErrorLog(prof_ele.Tag, "IsOnRide");
                        }
                    }
                }
                else
                {
                    ret = false;
                    WriteErrorLog(prof_ele.Tag, "Prof");
                }
            }

            parser.Clear();
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("PlayerNearCameraConfig Read Exception: {0}", e.Message);
            ret = false;
        }
        return ret;
    }

    private void WriteErrorLog(string element_name, string attribute)
    {
        Common.HobaDebuger.LogErrorFormat("PlayerNearCameraConfig {0} Parse Attribute Exception: {1}", element_name, attribute);
    }

    private void WriteUnknowLog(string root_name, string element_name)
    {
        Common.HobaDebuger.LogErrorFormat("PlayerNearCameraConfig {1} got unknow field: {0} ", root_name, element_name);
    }
}

public class CBankConfigEntry
{
    public string Name = string.Empty;
    public string NavmeshName = string.Empty;
    public bool Localized = false;
}

public class WwiseBankConfigParams
{
    public List<CBankConfigEntry> MapBankList = new List<CBankConfigEntry>();
    public List<CBankConfigEntry> NonMapBankList = new List<CBankConfigEntry>();
    public List<CBankConfigEntry> AlwaysLoadingBankList = new List<CBankConfigEntry>();

    public void DefaultValue()
    {
        MapBankList.Clear();
        NonMapBankList.Clear();
        AlwaysLoadingBankList.Clear();
    }

    public bool ParseFromXmlString(string content)
    {
        bool bRet = false;
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);

            AlwaysLoadingBankList.Clear();
            MapBankList.Clear();
            NonMapBankList.Clear();

            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                SecurityElement ele = (SecurityElement)root.Children[i];
                switch(ele.Tag)
                {
                    case "AlwaysLoad":
                        ParseListBanks(AlwaysLoadingBankList, ele);
                        break;
                    case "NonMap":
                        ParseListBanks(NonMapBankList, ele);
                        break;
                    case "Map":
                        ParseListBanks(MapBankList, ele);
                        break;
                }
            }
            parser.Clear();
            bRet = true;
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("WwiseBankConfig Xml Parse Exception! {0}", e.Message);
        }
        return bRet;
    }

    private void ParseListBanks(List<CBankConfigEntry> list, SecurityElement root)
    {
        list.Clear();
        for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
        {
            SecurityElement ele = (SecurityElement)root.Children[i];
            if (ele.Tag != "bank")
                continue;

            string bankname = ele.Attribute("name");
            string navmeshname = ele.Attribute("navmesh");
            int localize = 0;
            Int32.TryParse(ele.Attribute("localize"), out localize);

            list.Add(new CBankConfigEntry() { NavmeshName = navmeshname, Name = bankname, Localized = (localize != 0) });
        }
    }
}

public enum WeatherAudioType
{
    None = 0,
    Day,
    Night,
    Rain,
    Snow,
}

public class WwiseSoundConfigParams
{
    public int WwiseDefaultPoolSize = 4096;
    public int WwiseLowerPoolSize = 2048;
    public int WwiseStreamingPoolSize = 1024;
    public uint WwiseMaxNumPools = 20;
    public uint WwiseNumSamplePerFrame = 512;

    public uint WwiseBGMVolume = 341651998u;
    public uint WwiseSFXVolume = 1564184899u;
    public uint WwiseCutsceneVolume = 1766603218u;
    public uint WwiseUIVolume = 1719345792u;

    public uint WwiseBGEffect = 2706344916u;
    public string WwiseBGEffectSound = "Tuto_BG_Effect";

    Dictionary<int, string> Dic_WeatherAudioMap = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_Male = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_Female = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_Hoofed = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_Dragon = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_Felidae = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_Dunk = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_Pig = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_Lion = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_Wolf = new Dictionary<int, string>();
    Dictionary<int, string> Dic_FootStepAudioMap_BDragon = new Dictionary<int, string>();

    public void DefaultValue()
    {
        WwiseDefaultPoolSize = 4096;
        WwiseLowerPoolSize = 2048;
        WwiseStreamingPoolSize = 1024;
        WwiseMaxNumPools = 20;
        WwiseNumSamplePerFrame = 512;
        WwiseBGMVolume = 341651998u;
        WwiseSFXVolume = 1564184899u;
        WwiseCutsceneVolume = 1766603218u;
        WwiseUIVolume = 1719345792u;
        WwiseBGEffect = 2706344916u;
        WwiseBGEffectSound = "Tuto_BG_Effect";

        Dic_WeatherAudioMap.Clear();
        Dic_FootStepAudioMap_Male.Clear();
        Dic_FootStepAudioMap_Female.Clear();
        Dic_FootStepAudioMap_Hoofed.Clear();
        Dic_FootStepAudioMap_Dragon.Clear();
        Dic_FootStepAudioMap_Felidae.Clear();
        Dic_FootStepAudioMap_Dunk.Clear();
        Dic_FootStepAudioMap_Pig.Clear();
        Dic_FootStepAudioMap_Lion.Clear();
        Dic_FootStepAudioMap_Wolf.Clear();
        Dic_FootStepAudioMap_BDragon.Clear();
    }

    public string GetWeatherAudio(WeatherAudioType type)
    {
        string val;
        if (Dic_WeatherAudioMap.TryGetValue((int)type, out val))
            return val;
        return string.Empty;
    }

    public string GetMaleFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Male.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public string GetFemaleFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Female.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public string GetHoofedFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Hoofed.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public string GetDragonFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Dragon.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public string GetFelidaeFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Felidae.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public string GetdunkFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Dunk.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public string GetpigFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Pig.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public string GetlionFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Lion.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public string GetwolfFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Wolf.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public string GetBDragonFootStepAudio(int type)
    {
        string val;
        if (!Dic_FootStepAudioMap_Wolf.TryGetValue(type, out val))
            return string.Empty;
        return val;
    }

    public bool ParseFromXmlString(string content)
    {
        bool bRet = false;
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);

            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                uint value = 0;
                int ivalue = 0;
                SecurityElement ele = (SecurityElement)root.Children[i];                
                switch(ele.Tag)
                {
                    case "WeatherSound":
                        ParseWeatherSound(Dic_WeatherAudioMap, ele);
                        break;
                    case "FootStepSound":
                        ParseFootStepSound("Male", Dic_FootStepAudioMap_Male, ele);
                        ParseFootStepSound("Female", Dic_FootStepAudioMap_Female, ele);
                        ParseFootStepSound("Hoofed", Dic_FootStepAudioMap_Hoofed, ele);
                        ParseFootStepSound("Dragon", Dic_FootStepAudioMap_Dragon, ele);
                        ParseFootStepSound("Felidae", Dic_FootStepAudioMap_Felidae, ele);
                        ParseFootStepSound("Dunk", Dic_FootStepAudioMap_Dunk, ele);
                        ParseFootStepSound("Pig", Dic_FootStepAudioMap_Pig, ele);
                        ParseFootStepSound("Lion", Dic_FootStepAudioMap_Lion, ele);
                        ParseFootStepSound("Wolf", Dic_FootStepAudioMap_Wolf, ele);
                        ParseFootStepSound("BDragon", Dic_FootStepAudioMap_BDragon, ele);
                        break;
                    case "WwiseBGMVolume":
                        if (uint.TryParse(ele.Attribute("value"), out value))
                            this.WwiseBGMVolume = value;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseBGMVolume Parse Error!");
                        break;
                    case "WwiseSFXVolume":
                        if (uint.TryParse(ele.Attribute("value"), out value))
                            this.WwiseSFXVolume = value;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseSFXVolume Parse Error!");
                        break;
                    case "WwiseCutsceneVolume":
                        if (uint.TryParse(ele.Attribute("value"), out value))
                            this.WwiseCutsceneVolume = value;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseCutsceneVolume Parse Error!");
                        break;
                    case "WwiseUIVolume":
                        if (uint.TryParse(ele.Attribute("value"), out value))
                            this.WwiseUIVolume = value;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseUIVolume Parse Error!");
                        break;
                    case "WwiseBGEffect":
                        if (uint.TryParse(ele.Attribute("value"), out value))
                            this.WwiseBGEffect = value;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseBGEffect Parse Error!");
                        break;
                    case "WwiseDefaultPoolSize":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.WwiseDefaultPoolSize = ivalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseDefaultPoolSize Parse Error!");
                        break;
                    case "WwiseLowerPoolSize":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.WwiseLowerPoolSize = ivalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseLowerPoolSize Parse Error!");
                        break;
                    case "WwiseStreamingPoolSize":
                        if (int.TryParse(ele.Attribute("value"), out ivalue))
                            this.WwiseStreamingPoolSize = ivalue;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseStreamingPoolSize Parse Error!");
                        break;
                    case "WwiseMaxNumPools":
                        if (uint.TryParse(ele.Attribute("value"), out value))
                            this.WwiseMaxNumPools = value;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseMaxNumPools Parse Error!");
                        break;
                    case "WwiseNumSamplePerFrame":
                        if (uint.TryParse(ele.Attribute("value"), out value))
                            this.WwiseNumSamplePerFrame = value;
                        else
                            Common.HobaDebuger.LogWarningFormat("WwiseNumSamplePerFrame Parse Error!");
                        break;
                    case "WwiseBGEffectSound":
                        this.WwiseBGEffectSound = ele.Attribute("value");
                        break;
                    default:
                        break;
                }
            }
            parser.Clear();
            bRet = true;
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("WwiseSoundConfig Xml Parse Exception! {0}", e.Message);
        }
        return bRet;
    }

    private void ParseSoundMap(Dictionary<string, string> audioMap, SecurityElement root)
    {
        for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
        {
            SecurityElement ele = (SecurityElement)root.Children[i];
            if (ele.Tag != "item")
                continue;

            string audio = ele.Attribute("audio");
            string wwise = ele.Attribute("wwise");

            if (string.IsNullOrEmpty(audio) || string.IsNullOrEmpty(wwise))
                continue;

            audio = audio.ToLower();
            if (audioMap.ContainsKey(audio))
                continue;

            audioMap.Add(audio, wwise);
        }
    }

    private void ParseWeatherSound(Dictionary<int, string> audioMap, SecurityElement root)
    {
        for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
        {
            SecurityElement ele = (SecurityElement)root.Children[i];
            if (ele.Tag != "item")
                continue;

            string audio = ele.Attribute("type");
            string wwise = ele.Attribute("wwise");

            int type = 0;
            if (!int.TryParse(audio, out type) || string.IsNullOrEmpty(wwise))
                continue;

            if (audioMap.ContainsKey(type))
                continue;

            audioMap.Add(type, wwise);
        }
    }

    private void ParseFootStepSound(string key, Dictionary<int, string> audioMap, SecurityElement root)
    {
        for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
        {
            SecurityElement ele = (SecurityElement)root.Children[i];
            if (ele.Tag != key)
                continue;

            string audio = ele.Attribute("type");
            string wwise = ele.Attribute("wwise");

            int type = 0;
            if (!int.TryParse(audio, out type) || string.IsNullOrEmpty(wwise))
                continue;

            if (!audioMap.ContainsKey(type))
                audioMap.Add(type, wwise);
        }
    }
}

public class WeatherTimeConifg
{
    public class WeatherTimeData
    {
        public int MorningLastTime = 300;
        public int MorningLerpTime = 60;
        public int DayLastTime = 600;
        public int DayLerpTime = 60;
        public int DustLastTime = 300;
        public int DustLerpTime = 60;
        public int NightLastTime = 480;
        public int NightLerpTime = 60;
        public int RegionLerpTime = 10;
    }

    public WeatherTimeData TimeData = new WeatherTimeData();

    public bool ParseFromXmlString(string content)
    {
        bool bRet = false;
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);
            SecurityElement root = parser.ToXml();
            for (int i = 0; i < (root.Children != null ? root.Children.Count : 0); ++i)
            {
                SecurityElement ele = (SecurityElement)root.Children[i];
                switch (ele.Tag)
                {
                    case "WeatherTimeData":
                        {
                            int temp = 0;
                            TimeData.MorningLastTime = 120;
                            TimeData.MorningLerpTime = 120;
                            TimeData.DayLastTime = 120;
                            TimeData.DayLerpTime = 120;
                            TimeData.DustLastTime = 120;
                            TimeData.DustLerpTime = 120;
                            TimeData.NightLastTime = 120;
                            TimeData.NightLerpTime = 120;
                            TimeData.MorningLastTime = 120;
                            if (!int.TryParse(ele.Attribute("MorningLastTime"), out temp))
                            {
                                WriteErrorLog("WeatherTimeData", "MorningLastTime");
                            }
                            else
                            {
                                TimeData.MorningLastTime = temp;
                            }


                            if (!int.TryParse(ele.Attribute("MorningLerpTime"), out temp))
                            {
                                WriteErrorLog("WeatherTimeData", "MorningLerpTime");
                            }
                            else
                            {
                                TimeData.MorningLerpTime = temp;
                            }


                            if (!int.TryParse(ele.Attribute("DayLastTime"), out temp))
                            {
                                WriteErrorLog("WeatherTimeData", "DayLastTime");
                            }
                            else
                            {
                                TimeData.DayLastTime = temp;
                            }

                            if (!int.TryParse(ele.Attribute("DayLerpTime"), out temp))
                            {
                                WriteErrorLog("WeatherTimeData", "DayLerpTime");
                            }
                            else
                            {
                                TimeData.DayLerpTime = temp;
                            }

                            if (!int.TryParse(ele.Attribute("DustLastTime"), out temp))
                            {
                                WriteErrorLog("WeatherTimeData", "DustLastTime");
                            }
                            else
                            {
                                TimeData.DustLastTime = temp;
                            }

                            if (!int.TryParse(ele.Attribute("DustLerpTime"), out temp))
                            {
                                WriteErrorLog("WeatherTimeData", "DustLerpTime");
                            }
                            else
                            {
                                TimeData.DustLerpTime = temp;
                            }
                            if (!int.TryParse(ele.Attribute("NightLastTime"), out temp))
                            {
                                WriteErrorLog("WeatherTimeData", "NightLastTime");
                            }
                            else
                            {
                                TimeData.NightLastTime = temp;
                            }
                            if (!int.TryParse(ele.Attribute("NigthLerpTime"), out temp))
                            {
                                WriteErrorLog("WeatherTimeData", "NigthLerpTime");
                            }
                            else
                            {
                                TimeData.NightLerpTime = temp;
                            }
                            if (!int.TryParse(ele.Attribute("RegionLerpTime"), out temp))
                            {
                                WriteErrorLog("WeatherTimeData", "RegionLerpTime");
                            }
                            else
                            {
                                TimeData.RegionLerpTime = temp;
                            }
                        }
                        break;
                    default:
                        {
                            WriteUnknownLog("WeatherTimeData", ele.Tag);
                        }
                        break;
                }
            }
            parser.Clear();
            bRet = true;
        }
        catch (Exception e)
        {
            WriteErrorLog("WeatherTimeConifg", e.Message);
        }
        return bRet;
    }

    private void WriteErrorLog(string method, string field)
    {
        Common.HobaDebuger.LogErrorFormat("{0} Parse Exception: {1}", method, field);
        //AndroidLogger.Instance.WriteToSDCard(HobaString.Format("{0} Parse Exception: {1}", method, field));
    }

    private void WriteUnknownLog(string method, string field)
    {
        Common.HobaDebuger.LogErrorFormat("{0} Parse Unknown Field: {1}", method, field);
        //AndroidLogger.Instance.WriteToSDCard(HobaString.Format("{0} Parse Unknown Field: {1}", method, field));
    }
}

public class WeatherSystemConfig
{
    public List<WeatherData> WeatherDataList = new List<WeatherData>();

    public bool ParseFromXmlString(string content)
    {
        bool bRet = false;
        try
        {
            SecurityParser parser = Main.XMLParser;
            parser.LoadXml(content);

            SecurityElement root = parser.ToXml();
            ArrayList ChildrenList = root.Children;
            for (int i = 0; i < ChildrenList.Count; ++i)
            {
                SecurityElement ele = (SecurityElement)ChildrenList[i];
                if (ele.Tag == "WeatherData")
                {
                    WeatherData data = new WeatherData();
                    ParseFromWeatherData(ref data, ele);
                    WeatherDataList.Add(data);
                }
            }
            parser.Clear();
            bRet = true;
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("Exception was raised, {0}", e);
        }
        return bRet;
    }

    private void ParseFromWeatherData(ref WeatherData data, SecurityElement weatherDataElement)
    {
        int guid = 0;
        if (!Int32.TryParse(weatherDataElement.Attribute("GUID"), out guid))
            return;

        data._DataGuid = guid;
        for (int i = 0; i < (weatherDataElement.Children != null ? weatherDataElement.Children.Count : 0); ++i)
        {
            SecurityElement ele = (SecurityElement)weatherDataElement.Children[i];
            switch (ele.Tag)
            {
                case "SunConfig":
                    ParseFromSunConfig(ref data._SunConfig, ele);
                    break;
                case "GlobalLightConfig":
                    ParseFromGlobaLightConfig(ref data._GlobalLightConfig, ele);
                    break;
                case "SkyBoxPath":
                    data._SkyBoxMatPath = ele.Attribute("SkyBoxPath"); ;
                    break;
                case "SfxPath":
                    data._SfxPath = ele.Attribute("SfxPath");
                    break;
                case "WwiseEvent":
                    data.WwiseEvent = ele.Attribute("WwiseEvent");
                    break;
                case "SkyBoxPathEx":
                    data._SkyBoxMatPathEX = ele.Attribute("SkyBoxPathEx");
                    break;
                case "PlayerLightConfig":
                    ParseFromPlayerLightConfig(ref data._PlayerLightConfig, ele);
                    break;
                case "EnvConifg":
                    ParseEnvironmentConfig(ref data._EnvironmentConfig, ele);
                    break;
                case "FogConfig":
                    ParseFromFogConfig(ref data._FogConfig, ele);
                    break;
                case "PostEffectConfig":
                    ParseFromPostPostEffectConfig(ref data._PostEffectConfig, ele);
                    break;
                case "ShadowConfig":
                    {
                        float shadowConfigNum = 0;
                        if (float.TryParse(ele.Attribute("ShadowConfig"), out shadowConfigNum))
                            data._ShadowConfig = shadowConfigNum;
                    }
                    break;
                case "SkyConfig":
                    ParseFromSkyConfig(ref data._SkyConifg, ele);
                    break;
                case "ShadowsMidtonesHighlightsConfig":
                    ParsefromHighLight(ref data._HightLightConfig, ele);
                    break;
                case "BrightnessContrastGammaConfig":
                    ParsefromGramma(ref data._GammaConfig, ele);
                    break;
            }
        }
    }

    private void ParsefromHighLight(ref WeatherData.ShadowsMidtonesHighlightsConfig higtLightConfig, SecurityElement elemt)
    {
        Color shadows = new Color(1f, 1f, 1f, 0.5f);
        Color midtones = new Color(1f, 1f, 1f, 0.5f);
        Color highlights = new Color(1f, 1f, 1f, 0.5f);
        float amount = 1f;

        ColorUtility.TryParseHtmlString(elemt.Attribute("Shadows"), out shadows);
        higtLightConfig.Shadows = shadows;

        ColorUtility.TryParseHtmlString(elemt.Attribute("Midtones"), out midtones);
        higtLightConfig.Midtones = midtones;

        ColorUtility.TryParseHtmlString(elemt.Attribute("Highlights"), out highlights);
        higtLightConfig.Highlights = highlights;

        float.TryParse(elemt.Attribute("Amount"), out amount);
        higtLightConfig.Amount = amount;
    }

    private void ParsefromGramma(ref WeatherData.BrightnessContrastGammaConfig gammaConfig, SecurityElement gammaElemt)
    {
        float brightness = 0f;
        float contrast = 1f;

        float.TryParse(gammaElemt.Attribute("Brightness"), out brightness);
        gammaConfig.Brightness = brightness;

        float.TryParse(gammaElemt.Attribute("Contrast"), out contrast);
        gammaConfig.Contrast = contrast;
    }

    private void ParseFromSkyConfig(ref WeatherData.SkyConfig skyConfig, SecurityElement skyConfigElement)
    {
        int cubeMapBias = 0;
        float reflectionIntensity = 0;
        float headlightIntesity = 0;
        Color refColor = Color.white;
        string cubeMapPath = skyConfigElement.Attribute("CubeMapPath");
        skyConfig._CubeMapPath = "";
        if (null != cubeMapPath)
            skyConfig._CubeMapPath = cubeMapPath;
        float w = 0;
        float x = 0;
        float y = 0;
        float z = 0;

        string envCubeMapPath = "" ;
        envCubeMapPath = skyConfigElement.Attribute("EnvCubeMapPath");
        if (null != envCubeMapPath)
            skyConfig._EnvCubeMapPath = envCubeMapPath;
        float windFrequency = 0;
        float windNoise = 0;
        float lightmapFeedback = 0;

        int.TryParse(skyConfigElement.Attribute("CubeMapBias"), out cubeMapBias);
        skyConfig._CubeMapBias = cubeMapBias;

        float.TryParse(skyConfigElement.Attribute("ReflectionIntensity"), out reflectionIntensity);
        skyConfig._ReflectionIntensity = reflectionIntensity;

        float.TryParse(skyConfigElement.Attribute("HeadlightIntesity"), out headlightIntesity);
        skyConfig._HeadlightIntesity = headlightIntesity;

        ColorUtility.TryParseHtmlString(skyConfigElement.Attribute("RefColor"), out refColor);
        skyConfig._RefColor = refColor;

        float.TryParse(skyConfigElement.Attribute("WindDirectionW"), out w);
        float.TryParse(skyConfigElement.Attribute("WindDirectionX"), out x);
        float.TryParse(skyConfigElement.Attribute("WindDirectionY"), out y);
        float.TryParse(skyConfigElement.Attribute("WindDirectionZ"), out z);
        skyConfig._WindDirection = new Vector4(x, y, z, w);

        x = 0; y = 0; z = 0; w = 0;
        float.TryParse(skyConfigElement.Attribute("AddonDirectionW"), out w);
        float.TryParse(skyConfigElement.Attribute("AddonDirectionX"), out x);
        float.TryParse(skyConfigElement.Attribute("AddonDirectionY"), out y);
        float.TryParse(skyConfigElement.Attribute("AddonDirectionZ"), out z);
        skyConfig._AddonDirection = new Vector4(x, y, z, w);

        float.TryParse(skyConfigElement.Attribute("WindFrequency"), out windFrequency);
        skyConfig._WindFrequency = windFrequency;
        float.TryParse(skyConfigElement.Attribute("WindNoise"), out windNoise);
        skyConfig._WindNoise = windNoise;

        float.TryParse(skyConfigElement.Attribute("LightmapFeedback"), out lightmapFeedback);
        skyConfig._LightmapFeedback = lightmapFeedback;
    }

    private void ParseFromPostPostEffectConfig(ref WeatherData.PostEffectConfig postEffectChain, SecurityElement postEffectChainElement)
    {
        for (int i = 0; i < (postEffectChainElement.Children != null ? postEffectChainElement.Children.Count : 0); ++i)
        {
            SecurityElement ele = (SecurityElement)postEffectChainElement.Children[i];
            switch (ele.Tag)
            {
                case "Fog":
                        ParseFromPostEffectFog(ref postEffectChain._Fog, ele);
                    break;

                case "DOF":
                        ParseFromPostEffectDOF(ref postEffectChain._DOF, ele);
                    break;
                case "HSV":
                        ParseFromPostEffectHSV(ref postEffectChain._HSV, ele);
                    break;
                case "BrightnessAndContrast":
                        ParseFromPostEffectBrightnessAndContrast(ref postEffectChain._BrightnessAndContrast, ele);
                    break;
                case "Bloom":
                    {
                        float threshold = 1.2f;
                        float intensity = 1f;
                        float radius = 0.5f;
                        int iteration = 8;
                        float softKneeA = 0;
                        bool withFlicker = false;

                        Boolean.TryParse(ele.Attribute("WithFlicker"), out withFlicker);
                        postEffectChain.WithFlicker = withFlicker;

                        float.TryParse(ele.Attribute("Threshold"), out threshold);
                        postEffectChain.Threshold = threshold;

                        float.TryParse(ele.Attribute("Intensity"), out intensity);
                        postEffectChain.Intensity = intensity;

                        float.TryParse(ele.Attribute("Radius"), out radius);
                        postEffectChain.Radius = radius;

                        int.TryParse(ele.Attribute("Iteration"), out iteration);
                        postEffectChain.Iteration = iteration;

                        float.TryParse(ele.Attribute("SoftKneeA"), out softKneeA);
                        postEffectChain.SoftKneeA = softKneeA;
                    }
                    break;
                case "SpecialVision":
                        ParseFromPostEffectSpecialVision(ref postEffectChain._SpecialVision, ele);
                    break;
            }
        }
    }

    private void ParseFromPostEffectFog(ref WeatherData.PostEffectConfig.Fog fog, SecurityElement ele)
    {
        bool enabled = false;
        Color fogColor = Color.white;
        float parameter0 = 0.0f;
        float parameter1 = 0.0f;
        float parameter2 = 0.0f;
        float parameter3 = 0.0f;

        Boolean.TryParse(ele.Attribute("FogEnabled"), out enabled);
        fog._FogEnabled = enabled;

        ColorUtility.TryParseHtmlString(ele.Attribute("FogColor"), out fogColor);
        fog._FogColor = fogColor;

        float.TryParse(ele.Attribute("FogParamterX"), out parameter0);
        fog._FogParamter0 = parameter0;

        float.TryParse(ele.Attribute("FogParamterY"), out parameter1);
        fog._FogParamter1 = parameter1;

        float.TryParse(ele.Attribute("FogParamterZ"), out parameter2);
        fog._FogParamter2 = parameter2;

        float.TryParse(ele.Attribute("FogParamterW"), out parameter3);
        fog._FogParamter3 = parameter3;
    }

    private void ParseFromPostEffectDOF(ref WeatherData.PostEffectConfig.DOF dof, SecurityElement ele)
    {
        bool enabled = false;
        float parameter0 = 0.0f;
        float parameter1 = 0.0f;
        float parameter2 = 0.0f;

        Boolean.TryParse(ele.Attribute("DOFEnabled"), out enabled);
        dof._DOFEnabled = enabled;

        float.TryParse(ele.Attribute("DOFParamterX"), out parameter0);
        dof.DOFParamter0 = parameter0;

        float.TryParse(ele.Attribute("DOFParamterY"), out parameter1);
        dof.DOFParamter1 = parameter1;

        float.TryParse(ele.Attribute("DOFParamterZ"), out parameter2);
        dof.DOFParamter2 = parameter2;
    }

    private void ParseFromPostEffectHSV(ref WeatherData.PostEffectConfig.HSV hsv, SecurityElement ele)
    {
        bool enabled = false;
        float parameter0 = 0.0f;
        float parameter1 = 0.0f;
        float parameter2 = 0.0f;

        Boolean.TryParse(ele.Attribute("HSVEnabled"), out enabled);
        hsv._HSVEnabled = enabled;

        float.TryParse(ele.Attribute("HSVParamterX"), out parameter0);
        hsv.HsvParamter0 = parameter0;

        float.TryParse(ele.Attribute("HSVParamterY"), out parameter1);
        hsv.HsvParamter1 = parameter1;

        float.TryParse(ele.Attribute("HSVParamterZ"), out parameter2);
        hsv.HsvParamter2 = parameter2;
    }

    private void ParseFromPostEffectBrightnessAndContrast(ref WeatherData.PostEffectConfig.BrightnessAndContrast brightnessAndContrast, SecurityElement ele)
    {
        bool enabled = false;
        float parameter0 = 0.0f;
        float parameter1 = 0.0f;
        float parameter2 = 0.0f;

        Boolean.TryParse(ele.Attribute("BrightnessAndConstrastEnabled"), out enabled);
        brightnessAndContrast.BrightnessAndConstrastEnabled = enabled;

        float.TryParse(ele.Attribute("BrightnessAndConstrastParamterX"), out parameter0);
        brightnessAndContrast.BrightnessAndConstrastParamter0 = parameter0;

        float.TryParse(ele.Attribute("BrightnessAndConstrastParamterY"), out parameter1);
        brightnessAndContrast.BrightnessAndConstrastParamter1 = parameter1;

        float.TryParse(ele.Attribute("BrightnessAndConstrastParamterZ"), out parameter2);
        brightnessAndContrast.BrightnessAndConstrastParamter2 = parameter2;
    }

//     private void ParseFromPostEffectBloom(ref WeatherData.PostEffectConfig.Bloom bloom, SecurityElement ele)
//     {
//         bool enabled = false;
//         float parameter0 = 0.0f;
//         float parameter1 = 0.0f;
// 
//         if (!Boolean.TryParse(ele.Attribute("BloomEnabled"), out enabled))
//             WriteErrorLog("ParseFromPostEffectBloom", "Enabled");
//         bloom.BloomEnabled = enabled;
// 
//         if (!float.TryParse(ele.Attribute("BloomParamterX"), out parameter0))
//             WriteErrorLog("ParseFromPostEffectBloom", "Parameter0");
//         bloom.BloomParamter0 = parameter0;
// 
//         if (!float.TryParse(ele.Attribute("BloomParamterY"), out parameter1))
//             WriteErrorLog("ParseFromPostEffectBloom", "Parameter1");
//         bloom.BloomParamter1 = parameter1;
//     }

    private void ParseFromPostEffectSpecialVision(ref WeatherData.PostEffectConfig.SpecialVision specialVision, SecurityElement ele)
    {
        bool enabled = false;
        Color specialColor = Color.white;
        float parameter0 = 0.0f;
        float parameter1 = 0.0f;

        Boolean.TryParse(ele.Attribute("SpecialVisionEnabled"), out enabled);
        specialVision.SpecialVisionEnabled = enabled;

        ColorUtility.TryParseHtmlString(ele.Attribute("SpecialVisionColor"), out specialColor);
        specialVision.SpecialVisionColor = specialColor;

        float.TryParse(ele.Attribute("SpecialVisionParamterX"), out parameter0);
        specialVision.SpecialVisionParamter0 = parameter0;

        float.TryParse(ele.Attribute("SpecialVisionParamterY"), out parameter1);
        specialVision.SpecialVisionParamter1 = parameter1;
    }

    private void ParseFromFogConfig(ref WeatherData.FogConfig fogConfig, SecurityElement fogElemt)
    {
        Color fogColor = Color.white;
        bool isFogOn = false;
        float fogBeginDis = 0;
        float fogEndDis = 0;
        int fogModeNum = 0;

        ColorUtility.TryParseHtmlString(fogElemt.Attribute("FogColor"), out fogColor);
        fogConfig._FogColor = fogColor;

        bool.TryParse(fogElemt.Attribute("IsFogOn"), out isFogOn);
        fogConfig._IsFogOn = isFogOn;

        int.TryParse(fogElemt.Attribute("FogMode"), out fogModeNum);
        fogConfig._FogMode = (FogMode)fogModeNum;

        float.TryParse(fogElemt.Attribute("FogBeginDis"), out fogBeginDis);
        fogConfig._FogBeginDis = fogBeginDis;

        float.TryParse(fogElemt.Attribute("FogEndDis"), out fogEndDis);
        fogConfig._FogEndDis = fogEndDis;
    }
    private void ParseEnvironmentConfig(ref WeatherData.EnvironmentConfig envConfig, SecurityElement envElemet)
    {
        Color equatorColor = Color.white;
        Color groundColor = Color.white;
        Color skyColor = Color.white;
        float ambientIntensity = 0;

        ColorUtility.TryParseHtmlString(envElemet.Attribute("EquatorColor"), out equatorColor);
        envConfig._EquatorColor = equatorColor;

        ColorUtility.TryParseHtmlString(envElemet.Attribute("SkyColor"), out skyColor);
        envConfig._SkyColor = skyColor;

        ColorUtility.TryParseHtmlString(envElemet.Attribute("GroundColor"), out groundColor);
        envConfig._GroundColor = groundColor;

        float.TryParse(envElemet.Attribute("AmbientIntensity"), out ambientIntensity);
        envConfig._AmbientIntensity = ambientIntensity;
    }

    private void ParseFromPlayerLightConfig(ref WeatherData.LightConfig playerLightConfig, SecurityElement playerLightElement)
    {
        Color lightColor = Color.white;
        float intensity = 0;
        float bounceIntensity = 0;
        float rotationX = 0;
        float rotationY = 0;
        float rotationZ = 0;
        float strength = 0;
        float bias = 0;
        float normalBias = 0;
        float shadowNearPlane = 0;

        ColorUtility.TryParseHtmlString(playerLightElement.Attribute("LightColor"), out lightColor);
        playerLightConfig._LightColor = lightColor;

        float.TryParse(playerLightElement.Attribute("Intensity"), out intensity);
        playerLightConfig._Intensity = intensity;

        float.TryParse(playerLightElement.Attribute("BounceIntensity"), out bounceIntensity);
        playerLightConfig._BounceIntensity = bounceIntensity;

        float.TryParse(playerLightElement.Attribute("RotationX"), out rotationX);
        playerLightConfig._RotationX = rotationX;

        float.TryParse(playerLightElement.Attribute("RotationY"), out rotationY);
        playerLightConfig._RotationY = rotationY;

        float.TryParse(playerLightElement.Attribute("RotationZ"), out rotationZ);
        playerLightConfig._RotationZ = rotationZ;

        float.TryParse(playerLightElement.Attribute("Strength"), out strength);
        playerLightConfig._Strength = strength;

        float.TryParse(playerLightElement.Attribute("Bias"), out bias);
        playerLightConfig._Bias = bias;

        float.TryParse(playerLightElement.Attribute("NormalBias"), out normalBias);
        playerLightConfig._NormalBias = normalBias;

        float.TryParse(playerLightElement.Attribute("ShadowNearPlane"), out shadowNearPlane);
        playerLightConfig._ShadowNearPlane = shadowNearPlane;
    }
    private void ParseFromGlobaLightConfig(ref WeatherData.LightConfig globalLightConfig, SecurityElement globalLightElement)
    {
        Color lightColor = Color.white;
        float intensity = 0;
        float bounceIntensity = 0;
        float rotationX = 0;
        float rotationY = 0;
        float rotationZ = 0;

        ColorUtility.TryParseHtmlString(globalLightElement.Attribute("LightColor"), out lightColor);
        globalLightConfig._LightColor = lightColor;

        float.TryParse(globalLightElement.Attribute("Intensity"), out intensity);
        globalLightConfig._Intensity = intensity;

        float.TryParse(globalLightElement.Attribute("BounceIntensity"), out bounceIntensity);
        globalLightConfig._BounceIntensity = bounceIntensity;

        float.TryParse(globalLightElement.Attribute("RotationX"), out rotationX);
        globalLightConfig._RotationX = rotationX;

        float.TryParse(globalLightElement.Attribute("RotationY"), out rotationY);
        globalLightConfig._RotationY = rotationY;

        float.TryParse(globalLightElement.Attribute("RotationZ"), out rotationZ);
        globalLightConfig._RotationZ = rotationZ;
    }

    private void ParseFromSunConfig(ref WeatherData.SunConfig sun, SecurityElement sunElement)
    {
        Color sunColor = Color.white;
        float sunColorIntensity = 1.0f;
        Color ambientColor = Color.white;
        float ambientColorIntensity = 1.0f;

        ColorUtility.TryParseHtmlString(sunElement.Attribute("SunColor"), out sunColor);
        sun._SunColor = sunColor;

        float.TryParse(sunElement.Attribute("Intensity"), out sunColorIntensity);
        sun._Intensity = sunColorIntensity;

        ColorUtility.TryParseHtmlString(sunElement.Attribute("AmbientColor"), out ambientColor);
        sun._AmbientColor = ambientColor;

        float.TryParse(sunElement.Attribute("AmbientColorIntensity"), out ambientColorIntensity);
        sun._AmbientColorIntensity = ambientColorIntensity;
    }
}

public class GamePersistentConfigParams
{
    public float BGMVolume = 0.5f;

    public bool ReadFromFile()
    {
#if UNITY_IOS || UNITY_ANDROID
        var path = Application.persistentDataPath;
#else
        var path = Environment.CurrentDirectory;
#endif
        string fileName = Path.Combine(path, "sysconfig.txt");
        if (!File.Exists(fileName))
            return false;
        try
        {
            StreamReader reader = FileOperate.OpenTextFile(fileName);
            if (reader == null)
                return false;
            string strLine = reader.ReadLine();
            reader.Close();

            if (strLine != null)
            {
               float fvalue;
               if (float.TryParse(strLine, out fvalue))
                   BGMVolume = fvalue;
            }
            return true;
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("Exception occurs {0}", e);
        }
        return false;
    }

    public void WriteToFile()
    {
#if UNITY_IOS || UNITY_ANDROID
        var path = Application.persistentDataPath;
#else
        var path = Environment.CurrentDirectory;
#endif
        string fileName = Path.Combine(path, "sysconfig.txt");
        try
        {
            StreamWriter sw = File.CreateText(fileName);
            sw.WriteLine(BGMVolume.ToString());
            sw.Close();
            sw.Dispose();
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("Exception occurs {0}", e);
        }
    }
}