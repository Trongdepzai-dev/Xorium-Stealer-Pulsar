// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Hashing.PBKDF2
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Security.Cryptography;

#nullable disable
namespace Intelix.Helper.Hashing;

public class PBKDF2
{
  private readonly int _blockSize;
  private uint _blockIndex = 1;
  private byte[] _bufferBytes;
  private int _bufferStartIndex;
  private int _bufferEndIndex;

  private HMAC Algorithm { get; }

  private byte[] Salt { get; }

  private int IterationCount { get; }

  public PBKDF2(HMAC algorithm, byte[] password, byte[] salt, int iterations)
  {
    this.Algorithm = algorithm ?? throw new ArgumentNullException(nameof (algorithm), "Algorithm cannot be null.");
    HMAC algorithm1 = this.Algorithm;
    algorithm1.Key = password ?? throw new ArgumentNullException(nameof (password), "Password cannot be null.");
    this.Salt = salt ?? throw new ArgumentNullException(nameof (salt), "Salt cannot be null.");
    this.IterationCount = iterations;
    this._blockSize = this.Algorithm.HashSize / 8;
    this._bufferBytes = new byte[this._blockSize];
  }

  public byte[] GetBytes(int count)
  {
    byte[] dst = new byte[count];
    int dstOffset = 0;
    int count1 = this._bufferEndIndex - this._bufferStartIndex;
    if (count1 > 0)
    {
      if (count < count1)
      {
        Buffer.BlockCopy((Array) this._bufferBytes, this._bufferStartIndex, (Array) dst, 0, count);
        this._bufferStartIndex += count;
        return dst;
      }
      Buffer.BlockCopy((Array) this._bufferBytes, this._bufferStartIndex, (Array) dst, 0, count1);
      this._bufferStartIndex = this._bufferEndIndex = 0;
      dstOffset += count1;
    }
    for (; dstOffset < count; dstOffset += this._blockSize)
    {
      int count2 = count - dstOffset;
      this._bufferBytes = this.Func();
      if (count2 > this._blockSize)
      {
        Buffer.BlockCopy((Array) this._bufferBytes, 0, (Array) dst, dstOffset, this._blockSize);
      }
      else
      {
        Buffer.BlockCopy((Array) this._bufferBytes, 0, (Array) dst, dstOffset, count2);
        this._bufferStartIndex = count2;
        this._bufferEndIndex = this._blockSize;
        return dst;
      }
    }
    return dst;
  }

  private byte[] Func()
  {
    byte[] numArray1 = new byte[this.Salt.Length + 4];
    Buffer.BlockCopy((Array) this.Salt, 0, (Array) numArray1, 0, this.Salt.Length);
    Buffer.BlockCopy((Array) PBKDF2.GetBytesFromInt(this._blockIndex), 0, (Array) numArray1, this.Salt.Length, 4);
    byte[] hash = this.Algorithm.ComputeHash(numArray1);
    byte[] numArray2 = hash;
    for (int index1 = 2; index1 <= this.IterationCount; ++index1)
    {
      hash = this.Algorithm.ComputeHash(hash, 0, hash.Length);
      for (int index2 = 0; index2 < this._blockSize; ++index2)
        numArray2[index2] ^= hash[index2];
    }
    if (this._blockIndex == uint.MaxValue)
      throw new InvalidOperationException("Derived key too long.");
    ++this._blockIndex;
    return numArray2;
  }

  private static byte[] GetBytesFromInt(uint i)
  {
    byte[] bytes = BitConverter.GetBytes(i);
    if (!BitConverter.IsLittleEndian)
      return bytes;
    return new byte[4]
    {
      bytes[3],
      bytes[2],
      bytes[1],
      bytes[0]
    };
  }
}
