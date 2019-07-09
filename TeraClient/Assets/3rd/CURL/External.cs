/***************************************************************************
 *
 * Project: libcurl.NET
 *
 * Copyright (c) 2004, 2005 Jeff Phillips (jeff@jeffp.net)
 *
 * This software is licensed as described in the file COPYING, which you
 * should have received as part of this distribution.
 *
 * You may opt to use, copy, modify, merge, publish, distribute and/or sell
 * copies of this Software, and permit persons to whom the Software is
 * furnished to do so, under the terms of the COPYING file.
 *
 * This software is distributed on an "AS IS" basis, WITHOUT WARRANTY OF
 * ANY KIND, either express or implied.
 *
 * $Id: External.cs,v 1.1 2005/02/17 22:47:25 jeffreyphillips Exp $
 **************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SeasideResearch.LibCurlNet
{
#pragma warning disable 414
    public class MonoPInvokeCallbackAttribute : System.Attribute
    {
        private Type type;
        public MonoPInvokeCallbackAttribute(Type t) { type = t; }
    }
#pragma warning restore 414

#if !UNITY_IPHONE
    [SuppressUnmanagedCodeSecurity]
#endif
	/// <summary>
	/// P/Invoke signatures.
	/// </summary>
	internal class External
	{

#if UNITY_IPHONE
        const string LUADLL = "__Internal";
#else
#if !SERVER_USE
        const string LUADLL = "hoba";
#else
        const string LUADLL = "hobaserver";
#endif
#endif

        // internal delegates from cURL
        internal delegate int CURL_WRITE_DELEGATE(IntPtr buf, int sz,
            int nmemb, IntPtr parm);
        internal delegate int CURL_READ_DELEGATE(IntPtr buf, int sz,
            int nmemb, IntPtr parm);
        internal delegate int CURL_PROGRESS_DELEGATE(IntPtr parm,
            double dlTotal, double dlNow, double ulTotal, double ulNow);
        internal delegate int CURL_DEBUG_DELEGATE(CURLINFOTYPE infoType,
            IntPtr msgBuf, int msgBufSize, IntPtr parm);
        internal delegate int CURL_HEADER_DELEGATE(IntPtr buf, int sz,
            int nmemb, IntPtr stream);
        internal delegate int CURL_SSL_CTX_DELEGATE(IntPtr ctx,
            IntPtr parm);
        internal delegate CURLIOERR CURL_IOCTL_DELEGATE(
            CURLIOCMD cmd, IntPtr parm);
        internal delegate void CURLSH_LOCK_DELEGATE(int data,
            int access, IntPtr userPtr);
        internal delegate void CURLSH_UNLOCK_DELEGATE(int data,
            IntPtr userPtr);

        // libcurl imports
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_global_init")]
        internal static extern CURLcode curl_global_init(int flags);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_global_cleanup")]
        internal static extern void curl_global_cleanup();
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl,
             CharSet = CharSet.Ansi, EntryPoint = "CURL_curl_escape")]
        internal static extern IntPtr curl_escape([MarshalAs(UnmanagedType.LPStr)]String url, int length);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl,
             CharSet = CharSet.Ansi, EntryPoint = "CURL_curl_unescape")]
        internal static extern IntPtr curl_unescape([MarshalAs(UnmanagedType.LPStr)]String url, int length);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_free")]
        internal static extern void curl_free(IntPtr p);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_version")]
        internal static extern IntPtr curl_version();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_init")]
        internal static extern IntPtr curl_easy_init();
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_cleanup")]
        internal static extern void curl_easy_cleanup(IntPtr pCurl);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_setopt_ptr")]
        internal static extern CURLcode curl_easy_setopt_ptr(IntPtr pCurl,
            CURLoption opt, IntPtr parm);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_setopt_int")]
        internal static extern CURLcode curl_easy_setopt_int(IntPtr pCurl,
            CURLoption opt, int parm);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_setopt_int64")]
        internal static extern CURLcode curl_easy_setopt_int64(IntPtr pCurl,
            CURLoption opt, Int64 parm);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_perform")]
        internal static extern CURLcode curl_easy_perform(IntPtr pCurl);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_duphandle")]
        internal static extern IntPtr curl_easy_duphandle(IntPtr pCurl);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_strerror")]
        internal static extern IntPtr curl_easy_strerror(CURLcode err);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_getinfo_ptr")]
        internal static extern CURLcode curl_easy_getinfo_ptr(IntPtr pCurl,
            CURLINFO info, ref IntPtr pInfo);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_getinfo_int")]
        internal static extern CURLcode curl_easy_getinfo_int(IntPtr pCurl,
            CURLINFO info, ref int pInfo);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_getinfo_double")]
        internal static extern CURLcode curl_easy_getinfo_double(IntPtr pCurl,
            CURLINFO info, ref double pInfo);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_getinfo_time")]
        internal static extern CURLcode curl_easy_getinfo_time(IntPtr pCurl,
            CURLINFO info, ref int yy, ref int mm, ref int dd, ref int hh, ref int mn, ref int ss);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_easy_reset")]
        internal static extern void curl_easy_reset(IntPtr pCurl);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_multi_init")]
        internal static extern IntPtr curl_multi_init();
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_multi_cleanup")]
        internal static extern CURLMcode curl_multi_cleanup(IntPtr pmulti);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_multi_add_handle")]
        internal static extern CURLMcode curl_multi_add_handle(IntPtr pmulti, IntPtr peasy);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_multi_remove_handle")]
        internal static extern CURLMcode curl_multi_remove_handle(IntPtr pmulti, IntPtr peasy);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_multi_strerror")]
        internal static extern IntPtr curl_multi_strerror(CURLMcode errorNum);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_multi_perform")]
        internal static extern CURLMcode curl_multi_perform(IntPtr pmulti,
            ref int runningHandles);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_formfree")]
        internal static extern void curl_formfree(IntPtr pForm);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_share_init")]
        internal static extern IntPtr curl_share_init();
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_share_cleanup")]
        internal static extern CURLSHcode curl_share_cleanup(IntPtr pShare);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_share_strerror")]
        internal static extern IntPtr curl_share_strerror(CURLSHcode errorCode);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_share_setopt")]
        internal static extern CURLSHcode curl_share_setopt(IntPtr pShare,
            CURLSHoption optCode, IntPtr option);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_version_info")]
        internal static extern IntPtr curl_version_info(CURLversion ver);

        // libcurlshim imports
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_initialize")]
        internal static extern void curl_shim_initialize();
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_cleanup")]
        internal static extern void curl_shim_cleanup();
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_alloc_strings")]
        internal static extern IntPtr curl_shim_alloc_strings();
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl,
             CharSet = CharSet.Ansi, EntryPoint = "CURL_curl_shim_add_string_to_slist")]
        internal static extern IntPtr curl_shim_add_string_to_slist(
            IntPtr pStrings, [MarshalAs(UnmanagedType.LPStr)]String str);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl,
             CharSet = CharSet.Ansi, EntryPoint = "CURL_curl_shim_get_string_from_slist")]
        internal static extern IntPtr curl_shim_get_string_from_slist(
            IntPtr pSlist, ref IntPtr pStr);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl,
             CharSet = CharSet.Ansi, EntryPoint = "CURL_curl_shim_add_string")]
        internal static extern IntPtr curl_shim_add_string(IntPtr pStrings, [MarshalAs(UnmanagedType.LPStr)]String str);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_free_strings")]
        internal static extern void curl_shim_free_strings(IntPtr pStrings);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_install_delegates")]
        internal static extern int CURL_curl_shim_install_delegates(IntPtr pCurl, IntPtr pThis,
            IntPtr pWrite, IntPtr pRead,
            IntPtr pProgress, IntPtr pDebug,
            IntPtr pHeader, IntPtr pCtx,
            IntPtr pIoctl);

        internal static int curl_shim_install_delegates(IntPtr pCurl, IntPtr pThis,
            CURL_WRITE_DELEGATE pWrite, CURL_READ_DELEGATE pRead,
            CURL_PROGRESS_DELEGATE pProgress, CURL_DEBUG_DELEGATE pDebug,
            CURL_HEADER_DELEGATE pHeader, CURL_SSL_CTX_DELEGATE pCtx,
            CURL_IOCTL_DELEGATE pIoctl)
        {
            IntPtr write = Marshal.GetFunctionPointerForDelegate(pWrite);
            IntPtr read = Marshal.GetFunctionPointerForDelegate(pRead);
            IntPtr progress = Marshal.GetFunctionPointerForDelegate(pProgress);
            IntPtr debug = Marshal.GetFunctionPointerForDelegate(pDebug);
            IntPtr header = Marshal.GetFunctionPointerForDelegate(pHeader);
            IntPtr ctx = Marshal.GetFunctionPointerForDelegate(pCtx);
            IntPtr ioctl = Marshal.GetFunctionPointerForDelegate(pIoctl);

            return CURL_curl_shim_install_delegates(pCurl, pThis, write, read, progress, debug, header, ctx, ioctl);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_cleanup_delegates")]
        internal static extern void curl_shim_cleanup_delegates(IntPtr pThis);
//         [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_get_file_time")]
//         internal static extern void curl_shim_get_file_time(Int64 unixTime,     //time_t
//             ref int yy, ref int mm, ref int dd, ref int hh, ref int mn, ref int ss);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_free_slist")]
        internal static extern void curl_shim_free_slist(IntPtr p);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_alloc_fd_sets")]
        internal static extern IntPtr curl_shim_alloc_fd_sets();
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_free_fd_sets")]
        internal static extern void curl_shim_free_fd_sets(IntPtr fdsets);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_multi_fdset")]
        internal static extern CURLMcode curl_shim_multi_fdset(IntPtr multi,
            IntPtr fdsets, ref int maxFD);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_select")]
        internal static extern int curl_shim_select(int maxFD, IntPtr fdsets,
            int timeoutMillis);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_multi_info_read")]
        internal static extern IntPtr curl_shim_multi_info_read(IntPtr multi,
            ref int nMsgs);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_multi_info_free")]
        internal static extern void curl_shim_multi_info_free(IntPtr multiInfo);
//         [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
//         internal static extern int curl_shim_formadd(IntPtr[] ppForms,
//             IntPtr[] pParams, int nParams);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_install_share_delegates")]
        internal static extern int CURL_curl_shim_install_share_delegates(IntPtr pShare,
            IntPtr pThis, IntPtr pLock,
            IntPtr pUnlock);

        internal static int curl_shim_install_share_delegates(IntPtr pShare,
            IntPtr pThis, CURLSH_LOCK_DELEGATE pLock,
            CURLSH_UNLOCK_DELEGATE pUnlock)
        {
            IntPtr fLock = Marshal.GetFunctionPointerForDelegate(pLock);
            IntPtr fUnlock = Marshal.GetFunctionPointerForDelegate(pUnlock);
            return CURL_curl_shim_install_share_delegates(pShare, pThis, fLock, fUnlock);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_cleanup_share_delegates")]
        internal static extern void curl_shim_cleanup_share_delegates(IntPtr pShare);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_get_version_int_value")]
        internal static extern int curl_shim_get_version_int_value(IntPtr p, int offset);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_get_version_char_ptr")]
        internal static extern IntPtr curl_shim_get_version_char_ptr(IntPtr p, int offset);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_get_number_of_protocols")]
        internal static extern int curl_shim_get_number_of_protocols(IntPtr p, int offset);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_curl_shim_get_protocol_string")]
        internal static extern IntPtr curl_shim_get_protocol_string(IntPtr p, int offset,
            int index);

         //utility
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CURL_GetUrlFileSize")]
        internal static extern long CURL_GetUrlFileSize([MarshalAs(UnmanagedType.LPStr)]String url, int timeout);
    }
}
