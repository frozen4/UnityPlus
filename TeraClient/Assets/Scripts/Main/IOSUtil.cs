using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;  

public static class IOSUtil
{
    public enum PermissionType
    {
        PermissionPhotoLibrary = 0,
        PermissionCamera,
        PermissionMicrophone,
    }

    public enum PermissionStatus
    {
        StatusAuthorized = 0,
        StatusDenied,
        StatusNotDetermined,
    }

    [DllImport("__Internal")]
    private static extern void _iosOpenPhotoLibrary();
    [DllImport("__Internal")]
    private static extern void _iosOpenPhotoAlbums();
    [DllImport("__Internal")]
    private static extern void _iosOpenCamera();
    [DllImport("__Internal")]
    private static extern void _iosOpenPhotoLibrary_allowsEditing();
    [DllImport("__Internal")]
    private static extern void _iosOpenPhotoAlbums_allowsEditing();
    [DllImport("__Internal")]
    private static extern void _iosOpenCamera_allowsEditing();
    [DllImport("__Internal")]
    private static extern void _iosSaveImageToPhotosAlbum(string readAddr);
    [DllImport("__Internal")]
    private static extern IntPtr GetOpenUDID();
    [DllImport("__Internal")]
    private static extern IntPtr GetSysModel();
    [DllImport("__Internal")]
    private static extern IntPtr GetSysVersion();
    [DllImport("__Internal")]
    private static extern void _copyTextToClipboard(string str);
    [DllImport("__Internal")]
    private static extern void _openUrl(string str);
    [DllImport("__Internal")]
    private static extern void _setAVAudioSessionCategory(int avAudioSessionCategory);
    [DllImport("__Internal")]
    private static extern float _getScreenNativeScale();
    [DllImport("__Internal")]
    private static extern float _getScreenScale();
    [DllImport("__Internal")]
    private static extern void _showAlertView(string title, string content);
    [DllImport("__Internal")]
    private static extern int _checkPermission(int permissionType);
    [DllImport("__Internal")]
    private static extern bool _hasPermission(int permissionType);
    [DllImport("__Internal")]
    private static extern void _requestPermission(int permissionType);
    [DllImport("__Internal")]
    private static extern void _openSettingPage();

    public static int CheckPermission(int permissionType)
    {
        return _checkPermission(permissionType);
    }

    public static bool HasPermission(int permissionType)
    {
        return _hasPermission(permissionType);
    }

    public static void RequestPermission(int permissionType)
    {
        _requestPermission(permissionType);
    }

    public static void OpenSettingPage()
    {
        _openSettingPage();
    }

    public static void showAlertView(string title, string content)
    {
        _showAlertView(title, content);
    }

    public static float getScreenNativeScale()
    {
        return _getScreenNativeScale();
    }

    public static float getScreenScale()
    {
        return _getScreenScale();
    }

    public static void iosOpenUrl(string url)
    {
        _openUrl(url);
    }

    public static void setAVAudioSessionCategory(int category)
    {
        _setAVAudioSessionCategory(category);
    }

    public static void iosCopyTextToClipboard(string str)
    {
        _copyTextToClipboard(str);
    }

    public static string iosGetOpenUDID()
    {
        IntPtr str = GetOpenUDID();
        if (str != IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(str);
        }
        else
        {
            return "";
        }
    }

    public static string iosGetSysModel()
    {
        IntPtr str = GetSysModel();
        if (str != IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(str);
        }
        else
        {
            return "";
        }
    }

    public static string iosGetSysVersion()
    {
        IntPtr str = GetSysVersion();
        if (str != IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(str);
        }
        else
        {
            return "";
        }
    }
    public static void iosOpenPhotoLibrary(bool allowsEditing = false)
    {
        if (allowsEditing)
            _iosOpenPhotoLibrary_allowsEditing();
        else
            _iosOpenPhotoLibrary();
    }

    public static void iosOpenPhotoAlbums(bool allowsEditing = false)
    {
        if (allowsEditing)
            _iosOpenPhotoAlbums_allowsEditing();
        else
            _iosOpenPhotoAlbums();
    }

    public static void iosOpenCamera(bool allowsEditing = false)
    {
        if (allowsEditing)
            _iosOpenCamera_allowsEditing();
        else
            _iosOpenCamera();
    }

    public static void iosSaveImageToPhotosAlbum(string readAddr)
    {
        _iosSaveImageToPhotosAlbum(readAddr);
    }

    public static Texture2D Base64StringToTexture2D(string base64)
    {
        Texture2D tex = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        try
        {
            byte[] bytes = System.Convert.FromBase64String(base64);
            tex.LoadImage(bytes);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        return tex;
    }

    public static System.Action<string> CallBack_ImagePath;
    public static System.Action<string> CallBack_PickImage_With_Base64;
    public static System.Action<string> CallBack_ImageSavedToAlbum;

    // 打开相册相机后的从ios回调到unity的方法 
    public static void ImagePathCallBack(string path)
    {
        if (CallBack_ImagePath != null)
        {
            CallBack_ImagePath(path);
        }
    }

    // 打开相册相机后的从ios回调到unity的方法 
    public static void PickImageCallBack_Base64(string base64)
    {
        if (CallBack_PickImage_With_Base64 != null)
        {
            CallBack_PickImage_With_Base64(base64);
        }
    }

    // 保存图片到相册后，从ios回调到unity的方法 
    public static void SaveImageToPhotosAlbumCallBack(string msg)
    {
        if (CallBack_ImageSavedToAlbum != null)
        {
            CallBack_ImageSavedToAlbum(msg);
        }
    } 
}