using System.Collections.Generic;

namespace Common.Net
{
    class ThreadSafeStack<T> where T : class
    {
        private Stack<T> _stack;
        private object _stackObject;
        public ThreadSafeStack()
        {
            _stack = new Stack<T>();
            _stackObject = new object();
        }

        public int Count
        {
            get { return _stack.Count; }
        }

        public T Pop()
        {

            T result = null;
            lock (_stackObject)
            {
                result = _stack.Pop();
            }

            return result;
        }

        public void Push(T item)
        {
            lock (_stackObject)
            {
                _stack.Push(item);
            }
        }
    }
}
