using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Collections;

namespace AssetUtility
{
	/// <summary>
	/// 加载文件到高速缓存
	/// </summary>
	public sealed class FileLoader : MonoBehaviour		//需在退出时结束线程，所以做成 MonoBehaviour
	{
		private static FileLoader m_instance;
		private static FileLoader Instance
		{
			get
			{
				if (m_instance == null)
				{
					var go = new GameObject("FileLoader");
					m_instance = go.AddComponent<FileLoader>();
				}
				return m_instance;
			}
		}

		/// <summary>
		/// 加载给定路径的文件
		/// </summary>
		/// <param name="filePath">欲加载的文件路径</param>
		/// <returns>加载成功时返回打开的文件流；失败时返回 NULL </returns>
		public static Stream LoadFileRaw(String filePath)
		{
			try
			{
                string fullpath = HobaText.Format("{0}/{1}", EntryPoint.Instance.ResPath, filePath);
                //UnityEngine.Debug.Log("LoadFileRaw " + fullpath);
                FileStream fs = new FileStream(fullpath, FileMode.Open);
                return fs as Stream; 
			}
			catch (IOException)
			{
			    return null;
			}
			catch (System.IO.IsolatedStorage.IsolatedStorageException)
			{
			    return null;
			}
		}

		/// <summary>
		/// 加载给定路径的文件
		/// </summary>
		/// <param name="filePath">欲加载的文件路径</param>
		/// <returns>加载成功时返回打开的文件流；失败时返回 NULL </returns>
		public static Stream LoadFile(String filePath)
		{
			if (m_logEnabled)
			{
				lock (m_logs)
				{
					TimeSpan begTime = m_logWatch.Elapsed;
					var result = LoadFileRaw(filePath);
					TimeSpan endTime = m_logWatch.Elapsed;
					if (result != null)
					{
						m_logs.Add(new LogLine { file = filePath, time = (endTime - begTime).TotalSeconds });
					}
					return result;
				}
			}
			else
			{
				return LoadFileRaw(filePath);
			}
		}

		/// <summary>
		/// 异步加载给定路径的文件，完成时调用给定的回调函数。回调函数可能在子线程中被调用
		/// </summary>
		/// <param name="filePath">欲加载的文件路径</param>
		/// <param name="onFinish">加载完成时调用此函数。如果加载成功，传入函数的参数为打开的文件流；如果加载失败，传入函数的参数为null</param>
		public static void AsyncLoadFile(String filePath, Action<Stream> onFinish)
		{
			var self = Instance;
			self.m_worker.StartTaskWithCallback(()=>
			{
				return LoadFile(filePath);
			}, onFinish, (e) => onFinish(null));
		}

		public static void SetThreadPriority(System.Threading.ThreadPriority priority)
		{
			Instance.m_worker.SetThreadPriority(priority);
		}

		/// <summary>
		/// 退出时结束线程
		/// </summary>
		private void OnApplicationQuit()
		{
			m_worker.Stop();
		}

		private SingleThreadWorker m_worker = new SingleThreadWorker();

		public struct LogLine:IEquatable<LogLine>
		{
			public String file;
			public Double time;

            public bool Equals(LogLine other)
            {
                //throw new NotImplementedException();
                if (!(this.file.Equals(other.file)))
                    return false;
                if (!(this.time.Equals(other.time)))
                    return false;
                else
                    return true;
            }
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (!(obj is LogLine))
                    return false;
                return Equals((LogLine)obj);

            }
            public override int GetHashCode()
            {
                return file.GetHashCode() ^ time.GetHashCode();
            }
        }
		static private List<LogLine> m_logs = new List<LogLine>();
		static private Boolean m_logEnabled = false;
		static private Stopwatch m_logWatch;
		public static void EnableLog(Boolean enabled)
		{
			lock (m_logs)
			{
				m_logEnabled = enabled;
				if (enabled)
				{
					m_logWatch = Stopwatch.StartNew();;
					m_logs.Clear();
				}
			}
		}
		public static String GetLog()
		{
			lock (m_logs)
			{
				var strBuilder = HobaText.GetStringBuilder();
				strBuilder.AppendLine("=============IO Log=============");
				Double totalTime = 0;
                for(int i = 0; i < m_logs.Count; ++i)
				{
                    var log = m_logs[i];
                    totalTime += log.time;
					strBuilder.AppendFormat("{0} @ {1} ms", log.file, log.time * 1000);
					strBuilder.AppendLine();
				}
				strBuilder.AppendFormat("total time: {0} ms", totalTime * 1000);
				strBuilder.AppendLine();
				return strBuilder.ToString();
			}
		}
	}
}	//end namespace AssetUtility
