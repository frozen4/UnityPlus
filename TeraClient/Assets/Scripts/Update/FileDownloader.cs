/*
 * Originally written by John Batte
 * Modifications, API changes and cleanups by Phil Crosby
 * http://codeproject.com/cs/library/downloader.asp
 * Modifications, by quifi
 */

using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Security;

namespace Downloader
{
    /// <summary>
    /// Downloads and resumes files from HTTP, FTP, and File (file://) URLS
    /// </summary>
    public class FileDownloader
    {
        // Block size to download is by default ???.
        private const int downloadBlockSize = 1024 * 32;

        public FileDownloader()
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
            DownloadData data = null;
            this.canceled = false;

            try
            {
                // get download details                
                data = DownloadData.Create(url, hostName, destFileName, timeout);
                //// Find out the name of the file that the web server gave us.
                //string destFileName = Path.GetFileName(data.Response.ResponseUri.ToString());

                // create the download buffer
                byte[] buffer = new byte[downloadBlockSize];

                int readCount;

                // update how many bytes have already been read
                long totalDownloaded = data.StartPoint;

                bool gotCanceled = false;

                while ((int)(readCount = data.DownloadStream.Read(buffer, 0, downloadBlockSize)) > 0)
                {
                    // break on cancel
                    if (this.canceled)
                    {
                        gotCanceled = true;
                        break;
                    }

                    // update total bytes read
                    totalDownloaded += readCount;

                    // save block to end of file
                    SaveToFile(buffer, readCount, data.DownloadingToStream);

                    // send progress info
                    if (data.IsProgressKnown)
                        RaiseProgressChanged(totalDownloaded, data.FileSize);

                    // break on cancel
                    if (this.canceled)
                    {
                        gotCanceled = true;
                        break;
                    }
                }

                // stream could be incomplete
                if (data.IsProgressKnown)
                {
                    if (totalDownloaded < data.FileSize)
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

    /// <summary>
    /// Constains the connection to the file server and other statistics about a file
    /// that's downloading.
    /// </summary>
    public class DownloadData
    {
        private WebResponse response;

        private Stream stream;
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
            String fingerPrintFileName = FileDownloader.MakeFingerPrintFilePath(destFileName);

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
            String fingerPrintFileName = FileDownloader.MakeFingerPrintFilePath(destFileName);

            SecurityElement finger_print = new SecurityElement("finger_print");

            finger_print.AddAttribute("time_stamp", fingerPrint.timeStamp);
            finger_print.AddAttribute("file_size", fingerPrint.fileSize.ToString());

            File.WriteAllText(fingerPrintFileName, finger_print.ToString());
        }

        private static void DeleteFingerPrint(String destFileName)
        {
            File.Delete(FileDownloader.MakeFingerPrintFilePath(destFileName));
        }

        private static void DeleteDestFile(String destFileName)
        {
            File.Delete(FileDownloader.MakeFingerPrintFilePath(destFileName));

            File.Delete(destFileName);
        }

        public static DownloadData Create(string url, string hostname, string destFileName, int timeout)
        {
            // This is what we will return
            DownloadData downloadData = new DownloadData();
            downloadData.downloadTo = destFileName;

            long urlSize = downloadData.GetFileSize(url, hostname, timeout);
            downloadData.size = urlSize;

            WebRequest req = downloadData.GetRequest(url, timeout);
            try
            {
                downloadData.response = (WebResponse)req.GetResponse();
            }
            catch (Exception e)
            {
                throw new ArgumentException(String.Format(
                    "Error downloading \"{0}\": {1}", url, e.Message), e);
            }

            String lastModified = downloadData.response.Headers["Last-Modified"] ?? "";

            // Check to make sure the response isn't an error. If it is this method
            // will throw exceptions.
            ValidateResponse(downloadData.response, url);


            // Take the name of the file given to use from the web server.
            //String fileName = System.IO.Path.GetFileName(downloadData.response.ResponseUri.ToString());

            //String downloadTo = Path.Combine(destFileName, fileName);

            String downloadTo = destFileName;

            FingerPrint fingerPrint = LoadFingerPrint(downloadTo);
            // If we don't know how big the file is supposed to be,
            // we can't resume, so delete what we already have if something is on disk already.
            if (!downloadData.IsProgressKnown
                || fingerPrint.timeStamp != lastModified
                || fingerPrint.fileSize != urlSize)
            {
                DeleteDestFile(downloadTo);
            }

            SaveFingerPrint(downloadTo, new FingerPrint { timeStamp = lastModified, fileSize = urlSize });

            if (downloadData.IsProgressKnown && File.Exists(downloadTo))
            {
                // We only support resuming on http requests
                if (!(downloadData.Response is HttpWebResponse))
                {
                    DeleteDestFile(downloadTo);
                }
                else
                {
                    // Try and start where the file on disk left off
                    downloadData.start = new FileInfo(downloadTo).Length;

                    if (downloadData.start < urlSize)
                    {
                        // Try and resume by creating a new request with a new start position
                        downloadData.response.Close();
                        req = downloadData.GetRequest(url, timeout);
                        ((HttpWebRequest)req).AddRange((int)downloadData.start);
                        downloadData.response = req.GetResponse();

                        if (((HttpWebResponse)downloadData.Response).StatusCode != HttpStatusCode.PartialContent)
                        {
                            // They didn't support our resume request. 
                            DeleteDestFile(downloadTo);
                            downloadData.start = 0;
                        }

//                         if (req != null)        //销毁request
//                         {
//                             req.Abort();
//                             req = null;
//                         }
                    }
                }
            }

            downloadData.downloadToStream = File.Open(downloadTo, FileMode.Append, FileAccess.Write);

            return downloadData;
        }

        // Used by the factory method
        private DownloadData()
        {
        }


        /// <summary>
        /// Checks whether a WebResponse is an error.
        /// </summary>
        /// <param name="response"></param>
        private static void ValidateResponse(WebResponse response, string url)
        {
            if (response is HttpWebResponse)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)response;

                // If it's an HTML page, it's probably an error page. Comment this
                // out to enable downloading of HTML pages.
                if (/*httpResponse.ContentType.Contains("text/html") || */httpResponse.StatusCode == HttpStatusCode.NotFound)	//httpResponse.ContentType could be null
                {
                    throw new ArgumentException(
                        String.Format("Could not download \"{0}\" - a web page was returned from the web server.",
                        url));
                }
            }
            else if (response is FtpWebResponse)
            {
                FtpWebResponse ftpResponse = (FtpWebResponse)response;
                if (ftpResponse.StatusCode == FtpStatusCode.ConnectionClosed)
                    throw new ArgumentException(
                        String.Format("Could not download \"{0}\" - FTP server closed the connection.", url));
            }
            // FileWebResponse doesn't have a status code to check.
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
            WebRequest req = null;
            WebResponse response = null;
            long size = -1;
            try
            {
                req = GetRequest(url, timeout);
                response = req.GetResponse();
                size = response.ContentLength;
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

        private WebRequest GetRequest(string url, int timeout)
        {
            //WebProxy proxy = WebProxy.GetDefaultProxy();
            WebRequest request = WebRequest.Create(url);
            if (request is HttpWebRequest)
            {
                request.Credentials = CredentialCache.DefaultCredentials;
                //Uri result = request.Proxy.GetProxy(new Uri("http://www.google.com"));

                request.Method = "GET";
                request.Timeout = timeout;
                ((HttpWebRequest)request).ServicePoint.ConnectionLimit = 256;
            }
            return request;
        }

        public void Close()
        {
            if(this.response != null)
            {
                this.response.Close();
                this.response = null;
            }

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
        public WebResponse Response
        {
            get { return response; }
            set { response = value; }
        }
        public Stream DownloadStream
        {
            get
            {
                if (this.start == this.size)
                    return Stream.Null;
                if (this.stream == null)
                    this.stream = this.response.GetResponseStream();
                return this.stream;
            }
        }
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

    /// <summary>
    /// Progress of a downloading file.
    /// </summary>
    public class DownloadEventArgs : EventArgs
    {
        private int percentDone;
        private string downloadState;
        private long totalFileSize;

        public long TotalFileSize
        {
            get { return totalFileSize; }
            set { totalFileSize = value; }
        }
        private long currentFileSize;

        public long CurrentFileSize
        {
            get { return currentFileSize; }
            set { currentFileSize = value; }
        }

        public DownloadEventArgs(long totalFileSize, long currentFileSize)
        {
            this.totalFileSize = totalFileSize;
            this.currentFileSize = currentFileSize;

            this.percentDone = (int)((((double)currentFileSize) / totalFileSize) * 100);
        }

        public DownloadEventArgs(string state)
        {
            this.downloadState = state;
        }

        public DownloadEventArgs(int percentDone, string state)
        {
            this.percentDone = percentDone;
            this.downloadState = state;
        }

        public int PercentDone
        {
            get
            {
                return this.percentDone;
            }
        }

        public string DownloadState
        {
            get
            {
                return this.downloadState;
            }
        }
    }
    public delegate void DownloadProgressHandler(object sender, DownloadEventArgs e);
}