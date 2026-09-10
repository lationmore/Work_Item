using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;

namespace ElanWdt
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (args.Length == 1 &&
                (args[0] == "/?" || args[0] == "/？" ||
                 args[0].Equals("--help", StringComparison.OrdinalIgnoreCase)))
            {
                ShowHelp();
                return 0;
            }

            try
            {
                Dictionary<string, string> options = ParseArguments(args);
                string value;
                string vid = NormalizeId(options.TryGetValue("vid", out value)
                    ? value : ConfigurationManager.AppSettings["VID"], "VID");
                string pid = NormalizeId(options.TryGetValue("pid", out value)
                    ? value : ConfigurationManager.AppSettings["PID"], "PID");

                if (!IsAdministrator())
                {
                    Console.Error.WriteLine("請從以系統管理員身分開啟的命令提示字元執行。");
                    return 1;
                }

                Console.WriteLine("VID={0}, PID={1}", vid, pid);
                string devicePath = DriverIO.GetDevicePath(vid, pid);
                if (string.IsNullOrEmpty(devicePath))
                {
                    Console.Error.WriteLine("找不到符合 VID={0}, PID={1} 的 Biometric Reader。", vid, pid);
                    return 1;
                }

                Console.WriteLine("Device: " + devicePath);
                Console.WriteLine("即將停止 WbioSrvc；本工具不會自動重新啟動此服務，即使後續操作失敗。");
                ServiceHelper.StopWbioSrvc();
                Console.WriteLine("傳送 WDT reset 指令...");
                int bytesReturned = DriverIO.Elan_WDT(devicePath);
                Console.WriteLine("DeviceIoControl 成功，BytesReturned={0}；尚未驗證硬體是否完成重置。", bytesReturned);
                return 0;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("參數錯誤：" + ex.Message);
                ShowHelp();
                return 2;
            }
            catch (Win32Exception ex)
            {
                Console.Error.WriteLine("Win32 錯誤：{0} (0x{0:X})，{1}", ex.NativeErrorCode, ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("執行失敗：" + ex.Message);
                Win32Exception native = ex.InnerException as Win32Exception;
                if (native != null)
                    Console.Error.WriteLine("Win32 錯誤：{0} (0x{0:X})，{1}", native.NativeErrorCode, native.Message);
                return 1;
            }
        }

        private static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static Dictionary<string, string> ParseArguments(string[] args)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string arg in args)
            {
                int separator = arg.IndexOf(':');
                if (!arg.StartsWith("/", StringComparison.Ordinal) || separator <= 1 || separator == arg.Length - 1)
                    throw new ArgumentException("格式應為 /vid:04F3 /pid:0C8C。");

                string key = arg.Substring(1, separator - 1);
                string value = arg.Substring(separator + 1);
                if (!key.Equals("vid", StringComparison.OrdinalIgnoreCase) &&
                    !key.Equals("pid", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("不支援的參數：" + key);
                if (result.ContainsKey(key))
                    throw new ArgumentException("重複的參數：" + key);
                result.Add(key, value);
            }
            return result;
        }

        private static string NormalizeId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " 未設定，請修改設定檔或指定命令列參數。");
            value = value.Trim();
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                value = value.Substring(2);
            ushort number;
            if (value.Length != 4 || !ushort.TryParse(value, NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture, out number))
                throw new ArgumentException(name + " 必須是 4 位十六進位，例如 04F3 或 0x04F3。");
            return number.ToString("X4", CultureInfo.InvariantCulture);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("用法：elan_WDT.exe [/vid:04F3] [/pid:0C8C]");
            Console.WriteLine("說明：elan_WDT.exe /? 或 --help");
            Console.WriteLine("未指定的 VID/PID 從設定檔讀取；命令列優先。也接受 0x 前綴。");
            Console.WriteLine("範例：elan_WDT.exe /vid:0x04F3 /pid:0x0C8C");
            Console.WriteLine("此工具會停止 WbioSrvc，且不會自動重新啟動。");
        }
    }

    internal static class ServiceHelper
    {
        public static void StopWbioSrvc()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(10);
            using (var service = new ServiceController("WbioSrvc"))
            {
                service.Refresh();
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    Console.WriteLine("WbioSrvc 已停止。");
                    return;
                }
                if (service.Status == ServiceControllerStatus.StartPending)
                {
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    service.Refresh();
                }
                if (service.Status != ServiceControllerStatus.StopPending)
                    service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                Console.WriteLine("WbioSrvc 已停止。");
            }
        }
    }
}
