// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.CpuInfo
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Microsoft.Win32;
using System;

#nullable disable
namespace Intelix.Helper;

public static class CpuInfo
{
  public static string GetName()
  {
    try
    {
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0"))
      {
        if (!(registryKey?.GetValue("ProcessorNameString") is string name))
          name = registryKey?.GetValue("VendorIdentifier") is string str ? str : "Unknown";
        return name;
      }
    }
    catch
    {
      return "Unknown";
    }
  }

  public static int GetLogicalCores()
  {
    try
    {
      return Environment.ProcessorCount;
    }
    catch
    {
      return 0;
    }
  }
}
