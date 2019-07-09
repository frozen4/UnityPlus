
#if SERVER_USE
using Mono.Unix;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public class WorkThread
    {
        private volatile bool stop = false;
        private Action _action = null;
        private Thread _thread = null;
        public WorkThread(Action action)
        {
            _action = action;
        }
        public void run()
        {
            while (!stop)
            {
                _action?.Invoke();
            }

            _action?.Invoke();
        }

        public bool start()
        {
            if (_action == null)
            {
                return false;
            }

            _thread = new Thread(this.run);
            _thread.Start();

            return true;
        }

        public void join()
        {
            stop = true;
            _thread.Join();
        }
    }
}
#endif