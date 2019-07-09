using System;
using System.Collections.Generic;
using System.Text;

public enum UpdateRetCode
{
    // general
    success = 0,
    fail = 1,
    cancel = 2,
    invalid_param = 3,
    fatal_error = 4,
    sepfile = 5,

    // file
    file_err = 101,
    file_not_exist = 102,
    file_read_err = 103,
    file_write_err = 104,
    pack_file_err = 105,
    pack_file_broken = 106,
    pack_file_invalid = 107,
    zfile_broken = 108,
    package_err = 109,
    package_open_fail = 110,
    package_create_fail = 111,
    package_read_fail = 112,
    package_write_fail = 113,

    // pack
    pack_err = 201,
    pack_open_err = 202,
    pack_read_err = 203,
    pack_write_err = 204,

    // net
    net_err = 301,
    connect_fail = 302,
    download_fail = 303,
    net_partial_file = 304,
    operation_timeouted = 305,

    // version
    element_ver_err = 401,
    element_no_ver_file = 402,
    element_invalid_ver_file = 403,
    element_version_too_new = 404,
    patcher_ver_err = 405,
    patcher_no_ver_file = 406,
    patcher_invalid_ver_file = 407,
    patcher_version_too_new = 408,
    patcher_version_too_old = 409,

    // serverlist
    server_list_parse_err = 501,
    server_list_no_file = 502,
    // rolelist
    role_list_parse_err = 551,
    role_list_no_file = 552,

    // others
    md5_not_match = 901,
    disk_no_space = 902,
    version_changed = 903,
    io_err = 904,
    urlarg_error = 905,

    // no error
    normal = 1001,		// no update, false
    active = 1002,		// has update, true
    download = 1003,		// big file, navigate to url
}

public static class UpdateConfig
{
    //更新服务器相对路径
    public const string VersionConfigRelativePath  = "version.txt";
    public const string ServerListRelativePathXML = "ServerList.xml";
    public const string ServerListRelativePathJSON = "";

    public const float ReconnectTime = 6;       //如果下载不成功，重连间隔(秒)
    public const float TotalReconnectTime = 10;       //总重连次数
    public const int MaxZeroDownloadTime = 15 * 1000;       //超过多久下载量一直为0时算作错误(毫秒)
}

