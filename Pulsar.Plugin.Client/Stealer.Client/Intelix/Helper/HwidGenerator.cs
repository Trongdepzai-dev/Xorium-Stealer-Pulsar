// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.HwidGenerator
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Helper;

public static class HwidGenerator
{
  private static string _hwid;
  private static readonly object _lock = new object();

  public static string GetHwid()
  {
    if (HwidGenerator._hwid != null)
      return HwidGenerator._hwid;
    lock (HwidGenerator._lock)
    {
      if (HwidGenerator._hwid != null)
        return HwidGenerator._hwid;
      List<string> values = new List<string>();
      string mg = (string) null;
      string cpuName = (string) null;
      List<string> vols = (List<string>) null;
      List<string> macs = (List<string>) null;
      Task.WaitAll(Task.Run((Action) (() => mg = HwidGenerator.GetMachineGuid())), Task.Run((Action) (() => cpuName = HwidGenerator.GetCpuName())), Task.Run((Action) (() => vols = HwidGenerator.GetFixedVolumeSerials())), Task.Run((Action) (() => macs = HwidGenerator.GetMacAddresses())));
      if (!string.IsNullOrEmpty(mg))
        values.Add("MG:" + mg);
      if (!string.IsNullOrEmpty(cpuName))
        values.Add("CPU:" + cpuName);
      values.Add("Cores:" + Environment.ProcessorCount.ToString());
      if (vols != null && vols.Count > 0)
        values.Add("VOLS:" + string.Join(",", (IEnumerable<string>) vols));
      if (macs != null && macs.Count > 0)
        values.Add("MACS:" + string.Join(",", (IEnumerable<string>) macs));
      values.Add("MN:" + Environment.MachineName);
      HwidGenerator._hwid = HwidGenerator.ComputeSha256(string.Join("|", (IEnumerable<string>) values));
      return HwidGenerator._hwid;
    }
  }

  private static string ComputeSha256(string input)
  {
    using (SHA256 shA256 = SHA256.Create())
    {
      byte[] hash = shA256.ComputeHash(Encoding.UTF8.GetBytes(input));
      StringBuilder stringBuilder = new StringBuilder(hash.Length * 2);
      foreach (byte num in hash)
        stringBuilder.Append(num.ToString("x2"));
      return stringBuilder.ToString();
    }
  }

  private static string GetMachineGuid()
  {
    try
    {
      using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Microsoft\\Cryptography"))
      {
        string str = registryKey?.GetValue("MachineGuid") as string;
        if (!string.IsNullOrEmpty(str))
          return str.Trim();
      }
      using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\Cryptography"))
        return (registryKey?.GetValue("MachineGuid") is string str ? str.Trim() : (string) null) ?? "";
    }
    catch
    {
      return "";
    }
  }

  private static string GetCpuName()
  {
    try
    {
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0"))
      {
        string str1 = registryKey?.GetValue("ProcessorNameString") as string;
        return !string.IsNullOrEmpty(str1) ? str1.Trim() : (registryKey?.GetValue("VendorIdentifier") is string str2 ? str2.Trim() : (string) null) ?? "";
      }
    }
    catch
    {
      return "";
    }
  }

  private static List<string> GetFixedVolumeSerials()
  {
    List<string> fixedVolumeSerials = new List<string>();
    try
    {
      foreach (DriveInfo drive in DriveInfo.GetDrives())
      {
        if (drive.DriveType == DriveType.Fixed && drive.IsReady)
        {
          StringBuilder lpVolumeNameBuffer = new StringBuilder(261);
          StringBuilder lpFileSystemNameBuffer = new StringBuilder(261);
          uint lpVolumeSerialNumber;
          if (NativeMethods.GetVolumeInformation(drive.RootDirectory.FullName, lpVolumeNameBuffer, lpVolumeNameBuffer.Capacity, out lpVolumeSerialNumber, out uint _, out uint _, lpFileSystemNameBuffer, lpFileSystemNameBuffer.Capacity))
            fixedVolumeSerials.Add(lpVolumeSerialNumber.ToString("X8").ToLowerInvariant());
        }
      }
    }
    catch
    {
    }
    return fixedVolumeSerials;
  }

  private static List<string> GetMacAddresses()
  {
    List<string> macAddresses = new List<string>();
    try
    {
      foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
      {
        if (networkInterface.OperationalStatus == OperationalStatus.Up)
        {
          byte[] addressBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();
          if (addressBytes.Length != 0)
          {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < addressBytes.Length; ++index)
            {
              if (index > 0)
                stringBuilder.Append(':');
              stringBuilder.Append(addressBytes[index].ToString("x2"));
            }
            macAddresses.Add(stringBuilder.ToString());
          }
        }
      }
    }
    catch
    {
    }
    return macAddresses;
  }
}
