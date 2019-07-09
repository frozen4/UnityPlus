using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AssetUtility
{
	/// <summary>
	/// 限制使用一个工作线程的任务执行者。主要用于 AFilePackage IO
	/// </summary>
	public sealed class SingleThreadWorker
	{
		public SingleThreadWorker()
		{
			m_thread = new Thread(WorkerProc);
			m_thread.Start();
		}

		public void Stop()
		{
			m_stopFlag = true;
		}

        public void Abort()
        {
            m_thread.Abort();
        }

		/// <summary>
		/// 异步执行任务，任务产生的异常将被抛弃
		/// </summary>
		/// <param name="task">欲异步执行的任务</param>
		public void StartTask(Action task)
		{
			if (task == null)
				throw new Exception("param 1 can not be null");

			lock (m_taskQueue)
				m_taskQueue.Enqueue(task);

			m_newTaskEvent.Set();
		}

		/// <summary>
		/// 异步执行任务并提供回调通知
		/// </summary>
		/// <typeparam name="T">任务的结果的类型</typeparam>
		/// <param name="task">欲异步执行的任务</param>
		/// <param name="onFinish">任务执行完成时调用此函数，此值可以为 null</param>
		/// <param name="onException">任务发生异常时调用此函数，此值可以为 null</param>
		public void StartTaskWithCallback<T>(Func<T> task, Action<T> onFinish, Action<Exception> onException)
		{
			if (task == null)
				throw new Exception("param 1 can not be null");

			StartTask(()=>
				{
					T returnValue;
					try
					{
						returnValue = task();
					}
					catch (Exception e)
					{
						if (onException != null)
						{
							onException(e);
							return;
						}
						else
						{
							throw;
						}
					}

					if (onFinish != null)
						onFinish(returnValue);
				});
		}

		public void SetThreadPriority(ThreadPriority priority)
		{
			m_thread.Priority = priority;
		}


		private Thread m_thread;
		private volatile Boolean m_stopFlag = false;
		private Queue<Action> m_taskQueue = new Queue<Action>();
		private EventWaitHandle m_newTaskEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        static int eventcount = 0;

		private void WorkerProc()
		{
			while (true)
			{
				if (m_stopFlag == true)
					return;

				while (true)
				{
					Action curTask = null;
					lock (m_taskQueue)
					{
						if (m_taskQueue.Count == 0)
							break;
						curTask = m_taskQueue.Dequeue();
					}

					try
					{
						curTask();
					}
					catch (Exception e)		//抛弃所有异常，保证后续任务正常执行
					{
						UnityEngine.Debug.LogException(e);	//? 是否应该显示？
					}
				}

				//目前队列为空，暂停活动
				m_newTaskEvent.WaitOne();
			}
		}
	}
}	//end namespace AssetUtility
