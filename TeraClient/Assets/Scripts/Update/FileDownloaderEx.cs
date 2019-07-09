using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Security;
using SeasideResearch.LibCurlNet;

namespace Downloader
{
    public delegate Int32 WRITE_DATA_DELEGATE(Byte[] buf, Int32 size, Int32 nmemb,
        Object extraData);
    public class DownLoadParam
    {
        public FileDownloaderEx downloader;
        public DownloadDataEx downloadData;
        public long totalDownloaded;
    }

    public class FileDownloaderEx
    {
        // Block size to download is by default ???.
        private const int downloadBlockSize = 1024 * 32;

        public FileDownloaderEx()
        {
            
        }

        // Determines whether the user has canceled or not.
        private bool canceled = false;

        public void Cancel()
        {
            this.canceled = true;
        }

        /// <summary>
        /// Progress update
        /// </summary>
        public event DownloadProgressHandler ProgressChanged;

        /// <summary>
        /// Fired when progress reaches 100%.
        /// </summary>
        public event EventHandler DownloadComplete;

        private void OnDownloadComplete()
        {
            if (this.DownloadComplete != null)
                this.DownloadComplete(this, new EventArgs());
        }

        ///// <summary>
        ///// Begin downloading the file at the specified url, and save it to the current fileName.
        ///// </summary>
        //public void Download(string url)
        //{
        //    Download(url, "");
        //}
        /// <summary>
        /// Begin downloading the file at the specified url, and save it to the given fileName.
        /// </summary>
        public void Download(string url, string hostName, string destFileName, int timeout)
        {
            DownloadDataEx data = null;
            this.canceled = false;

            try
            {
                // get download details                
                data = DownloadDataEx.Create(url, hostName, destFileName, 2000);
                //// Find out the name of the file that the web server gave us.
                //string destFileName = Path.GetFileName(data.Response.ResponseUri.ToString());

                DownLoadParam param = new DownLoadParam()
                {
                    downloader = this,
                    downloadData = data,
                    totalDownloaded = data.StartPoint,
                };

                data.DoDownload(url, hostName, destFileName, timeout, downloadBlockSize, OnWriteData, param);

                bool gotCanceled = false;
                if (this.canceled)
                {
                    gotCanceled = true;
                }

                // stream could be incomplete
                if (data.IsProgressKnown)
                {
                    if (param.totalDownloaded < data.FileSize)
                        throw new WebException("date transfer not completed", WebExceptionStatus.ConnectionClosed);
                }

                if (!gotCanceled)
                {
                    RaiseProgressChanged(data.FileSize, data.FileSize);
                    data.CleanOnFinish();
                    OnDownloadComplete();
                }
            }
            catch (UriFormatException e)
            {
                throw new ArgumentException(
                    String.Format("Could not parse the URL \"{0}\" - it's either malformed or is an unknown protocol.", url), e);
            }
            finally
            {
                if (data != null)
                    data.Close();
            }
        }

        public static int OnWriteData(Byte[] buf, Int32 size, Int32 nmemb, Object extraData)
        {
            DownLoadParam param = (DownLoadParam)extraData;
            FileDownloaderEx downloader = param.downloader;
            DownloadDataEx data = param.downloadData; 

            if (downloader.canceled)
                return -1;

            int readCount = size * nmemb;
            param.totalDownloaded += readCount;

            // save block to end of file
            downloader.SaveToFile(buf, readCount, data.DownloadingToStream);

            // send progress info
            if (data.IsProgressKnown)
                downloader.RaiseProgressChanged(param.totalDownloaded, data.FileSize);

            if (downloader.canceled)
                return -1;
            else
                return size * nmemb;
        }


        public static String MakeFingerPrintFilePath(String localFilePatch)
        {
            return localFilePatch + ".fp";
        }

        /// <summary>
        /// whether localFilePatch has downloading progressing information
        /// </summary>
        /// <param name="localFilePatch"></param>
        /// <returns></returns>
        public static Boolean IsFileInProgress(String localFilePatch)
        {
            return File.Exists(MakeFingerPrintFilePath(localFilePatch));
        }

        private void SaveToFile(byte[] buffer, int count, FileStream f)
        {
            try
            {
                f.Write(buffer, 0, count);
                f.Flush();
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(
                    String.Format("Error trying to save file \"{0}\": {1}", f.Name, e.Message), e);
            }
        }
        private void RaiseProgressChanged(long current, long target)
        {
            if (this.ProgressChanged != null)
                this.ProgressChanged(this, new DownloadEventArgs(target, current));
        }
    }

    public class DownloadDataEx
    {
        private String downloadTo;
        private FileStream downloadToStream = null;
        private long size;
        private long start;

        private struct FingerPrint
        {
            public String timeStamp;
            public Int64 fileSize;
        }
        private static FingerPrint LoadFingerPrint(String destFileName)
        {
            String fingerPrintFileName = FileDownloaderEx.MakeFingerPrintFilePath(destFileName);

            if (!File.Exists(fingerPrintFileName))	//记录文件尚未创建
                return new FingerPrint { timeStamp = "", fileSize = 0 };

            try
            {
                SecurityElement xmlDoc = SecurityElement.FromString(File.ReadAllText(fingerPrintFileName));
                String timeStamp = xmlDoc.Attributes["time_stamp"].ToString();
                Int64 fileSize = Int64.Parse(xmlDoc.Attributes["file_size"].ToString());
                return new FingerPrint { timeStamp = timeStamp, fileSize = fileSize };
            }
            catch (IOException)
            {
                return new FingerPrint { timeStamp = "", fileSize = 0 };
            }
            catch (System.IO.IsolatedStorage.IsolatedStorageException)
            {
                return new FingerPrint { timeStamp = "", fileSize = 0 };
            }
            catch (XmlSyntaxException)
            {
                return new FingerPrint { timeStamp = "", fileSize = 0 };
            }
            catch (FormatException)
            {
                return new FingerPrint { timeStamp = "", fileSize = 0 };
            }
            catch (NullReferenceException)
            {
                return new FingerPrint { timeStamp = "", fileSize = 0 };
            }
        }

        private static void SaveFingerPrint(String destFileName, FingerPrint fingerPrint)
        {
            String fingerPrintFileName = FileDownloaderEx.MakeFingerPrintFilePath(destFileName);

            SecurityElement finger_print = new SecurityElement("finger_print");

            finger_print.AddAttribute("time_stamp", fingerPrint.timeStamp);
            finger_print.AddAttribute("file_size", fingerPrint.fileSize.ToString());

            File.WriteAllText(fingerPrintFileName, finger_print.ToString());
        }

        private static void DeleteFingerPrint(String destFileName)
        {
            File.Delete(FileDownloaderEx.MakeFingerPrintFilePath(destFileName));
        }

        private static void DeleteDestFile(String destFileName)
        {
            File.Delete(FileDownloaderEx.MakeFingerPrintFilePath(destFileName));

            File.Delete(destFileName);
        }

        public static DownloadDataEx Create(string url, string hostname, string destFileName, int timeout)
        {
            String lastModified = "";

            // This is what we will return
            DownloadDataEx downloadData = new DownloadDataEx();

            downloadData.downloadTo = destFileName;

            long urlSize = downloadData.GetFileSize(url, hostname, timeout, ref lastModified);
            downloadData.size = urlSize;

            String downloadTo = destFileName;

            FingerPrint fingerPrint = LoadFingerPrint(downloadTo);
            // If we don't know how big the file is supposed to be,
            // we can't resume, so delete what we already have if something is on disk already.
            if (!downloadData.IsProgressKnown
                || lastModified == "" || fingerPrint.timeStamp != lastModified
                || fingerPrint.fileSize != urlSize)
            {
                DeleteDestFile(downloadTo);
            }

            SaveFingerPrint(downloadTo, new FingerPrint { timeStamp = lastModified, fileSize = urlSize });

            if (downloadData.IsProgressKnown && File.Exists(downloadTo))
            {
                DeleteDestFile(downloadTo);
                downloadData.start = 0;
            }

            downloadData.downloadToStream = File.Open(downloadTo, FileMode.CreateNew, FileAccess.Write);

            return downloadData;
        }

        // Used by the factory method
        private DownloadDataEx()
        {
        }

        /// <summary>
        /// Checks the file size of a remote file. If size is -1, then the file size
        /// could not be determined.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="progressKnown"></param>
        /// <returns></returns>
        public long GetFileSize(string url, string hostName, int timeout)
        {
            long size = -1;

            Easy easy = new Easy();
            Easy.SetCurrentEasy(easy);

            Slist headers = new Slist();

            CURLcode curlCode = CURLcode.CURLE_OK;

            try
            {

                headers.Append(HobaText.Format("Host: {0}", hostName));
                easy.SetOpt(CURLoption.CURLOPT_URL, url);
                easy.SetOpt(CURLoption.CURLOPT_HEADER, 1L);
                easy.SetOpt(CURLoption.CURLOPT_NOBODY, 1L);
                easy.SetOpt(CURLoption.CURLOPT_HTTPHEADER, headers);
                //easy.SetOpt(CURLoption.CURLOPT_DNS_CACHE_TIMEOUT, timeout);
                easy.SetOpt(CURLoption.CURLOPT_CONNECTTIMEOUT, timeout / 1000);

                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 0L);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 0L);

                int error = 0;
                double downloadFileLength = 0.0f;
                curlCode = easy.Perform();
                if (curlCode == CURLcode.CURLE_OK)
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
            }
            finally
            {
                headers.FreeAll();

                Easy.SetCurrentEasy(null);
                easy.Cleanup();
            }

            if (curlCode != CURLcode.CURLE_OK)
            {
                throw new ArgumentException(String.Format(
                    "Error downloading \"{0}\": {1}", url, curlCode));
            }

            return size;
        }

        public long GetFileSize(string url, string hostName, int timeout, ref string modifiedTimeGMT)
        {
            long size = -1;
            modifiedTimeGMT = "";

            Easy easy = new Easy();
            Easy.SetCurrentEasy(easy);

            Slist headers = new Slist();
            CURLcode curlCode = CURLcode.CURLE_OK;

            try
            {
                headers.Append(HobaText.Format("Host: {0}", hostName));
                easy.SetOpt(CURLoption.CURLOPT_URL, url);
                easy.SetOpt(CURLoption.CURLOPT_FILETIME, 1L);
                easy.SetOpt(CURLoption.CURLOPT_HEADER, 1L);
                easy.SetOpt(CURLoption.CURLOPT_NOBODY, 1L);
                easy.SetOpt(CURLoption.CURLOPT_HTTPHEADER, headers);
                easy.SetOpt(CURLoption.CURLOPT_CONNECTTIMEOUT, timeout / 1000);

                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 0L);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 0L);

                int error = 0;
                double downloadFileLength = -1.0f;
                curlCode = easy.Perform();
                if (curlCode == CURLcode.CURLE_OK)
                {
                    CURLcode code = easy.GetInfo(CURLINFO.CURLINFO_RESPONSE_CODE, ref error);

                    if (code == CURLcode.CURLE_OK && error == 200)
                    {
                        easy.GetInfo(CURLINFO.CURLINFO_CONTENT_LENGTH_DOWNLOAD, ref downloadFileLength);

                        DateTime time = new DateTime();
                        code = easy.GetInfo(CURLINFO.CURLINFO_FILETIME, ref time);
                        if (code == CURLcode.CURLE_OK)
                        {
                            modifiedTimeGMT = time.ToUniversalTime().ToString("R");
                        }
                    }

                    if (downloadFileLength >= 0.0f)
                        size = (int)downloadFileLength;
                }
                else
                {
                    size = -1;
                }
            }
            finally
            {
                headers.FreeAll();
                Easy.SetCurrentEasy(null);
                easy.Cleanup();
            }

            if (curlCode != CURLcode.CURLE_OK)
            {
                throw new ArgumentException(String.Format(
                    "Error downloading \"{0}\": {1}", url, curlCode));
            }

            return size;
        }

        public bool DoDownload(string url, string hostname, string destFileName, int timeout, long bufferSize, WRITE_DATA_DELEGATE onWriteData, Object extraData )
        {
            bool ret = false;
            Easy easy = new Easy();
            Easy.SetCurrentEasy(easy);

            Slist headers = new Slist();
            Easy.WriteFunction wf = new Easy.WriteFunction(onWriteData);
            CURLcode curlCode = CURLcode.CURLE_OK;

            try
            {
                headers.Append(HobaText.Format("Host: {0}", hostname));
                easy.SetOpt(CURLoption.CURLOPT_URL, url);
                easy.SetOpt(CURLoption.CURLOPT_WRITEDATA, extraData);
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);
                easy.SetOpt(CURLoption.CURLOPT_HEADER, 0L);
                easy.SetOpt(CURLoption.CURLOPT_VERBOSE, 0L);
                easy.SetOpt(CURLoption.CURLOPT_BUFFERSIZE, (long)bufferSize);
                easy.SetOpt(CURLoption.CURLOPT_HTTPHEADER, headers);
                //easy.SetOpt(CURLoption.CURLOPT_IPRESOLVE, (long)CURLipResolve.CURL_IPRESOLVE_V4);
                easy.SetOpt(CURLoption.CURLOPT_CONNECTTIMEOUT, timeout / 1000);

                //
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 0L);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 0L);

                if (StartPoint > 0)
                    easy.SetOpt(CURLoption.CURLOPT_RESUME_FROM, StartPoint);

                int error = 0;
                curlCode = easy.Perform();
                if (curlCode == CURLcode.CURLE_OK)
                {
                    CURLcode code = easy.GetInfo(CURLINFO.CURLINFO_RESPONSE_CODE, ref error);
                    if (code == CURLcode.CURLE_OK && error == 200)
                    {
                        ret = true;
                    }
                }

            }
            finally
            {
                headers.FreeAll();

                Easy.SetCurrentEasy(null);
                easy.Cleanup();
            }

            if (curlCode != CURLcode.CURLE_OK)
            {
                throw new WebException(String.Format(
                    "Error downloading \"{0}\": {1}", url, curlCode));
            }

            return ret;
        }

        public void Close()
        {
            if (this.downloadToStream != null)
            {
                this.downloadToStream.Dispose();
                this.downloadToStream = null;
            }
        }

        public void CleanOnFinish()
        {
            DeleteFingerPrint(downloadTo);
        }

        #region Properties

        public FileStream DownloadingToStream
        {
            get
            {
                return this.downloadToStream;
            }
        }
        public long FileSize
        {
            get
            {
                return this.size;
            }
        }
        public long StartPoint
        {
            get
            {
                return this.start;
            }
        }
        public bool IsProgressKnown
        {
            get
            {
                // If the size of the remote url is -1, that means we
                // couldn't determine it, and so we don't know
                // progress information.
                return this.size > -1;
            }
        }
        #endregion
    }
}
