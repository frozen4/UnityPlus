using System.Collections.Generic;
namespace Common.Net
{
    public class ThreadSafeQueue<T> where T : class
    {
        private Queue<T> _queue;
        private object _queueObject;
        public int Count { get { return _queue.Count; } }

        public ThreadSafeQueue()
        {
            _queue = new Queue<T>();
            _queueObject = new object();
        }
        public void Enqueue(T item)
        {
            lock (_queueObject)
            {
                _queue.Enqueue(item);
            }
        }

        public bool TryDequeue(out T result)
        {
            T ret = null;
            lock (_queueObject)
            {
                if(_queue.Count == 0)
                {
                    ret = null;
                }
                else
                {
                    ret = _queue.Dequeue();
                }
            }
            result = ret;
            return ret == null ? false : true;
        }

        public void Clear()
        {
            lock (_queueObject)
            {
                _queue.Clear();
            }
        }
    }
}
