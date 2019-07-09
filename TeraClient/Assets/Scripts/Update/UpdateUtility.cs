using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using Downloader;
using LuaInterface;
using UnityEngine;
using SeasideResearch.LibCurlNet;


public static class UpdateUtility
{


    public static DownloadTaskErrorCode GetByUrl(
        string url,
        string hostName,
        string dest,
        int timeout,
        EventHandler callbackComplete,
        DownloadProgressHandler callbackProgress,
        out string errMsg)
    {
        return DownloadMan.GetByUrl(
            url,
            hostName,
            dest,
            timeout,
            callbackComplete,
            callbackProgress,
            out errMsg);
    }

    private static WebRequest GetRequest(string url, int timeout)
    {
        //WebProxy proxy = WebProxy.GetDefaultProxy();
        WebRequest request = WebRequest.Create(url);
        if (request is HttpWebRequest)
        {
            request.Credentials = CredentialCache.DefaultCredentials;
            request.UseDefaultCredentials = true;
            //Uri result = request.Proxy.GetProxy(new Uri("http://www.google.com"));

            request.Method = "GET";
            request.Timeout = timeout;
            ((HttpWebRequest)request).ServicePoint.ConnectionLimit = 256;
        }

        return request;
    }

    public static long GetUrlFileSize(string url, int timeout)
    {
        WebRequest req = null;
        WebResponse response = null;
        long size = -1;
        try
        {
            req = GetRequest(url, timeout);
            response = req.GetResponse();
            size = response.ContentLength;
        }
        catch(Exception)          //是否是url文件权限问题?
        {
            size = -1;
        }
        finally
        {
            if (req != null)
            {
                req.Abort();
                req = null;
            }
            if (response != null)
            {
                response.Close();
                response = null;
            }
        }

        return size;
    }

    public static Int64 GetUrlFileSizeEx(string url, string hostname, int timeout)
    {
        long size = -1;

        Easy easy = new Easy();
        Easy.SetCurrentEasy(easy);
        Slist headers = new Slist();
        headers.Append(HobaText.Format("Host: {0}", hostname));

        easy.SetOpt(CURLoption.CURLOPT_URL, url);
        easy.SetOpt(CURLoption.CURLOPT_HEADER, 1L);
        easy.SetOpt(CURLoption.CURLOPT_NOBODY, 1L);
        easy.SetOpt(CURLoption.CURLOPT_HTTPHEADER, headers);
        //easy.SetOpt(CURLoption.CURLOPT_IPRESOLVE, (long)CURLipResolve.CURL_IPRESOLVE_V4);
        easy.SetOpt(CURLoption.CURLOPT_CONNECTTIMEOUT, timeout / 1000);

        easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 0L);
        easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 0L);

        int error = 0;
        double downloadFileLength = 0.0f;
        if (easy.Perform() == CURLcode.CURLE_OK)
        {
            CURLcode code = easy.GetInfo(CURLINFO.CURLINFO_RESPONSE_CODE, ref error);

            if (code == CURLcode.CURLE_OK && error == 200)
            {
                easy.GetInfo(CURLINFO.CURLINFO_CONTENT_LENGTH_DOWNLOAD, ref downloadFileLength);
            }

            if (downloadFileLength >= 0.0f)
                size = (int)downloadFileLength;
        }
        else
        {
            size = -1;
        }

        headers.FreeAll();
        Easy.SetCurrentEasy(null);
        easy.Cleanup();

        return size;
    }

    public static string GetUrlContentType(string url, string hostname, int timeout)
    {
        Easy easy = new Easy();
        Easy.SetCurrentEasy(easy);
        Slist headers = new Slist();
        headers.Append(HobaText.Format("Host: {0}", hostname));

        easy.SetOpt(CURLoption.CURLOPT_URL, url);
        easy.SetOpt(CURLoption.CURLOPT_HEADER, 1L);
        easy.SetOpt(CURLoption.CURLOPT_NOBODY, 1L);
        easy.SetOpt(CURLoption.CURLOPT_HTTPHEADER, headers);
        //easy.SetOpt(CURLoption.CURLOPT_IPRESOLVE, (long)CURLipResolve.CURL_IPRESOLVE_V4);
        easy.SetOpt(CURLoption.CURLOPT_CONNECTTIMEOUT, timeout / 1000);

        easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 0L);
        easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 0L);

        int error = 0;
        string contentType = "";
        if (easy.Perform() == CURLcode.CURLE_OK)
        {
            CURLcode code = easy.GetInfo(CURLINFO.CURLINFO_RESPONSE_CODE, ref error);

            if (code == CURLcode.CURLE_OK && error == 200)
            {
                easy.GetInfo(CURLINFO.CURLINFO_CONTENT_TYPE, ref contentType);
            }
        }

        headers.FreeAll();
        Easy.SetCurrentEasy(null);
        easy.Cleanup();

        return contentType;
    }

    public static string GetHostName(string strUrl)
    {
        string strHostName = strUrl;
        int index = strUrl.IndexOf("//");
        if (index >= 0)
            strHostName = strHostName.Substring(index + 2, strHostName.Length - index - 2);

        index = strHostName.IndexOf('/');
        if (index > 0)
            strHostName = strHostName.Substring(0, index);
        index = strHostName.IndexOf(':');           //port去掉        
        if (index > 0)
            strHostName = strHostName.Substring(0, index);

        return strHostName;
    }
}

