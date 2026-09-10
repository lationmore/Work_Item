using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ElanWdt
{
    internal static class DriverIO
    {
        private static readonly Guid BiometricReaderGuid = new Guid("e2b5183a-99ea-4cc3-ad6b-80ca8d715b80");
        private static readonly IntPtr InvalidHandle = new IntPtr(-1);
        private const uint DIGCF_PRESENT = 0x00000002;
        private const uint DIGCF_DEVICEINTERFACE = 0x00000010;
        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;
        private const uint OPEN_EXISTING = 3;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const int ERROR_NO_MORE_ITEMS = 259;

        // 原始協定：FILE_DEVICE_BIOMETRIC=0x44, Function=0x850,
        // FILE_ANY_ACCESS=0, METHOD_BUFFERED=0。
        private const uint SendWriteRequest = (0x44u << 16) | (0x850u << 2);

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public uint cbSize;
            public Guid InterfaceClassGuid;
            public uint Flags;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", EntryPoint = "SetupDiGetClassDevsW", ExactSpelling = true,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, string enumerator,
            IntPtr hwndParent, uint flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet,
            IntPtr deviceInfoData, ref Guid interfaceClassGuid, uint memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", EntryPoint = "SetupDiGetDeviceInterfaceDetailW", ExactSpelling = true,
            CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData,
            uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", ExactSpelling = true,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess,
            uint shareMode, IntPtr securityAttributes, uint creationDisposition,
            uint flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeviceIoControl(SafeFileHandle device, uint controlCode,
            [In] byte[] input, int inputSize, [Out] byte[] output, int outputSize,
            out int bytesReturned, IntPtr overlapped);

        public static string GetDevicePath(string vid, string pid)
        {
            Guid guid = BiometricReaderGuid;
            IntPtr infoSet = SetupDiGetClassDevs(ref guid, null, IntPtr.Zero,
                DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if (infoSet == InvalidHandle)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            try
            {
                string matchedPath = null;
                for (uint index = 0; ; index++)
                {
                    var data = new SP_DEVICE_INTERFACE_DATA
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA))
                    };
                    if (!SetupDiEnumDeviceInterfaces(infoSet, IntPtr.Zero, ref guid, index, ref data))
                    {
                        int error = Marshal.GetLastWin32Error();
                        if (error == ERROR_NO_MORE_ITEMS)
                            break;
                        throw new Win32Exception(error);
                    }

                    uint requiredSize;
                    bool sizeResult = SetupDiGetDeviceInterfaceDetail(infoSet, ref data,
                        IntPtr.Zero, 0, out requiredSize, IntPtr.Zero);
                    int sizeError = Marshal.GetLastWin32Error();
                    if (!sizeResult && sizeError != ERROR_INSUFFICIENT_BUFFER)
                        throw new Win32Exception(sizeError);
                    if (requiredSize < 8)
                        throw new InvalidOperationException("裝置介面資料長度異常。");

                    IntPtr buffer = Marshal.AllocHGlobal(checked((int)requiredSize));
                    try
                    {
                        // Unicode cbSize: x64 process=8, x86 process=6。
                        // DevicePath 的偏移在兩種情況都是 4，不是 cbSize。
                        Marshal.WriteInt32(buffer, IntPtr.Size == 8 ? 8 : 6);
                        uint actualSize;
                        if (!SetupDiGetDeviceInterfaceDetail(infoSet, ref data, buffer,
                            requiredSize, out actualSize, IntPtr.Zero))
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        string path = Marshal.PtrToStringUni(IntPtr.Add(buffer, 4));
                        if (string.IsNullOrEmpty(path) ||
                            path.IndexOf("VID_" + vid, StringComparison.OrdinalIgnoreCase) < 0 ||
                            path.IndexOf("PID_" + pid, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                        if (matchedPath != null)
                            throw new InvalidOperationException("找到多個符合 VID/PID 的介面；請僅保留一個目標裝置後再執行。");
                        matchedPath = path;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
                return matchedPath;
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(infoSet);
            }
        }

        public static int Elan_WDT(string devicePath)
        {
            return Send_Command(devicePath,
                new byte[] { 0x40, 0x27, 0x57, 0x44, 0x54, 0x52, 0x53, 0x54 }, new byte[2]);
        }

        // 保留原始方法，但主程式不會呼叫此方法。
        public static int In_Boot_Code(string devicePath)
        {
            return Send_Command(devicePath,
                new byte[] { 0x42, 0x01, 0x52, 0x55, 0x4E, 0x49, 0x41, 0x50 }, new byte[2]);
        }

        public static int Send_Command(string devicePath, byte[] input, byte[] output,
            uint controlCode = SendWriteRequest)
        {
            if (string.IsNullOrWhiteSpace(devicePath))
                throw new ArgumentException("裝置路徑不可為空。", nameof(devicePath));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            // desiredAccess=0 沿用原始程式；實際允許的權限仍取決於驅動。
            using (SafeFileHandle handle = CreateFile(devicePath, 0,
                FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero))
            {
                if (handle.IsInvalid)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                int bytesReturned;
                if (!DeviceIoControl(handle, controlCode, input, input.Length, output,
                    output.Length, out bytesReturned, IntPtr.Zero))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                return bytesReturned;
            }
        }
    }
}
