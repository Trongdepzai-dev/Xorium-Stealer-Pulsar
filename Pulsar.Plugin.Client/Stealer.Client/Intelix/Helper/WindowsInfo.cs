// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.WindowsInfo
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Microsoft.Win32;
using System;

#nullable disable
namespace Intelix.Helper;

public static class WindowsInfo
{
  public static string GetProductName()
  {
    try
    {
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion"))
      {
        if (registryKey == null)
          return "Unknown";
        if (!(registryKey.GetValue("ProductName") is string str1))
          str1 = "Unknown";
        if (!(registryKey.GetValue("ReleaseId") is string str3))
          str3 = registryKey.GetValue("DisplayVersion") is string str2 ? str2 : "";
        return $"{str1} {str3}".Trim();
      }
    }
    catch
    {
      return "Unknown";
    }
  }

  public static string GetBuildNumber()
  {
    try
    {
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion"))
        return registryKey == null ? "Unknown" : registryKey.GetValue("CurrentBuild")?.ToString() ?? registryKey.GetValue("CurrentBuildNumber")?.ToString() ?? "Unknown";
    }
    catch
    {
      return "Unknown";
    }
  }

  public static string GetArchitecture()
  {
    try
    {
      return Environment.Is64BitOperatingSystem ? "x64" : "x86";
    }
    catch
    {
      return "Unknown";
    }
  }

  public static string GetVersion()
  {
    try
    {
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion"))
      {
        if (registryKey == null)
          return "Unknown";
        object obj1 = registryKey.GetValue("CurrentMajorVersionNumber");
        object obj2 = registryKey.GetValue("CurrentMinorVersionNumber");
        if (obj1 != null && obj2 != null)
          return $"{obj1}.{obj2}";
        string str = registryKey.GetValue("CurrentVersion") as string;
        return !string.IsNullOrEmpty(str) ? str : "Unknown";
      }
    }
    catch
    {
      return "Unknown";
    }
  }

  public static string GetFullInfo()
  {
    return $"OS Product: {WindowsInfo.GetProductName()}\nOS Build: {WindowsInfo.GetBuildNumber()}\nOS Arch: {WindowsInfo.GetArchitecture()}";
  }
}
