
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Common
{
    public class SampleDisposableClass : IDisposable
    {
        // 非托管资源
        private IntPtr _NativeResource = IntPtr.Zero;
        // 托管资源
        private Stream _ManagedResource = null;

        // disposing == true, 直接调用   
        // disposing == false, 垃圾回收器调用
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose Other ManagedObject 
                if (_ManagedResource != null)
                    _ManagedResource.Dispose();
            }

            //Dispose UnManagedObject   
            Close();

            if (disposing)
                GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~SampleDisposableClass()
        { 
            Dispose(false);
        }

        // 类中使用了非托管资源，则要考虑提供Close方法，和Open方法
        public void Open()
        {
            // 非托管资源处理
            if (_NativeResource == IntPtr.Zero)
            {
                _NativeResource = Marshal.AllocHGlobal(100);
            }
        }

        public void Close()
        {
            // 非托管资源处理
            if (_NativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_NativeResource);
                _NativeResource = IntPtr.Zero;
            }
        }
    }
}


