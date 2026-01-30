// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.ChaCha20Poly1305
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Numerics;
using System.Security.Cryptography;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class ChaCha20Poly1305
{
  public static byte[] Decrypt(
    byte[] key32,
    byte[] iv12,
    byte[] ciphertext,
    byte[] tag,
    byte[] aad = null)
  {
    if (key32 == null)
      throw new ArgumentNullException(nameof (key32));
    if (key32.Length != 32 /*0x20*/)
      throw new ArgumentException("Key must be 32 bytes", nameof (key32));
    if (iv12 == null)
      throw new ArgumentNullException(nameof (iv12));
    if (iv12.Length != 12)
      throw new ArgumentException("IV must be 12 bytes", nameof (iv12));
    if (ciphertext == null)
      throw new ArgumentNullException(nameof (ciphertext));
    if (tag == null)
      throw new ArgumentNullException(nameof (tag));
    if (tag.Length != 16 /*0x10*/)
      throw new ArgumentException("Tag must be 16 bytes", nameof (tag));
    if (aad == null)
      aad = Array.Empty<byte>();
    byte[] src = ChaCha20Poly1305.ChaCha20Block(key32, 0U, iv12);
    byte[] numArray = new byte[32 /*0x20*/];
    Buffer.BlockCopy((Array) src, 0, (Array) numArray, 0, 32 /*0x20*/);
    byte[] msg = ChaCha20Poly1305.BuildPoly1305Message(aad, ciphertext);
    if (!ChaCha20Poly1305.FixedTimeEquals(ChaCha20Poly1305.Poly1305TagWithBigInteger(numArray, msg), tag))
    {
      Array.Clear((Array) src, 0, src.Length);
      Array.Clear((Array) numArray, 0, numArray.Length);
      throw new CryptographicException("ChaCha20-Poly1305 authentication failed (tag mismatch).");
    }
    byte[] output = new byte[ciphertext.Length];
    ChaCha20Poly1305.ChaCha20Xor(key32, 1U, iv12, ciphertext, output);
    Array.Clear((Array) src, 0, src.Length);
    Array.Clear((Array) numArray, 0, numArray.Length);
    return output;
  }

  private static byte[] ChaCha20Block(byte[] key32, uint counter, byte[] nonce12)
  {
    uint[] sourceArray = new uint[16 /*0x10*/]
    {
      1634760805U,
      857760878U,
      2036477234U,
      1797285236U,
      0U,
      0U,
      0U,
      0U,
      0U,
      0U,
      0U,
      0U,
      0U,
      0U,
      0U,
      0U
    };
    for (int index = 0; index < 8; ++index)
      sourceArray[4 + index] = ChaCha20Poly1305.ToUInt32Little(key32, index * 4);
    sourceArray[12] = counter;
    sourceArray[13] = ChaCha20Poly1305.ToUInt32Little(nonce12, 0);
    sourceArray[14] = ChaCha20Poly1305.ToUInt32Little(nonce12, 4);
    sourceArray[15] = ChaCha20Poly1305.ToUInt32Little(nonce12, 8);
    uint[] destinationArray = new uint[16 /*0x10*/];
    Array.Copy((Array) sourceArray, (Array) destinationArray, 16 /*0x10*/);
    for (int index = 0; index < 10; ++index)
    {
      ChaCha20Poly1305.QuarterRound(ref destinationArray[0], ref destinationArray[4], ref destinationArray[8], ref destinationArray[12]);
      ChaCha20Poly1305.QuarterRound(ref destinationArray[1], ref destinationArray[5], ref destinationArray[9], ref destinationArray[13]);
      ChaCha20Poly1305.QuarterRound(ref destinationArray[2], ref destinationArray[6], ref destinationArray[10], ref destinationArray[14]);
      ChaCha20Poly1305.QuarterRound(ref destinationArray[3], ref destinationArray[7], ref destinationArray[11], ref destinationArray[15]);
      ChaCha20Poly1305.QuarterRound(ref destinationArray[0], ref destinationArray[5], ref destinationArray[10], ref destinationArray[15]);
      ChaCha20Poly1305.QuarterRound(ref destinationArray[1], ref destinationArray[6], ref destinationArray[11], ref destinationArray[12]);
      ChaCha20Poly1305.QuarterRound(ref destinationArray[2], ref destinationArray[7], ref destinationArray[8], ref destinationArray[13]);
      ChaCha20Poly1305.QuarterRound(ref destinationArray[3], ref destinationArray[4], ref destinationArray[9], ref destinationArray[14]);
    }
    byte[] outbuf = new byte[64 /*0x40*/];
    for (int index = 0; index < 16 /*0x10*/; ++index)
      ChaCha20Poly1305.LittleEndian(destinationArray[index] + sourceArray[index], outbuf, index * 4);
    return outbuf;
  }

  private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
  {
    a += b;
    d ^= a;
    d = ChaCha20Poly1305.Rol(d, 16 /*0x10*/);
    c += d;
    b ^= c;
    b = ChaCha20Poly1305.Rol(b, 12);
    a += b;
    d ^= a;
    d = ChaCha20Poly1305.Rol(d, 8);
    c += d;
    b ^= c;
    b = ChaCha20Poly1305.Rol(b, 7);
  }

  private static uint Rol(uint x, int n) => x << n | x >> 32 /*0x20*/ - n;

  private static uint ToUInt32Little(byte[] bs, int off)
  {
    return (uint) ((int) bs[off] | (int) bs[off + 1] << 8 | (int) bs[off + 2] << 16 /*0x10*/ | (int) bs[off + 3] << 24);
  }

  private static void LittleEndian(uint v, byte[] outbuf, int off)
  {
    outbuf[off] = (byte) (v & (uint) byte.MaxValue);
    outbuf[off + 1] = (byte) (v >> 8 & (uint) byte.MaxValue);
    outbuf[off + 2] = (byte) (v >> 16 /*0x10*/ & (uint) byte.MaxValue);
    outbuf[off + 3] = (byte) (v >> 24 & (uint) byte.MaxValue);
  }

  private static void ChaCha20Xor(
    byte[] key,
    uint counter,
    byte[] nonce,
    byte[] input,
    byte[] output)
  {
    if (input == null || input.Length == 0)
      return;
    int num1 = 0;
    uint counter1 = counter;
    int num2;
    for (; num1 < input.Length; num1 += num2)
    {
      byte[] numArray = ChaCha20Poly1305.ChaCha20Block(key, counter1, nonce);
      ++counter1;
      num2 = Math.Min(64 /*0x40*/, input.Length - num1);
      for (int index = 0; index < num2; ++index)
        output[num1 + index] = (byte) ((uint) input[num1 + index] ^ (uint) numArray[index]);
    }
  }

  private static byte[] BuildPoly1305Message(byte[] aad, byte[] ciphertext)
  {
    int length1 = aad != null ? aad.Length : 0;
    int length2 = ciphertext != null ? ciphertext.Length : 0;
    int num1 = (16 /*0x10*/ - length1 % 16 /*0x10*/) % 16 /*0x10*/;
    int num2 = (16 /*0x10*/ - length2 % 16 /*0x10*/) % 16 /*0x10*/;
    byte[] dst1 = new byte[length1 + num1 + length2 + num2 + 8 + 8];
    int dstOffset1 = 0;
    if (length1 > 0)
    {
      Buffer.BlockCopy((Array) aad, 0, (Array) dst1, dstOffset1, length1);
      dstOffset1 += length1;
    }
    if (num1 > 0)
      dstOffset1 += num1;
    if (length2 > 0)
    {
      Buffer.BlockCopy((Array) ciphertext, 0, (Array) dst1, dstOffset1, length2);
      dstOffset1 += length2;
    }
    if (num2 > 0)
      dstOffset1 += num2;
    byte[] bytes1 = BitConverter.GetBytes((ulong) length1);
    byte[] bytes2 = BitConverter.GetBytes((ulong) length2);
    byte[] dst2 = dst1;
    int dstOffset2 = dstOffset1;
    Buffer.BlockCopy((Array) bytes1, 0, (Array) dst2, dstOffset2, 8);
    int dstOffset3 = dstOffset1 + 8;
    Buffer.BlockCopy((Array) bytes2, 0, (Array) dst1, dstOffset3, 8);
    int num3 = dstOffset3 + 8;
    return dst1;
  }

  private static byte[] Poly1305TagWithBigInteger(byte[] oneTimeKey32, byte[] msg)
  {
    byte[] numArray1 = new byte[16 /*0x10*/];
    Buffer.BlockCopy((Array) oneTimeKey32, 0, (Array) numArray1, 0, 16 /*0x10*/);
    numArray1[3] &= (byte) 15;
    numArray1[7] &= (byte) 15;
    numArray1[11] &= (byte) 15;
    numArray1[15] &= (byte) 15;
    numArray1[4] &= (byte) 252;
    numArray1[8] &= (byte) 252;
    numArray1[12] &= (byte) 252;
    byte[] numArray2 = new byte[16 /*0x10*/];
    Buffer.BlockCopy((Array) oneTimeKey32, 16 /*0x10*/, (Array) numArray2, 0, 16 /*0x10*/);
    BigInteger bigInteger1 = new BigInteger(ChaCha20Poly1305.AppendZero(numArray1));
    BigInteger bigInteger2 = new BigInteger(ChaCha20Poly1305.AppendZero(numArray2));
    BigInteger bigInteger3 = (BigInteger.One << 130) - (BigInteger) 5;
    BigInteger bigInteger4 = BigInteger.Zero;
    for (int srcOffset = 0; srcOffset < msg.Length; srcOffset += 16 /*0x10*/)
    {
      int count = Math.Min(16 /*0x10*/, msg.Length - srcOffset);
      byte[] numArray3 = new byte[count];
      Buffer.BlockCopy((Array) msg, srcOffset, (Array) numArray3, 0, count);
      BigInteger bigInteger5 = new BigInteger(ChaCha20Poly1305.AppendZero(numArray3));
      BigInteger bigInteger6 = BigInteger.One << 8 * count;
      bigInteger5 += bigInteger6;
      bigInteger4 = (bigInteger4 + bigInteger5) * bigInteger1 % bigInteger3;
    }
    byte[] byteArray = (bigInteger4 + bigInteger2).ToByteArray();
    byte[] numArray4 = new byte[16 /*0x10*/];
    for (int index = 0; index < 16 /*0x10*/ && index < byteArray.Length; ++index)
      numArray4[index] = byteArray[index];
    return numArray4;
  }

  private static byte[] AppendZero(byte[] b)
  {
    byte[] dst = new byte[b.Length + 1];
    Buffer.BlockCopy((Array) b, 0, (Array) dst, 0, b.Length);
    dst[dst.Length - 1] = (byte) 0;
    return dst;
  }

  private static bool FixedTimeEquals(byte[] a, byte[] b)
  {
    if (a == null || b == null || a.Length != b.Length)
      return false;
    int num = 0;
    for (int index = 0; index < a.Length; ++index)
      num |= (int) a[index] ^ (int) b[index];
    return num == 0;
  }
}
