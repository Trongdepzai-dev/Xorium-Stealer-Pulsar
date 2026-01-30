// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.AesGcm256
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;

#nullable disable
namespace Intelix.Helper.Encrypted;

public class AesGcm256
{
  private static readonly byte[] SBox = new byte[256 /*0x0100*/]
  {
    (byte) 99,
    (byte) 124,
    (byte) 119,
    (byte) 123,
    (byte) 242,
    (byte) 107,
    (byte) 111,
    (byte) 197,
    (byte) 48 /*0x30*/,
    (byte) 1,
    (byte) 103,
    (byte) 43,
    (byte) 254,
    (byte) 215,
    (byte) 171,
    (byte) 118,
    (byte) 202,
    (byte) 130,
    (byte) 201,
    (byte) 125,
    (byte) 250,
    (byte) 89,
    (byte) 71,
    (byte) 240 /*0xF0*/,
    (byte) 173,
    (byte) 212,
    (byte) 162,
    (byte) 175,
    (byte) 156,
    (byte) 164,
    (byte) 114,
    (byte) 192 /*0xC0*/,
    (byte) 183,
    (byte) 253,
    (byte) 147,
    (byte) 38,
    (byte) 54,
    (byte) 63 /*0x3F*/,
    (byte) 247,
    (byte) 204,
    (byte) 52,
    (byte) 165,
    (byte) 229,
    (byte) 241,
    (byte) 113,
    (byte) 216,
    (byte) 49,
    (byte) 21,
    (byte) 4,
    (byte) 199,
    (byte) 35,
    (byte) 195,
    (byte) 24,
    (byte) 150,
    (byte) 5,
    (byte) 154,
    (byte) 7,
    (byte) 18,
    (byte) 128 /*0x80*/,
    (byte) 226,
    (byte) 235,
    (byte) 39,
    (byte) 178,
    (byte) 117,
    (byte) 9,
    (byte) 131,
    (byte) 44,
    (byte) 26,
    (byte) 27,
    (byte) 110,
    (byte) 90,
    (byte) 160 /*0xA0*/,
    (byte) 82,
    (byte) 59,
    (byte) 214,
    (byte) 179,
    (byte) 41,
    (byte) 227,
    (byte) 47,
    (byte) 132,
    (byte) 83,
    (byte) 209,
    (byte) 0,
    (byte) 237,
    (byte) 32 /*0x20*/,
    (byte) 252,
    (byte) 177,
    (byte) 91,
    (byte) 106,
    (byte) 203,
    (byte) 190,
    (byte) 57,
    (byte) 74,
    (byte) 76,
    (byte) 88,
    (byte) 207,
    (byte) 208 /*0xD0*/,
    (byte) 239,
    (byte) 170,
    (byte) 251,
    (byte) 67,
    (byte) 77,
    (byte) 51,
    (byte) 133,
    (byte) 69,
    (byte) 249,
    (byte) 2,
    (byte) 127 /*0x7F*/,
    (byte) 80 /*0x50*/,
    (byte) 60,
    (byte) 159,
    (byte) 168,
    (byte) 81,
    (byte) 163,
    (byte) 64 /*0x40*/,
    (byte) 143,
    (byte) 146,
    (byte) 157,
    (byte) 56,
    (byte) 245,
    (byte) 188,
    (byte) 182,
    (byte) 218,
    (byte) 33,
    (byte) 16 /*0x10*/,
    byte.MaxValue,
    (byte) 243,
    (byte) 210,
    (byte) 205,
    (byte) 12,
    (byte) 19,
    (byte) 236,
    (byte) 95,
    (byte) 151,
    (byte) 68,
    (byte) 23,
    (byte) 196,
    (byte) 167,
    (byte) 126,
    (byte) 61,
    (byte) 100,
    (byte) 93,
    (byte) 25,
    (byte) 115,
    (byte) 96 /*0x60*/,
    (byte) 129,
    (byte) 79,
    (byte) 220,
    (byte) 34,
    (byte) 42,
    (byte) 144 /*0x90*/,
    (byte) 136,
    (byte) 70,
    (byte) 238,
    (byte) 184,
    (byte) 20,
    (byte) 222,
    (byte) 94,
    (byte) 11,
    (byte) 219,
    (byte) 224 /*0xE0*/,
    (byte) 50,
    (byte) 58,
    (byte) 10,
    (byte) 73,
    (byte) 6,
    (byte) 36,
    (byte) 92,
    (byte) 194,
    (byte) 211,
    (byte) 172,
    (byte) 98,
    (byte) 145,
    (byte) 149,
    (byte) 228,
    (byte) 121,
    (byte) 231,
    (byte) 200,
    (byte) 55,
    (byte) 109,
    (byte) 141,
    (byte) 213,
    (byte) 78,
    (byte) 169,
    (byte) 108,
    (byte) 86,
    (byte) 244,
    (byte) 234,
    (byte) 101,
    (byte) 122,
    (byte) 174,
    (byte) 8,
    (byte) 186,
    (byte) 120,
    (byte) 37,
    (byte) 46,
    (byte) 28,
    (byte) 166,
    (byte) 180,
    (byte) 198,
    (byte) 232,
    (byte) 221,
    (byte) 116,
    (byte) 31 /*0x1F*/,
    (byte) 75,
    (byte) 189,
    (byte) 139,
    (byte) 138,
    (byte) 112 /*0x70*/,
    (byte) 62,
    (byte) 181,
    (byte) 102,
    (byte) 72,
    (byte) 3,
    (byte) 246,
    (byte) 14,
    (byte) 97,
    (byte) 53,
    (byte) 87,
    (byte) 185,
    (byte) 134,
    (byte) 193,
    (byte) 29,
    (byte) 158,
    (byte) 225,
    (byte) 248,
    (byte) 152,
    (byte) 17,
    (byte) 105,
    (byte) 217,
    (byte) 142,
    (byte) 148,
    (byte) 155,
    (byte) 30,
    (byte) 135,
    (byte) 233,
    (byte) 206,
    (byte) 85,
    (byte) 40,
    (byte) 223,
    (byte) 140,
    (byte) 161,
    (byte) 137,
    (byte) 13,
    (byte) 191,
    (byte) 230,
    (byte) 66,
    (byte) 104,
    (byte) 65,
    (byte) 153,
    (byte) 45,
    (byte) 15,
    (byte) 176 /*0xB0*/,
    (byte) 84,
    (byte) 187,
    (byte) 22
  };
  private static readonly byte[] Rcon = new byte[256 /*0x0100*/]
  {
    (byte) 0,
    (byte) 1,
    (byte) 2,
    (byte) 4,
    (byte) 8,
    (byte) 16 /*0x10*/,
    (byte) 32 /*0x20*/,
    (byte) 64 /*0x40*/,
    (byte) 128 /*0x80*/,
    (byte) 27,
    (byte) 54,
    (byte) 108,
    (byte) 216,
    (byte) 171,
    (byte) 77,
    (byte) 154,
    (byte) 47,
    (byte) 94,
    (byte) 188,
    (byte) 99,
    (byte) 198,
    (byte) 151,
    (byte) 53,
    (byte) 106,
    (byte) 212,
    (byte) 179,
    (byte) 125,
    (byte) 250,
    (byte) 239,
    (byte) 197,
    (byte) 145,
    (byte) 57,
    (byte) 114,
    (byte) 228,
    (byte) 211,
    (byte) 189,
    (byte) 97,
    (byte) 194,
    (byte) 159,
    (byte) 37,
    (byte) 74,
    (byte) 148,
    (byte) 51,
    (byte) 102,
    (byte) 204,
    (byte) 131,
    (byte) 29,
    (byte) 58,
    (byte) 116,
    (byte) 232,
    (byte) 203,
    (byte) 141,
    (byte) 1,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0,
    (byte) 0
  };
  private byte[] Key;
  private byte[,] RoundKeys;

  public AesGcm256(byte[] key)
  {
    if (key.Length != 32 /*0x20*/)
      throw new ArgumentException("Key length must be 256 bits.");
    this.Key = new byte[32 /*0x20*/];
    Array.Copy((Array) key, (Array) this.Key, 32 /*0x20*/);
    this.KeyExpansion();
  }

  public static byte[] Decrypt(
    byte[] key,
    byte[] iv,
    byte[] aad,
    byte[] cipherText,
    byte[] authTag)
  {
    return new AesGcm256(key).Decrypt(cipherText, authTag, iv, aad);
  }

  private void KeyExpansion()
  {
    int num1 = 8;
    int num2 = 4;
    int num3 = 14;
    this.RoundKeys = new byte[num2 * (num3 + 1), 4];
    for (int index = 0; index < num1; ++index)
    {
      this.RoundKeys[index, 0] = this.Key[4 * index];
      this.RoundKeys[index, 1] = this.Key[4 * index + 1];
      this.RoundKeys[index, 2] = this.Key[4 * index + 2];
      this.RoundKeys[index, 3] = this.Key[4 * index + 3];
    }
    byte[] numArray = new byte[4];
    for (int index = num1; index < num2 * (num3 + 1); ++index)
    {
      numArray[0] = this.RoundKeys[index - 1, 0];
      numArray[1] = this.RoundKeys[index - 1, 1];
      numArray[2] = this.RoundKeys[index - 1, 2];
      numArray[3] = this.RoundKeys[index - 1, 3];
      if (index % num1 == 0)
      {
        byte num4 = numArray[0];
        numArray[0] = numArray[1];
        numArray[1] = numArray[2];
        numArray[2] = numArray[3];
        numArray[3] = num4;
        numArray[0] = AesGcm256.SBox[(int) numArray[0]];
        numArray[1] = AesGcm256.SBox[(int) numArray[1]];
        numArray[2] = AesGcm256.SBox[(int) numArray[2]];
        numArray[3] = AesGcm256.SBox[(int) numArray[3]];
        numArray[0] ^= AesGcm256.Rcon[index / num1];
      }
      else if (num1 > 6 && index % num1 == 4)
      {
        numArray[0] = AesGcm256.SBox[(int) numArray[0]];
        numArray[1] = AesGcm256.SBox[(int) numArray[1]];
        numArray[2] = AesGcm256.SBox[(int) numArray[2]];
        numArray[3] = AesGcm256.SBox[(int) numArray[3]];
      }
      this.RoundKeys[index, 0] = (byte) ((uint) this.RoundKeys[index - num1, 0] ^ (uint) numArray[0]);
      this.RoundKeys[index, 1] = (byte) ((uint) this.RoundKeys[index - num1, 1] ^ (uint) numArray[1]);
      this.RoundKeys[index, 2] = (byte) ((uint) this.RoundKeys[index - num1, 2] ^ (uint) numArray[2]);
      this.RoundKeys[index, 3] = (byte) ((uint) this.RoundKeys[index - num1, 3] ^ (uint) numArray[3]);
    }
  }

  private void AddRoundKey(byte[,] state, int round)
  {
    for (int index1 = 0; index1 < 4; ++index1)
    {
      for (int index2 = 0; index2 < 4; ++index2)
        state[index2, index1] ^= this.RoundKeys[round * 4 + index1, index2];
    }
  }

  private void SubBytes(byte[,] state)
  {
    for (int index1 = 0; index1 < 4; ++index1)
    {
      for (int index2 = 0; index2 < 4; ++index2)
        state[index1, index2] = AesGcm256.SBox[(int) state[index1, index2]];
    }
  }

  private void ShiftRows(byte[,] state)
  {
    byte num1 = state[1, 0];
    state[1, 0] = state[1, 1];
    state[1, 1] = state[1, 2];
    state[1, 2] = state[1, 3];
    state[1, 3] = num1;
    byte num2 = state[2, 0];
    state[2, 0] = state[2, 2];
    state[2, 2] = num2;
    byte num3 = state[2, 1];
    state[2, 1] = state[2, 3];
    state[2, 3] = num3;
    byte num4 = state[3, 3];
    state[3, 3] = state[3, 2];
    state[3, 2] = state[3, 1];
    state[3, 1] = state[3, 0];
    state[3, 0] = num4;
  }

  private void MixColumns(byte[,] state)
  {
    byte[] numArray = new byte[4];
    for (int index = 0; index < 4; ++index)
    {
      numArray[0] = (byte) ((uint) this.GFMultiply((byte) 2, state[0, index]) ^ (uint) this.GFMultiply((byte) 3, state[1, index]) ^ (uint) state[2, index] ^ (uint) state[3, index]);
      numArray[1] = (byte) ((uint) state[0, index] ^ (uint) this.GFMultiply((byte) 2, state[1, index]) ^ (uint) this.GFMultiply((byte) 3, state[2, index]) ^ (uint) state[3, index]);
      numArray[2] = (byte) ((uint) state[0, index] ^ (uint) state[1, index] ^ (uint) this.GFMultiply((byte) 2, state[2, index]) ^ (uint) this.GFMultiply((byte) 3, state[3, index]));
      numArray[3] = (byte) ((uint) this.GFMultiply((byte) 3, state[0, index]) ^ (uint) state[1, index] ^ (uint) state[2, index] ^ (uint) this.GFMultiply((byte) 2, state[3, index]));
      state[0, index] = numArray[0];
      state[1, index] = numArray[1];
      state[2, index] = numArray[2];
      state[3, index] = numArray[3];
    }
  }

  private byte GFMultiply(byte a, byte b)
  {
    byte num1 = 0;
    for (int index = 0; index < 8; ++index)
    {
      if (((int) b & 1) != 0)
        num1 ^= a;
      int num2 = ((uint) a & 128U /*0x80*/) > 0U ? 1 : 0;
      a <<= 1;
      if (num2 != 0)
        a ^= (byte) 27;
      b >>= 1;
    }
    return num1;
  }

  private void EncryptBlock(byte[] input, byte[] output)
  {
    int length = 4;
    int round1 = 14;
    byte[,] state = new byte[4, length];
    for (int index = 0; index < 16 /*0x10*/; ++index)
      state[index % 4, index / 4] = input[index];
    this.AddRoundKey(state, 0);
    for (int round2 = 1; round2 <= round1 - 1; ++round2)
    {
      this.SubBytes(state);
      this.ShiftRows(state);
      this.MixColumns(state);
      this.AddRoundKey(state, round2);
    }
    this.SubBytes(state);
    this.ShiftRows(state);
    this.AddRoundKey(state, round1);
    for (int index = 0; index < 16 /*0x10*/; ++index)
      output[index] = state[index % 4, index / 4];
  }

  private byte[] GF128Multiply(byte[] X, byte[] Y)
  {
    byte[] numArray = new byte[16 /*0x10*/];
    byte[] destinationArray = new byte[16 /*0x10*/];
    Array.Copy((Array) Y, (Array) destinationArray, 16 /*0x10*/);
    for (int index1 = 0; index1 < 128 /*0x80*/; ++index1)
    {
      if (((int) X[index1 / 8] >> 7 - index1 % 8 & 1) == 1)
      {
        for (int index2 = 0; index2 < 16 /*0x10*/; ++index2)
          numArray[index2] ^= destinationArray[index2];
      }
      bool flag = ((int) destinationArray[15] & 1) == 1;
      for (int index3 = 15; index3 >= 0; --index3)
        destinationArray[index3] = (byte) ((int) destinationArray[index3] >> 1 | (index3 > 0 ? (int) destinationArray[index3 - 1] : 0) << 7);
      if (flag)
        destinationArray[0] ^= (byte) 225;
    }
    return numArray;
  }

  private byte[] GHASH(byte[] H, byte[] A, byte[] C)
  {
    int num1 = (A.Length + 15) / 16 /*0x10*/;
    int num2 = (C.Length + 15) / 16 /*0x10*/;
    byte[] numArray1 = new byte[16 /*0x10*/];
    byte[] X = new byte[16 /*0x10*/];
    byte[] destinationArray = new byte[16 /*0x10*/];
    int length1;
    for (int sourceIndex = 0; sourceIndex < A.Length; sourceIndex += length1)
    {
      Array.Clear((Array) destinationArray, 0, 16 /*0x10*/);
      length1 = Math.Min(16 /*0x10*/, A.Length - sourceIndex);
      Array.Copy((Array) A, sourceIndex, (Array) destinationArray, 0, length1);
      for (int index = 0; index < 16 /*0x10*/; ++index)
        X[index] = (byte) ((uint) numArray1[index] ^ (uint) destinationArray[index]);
      numArray1 = this.GF128Multiply(X, H);
    }
    int length2;
    for (int sourceIndex = 0; sourceIndex < C.Length; sourceIndex += length2)
    {
      Array.Clear((Array) destinationArray, 0, 16 /*0x10*/);
      length2 = Math.Min(16 /*0x10*/, C.Length - sourceIndex);
      Array.Copy((Array) C, sourceIndex, (Array) destinationArray, 0, length2);
      for (int index = 0; index < 16 /*0x10*/; ++index)
        X[index] = (byte) ((uint) numArray1[index] ^ (uint) destinationArray[index]);
      numArray1 = this.GF128Multiply(X, H);
    }
    byte[] numArray2 = new byte[16 /*0x10*/];
    ulong num3 = (ulong) A.Length * 8UL;
    ulong num4 = (ulong) C.Length * 8UL;
    for (int index = 0; index < 8; ++index)
    {
      numArray2[7 - index] = (byte) (num3 >> index * 8);
      numArray2[15 - index] = (byte) (num4 >> index * 8);
    }
    for (int index = 0; index < 16 /*0x10*/; ++index)
      X[index] = (byte) ((uint) numArray1[index] ^ (uint) numArray2[index]);
    return this.GF128Multiply(X, H);
  }

  private void IncrementCounter(byte[] counterBlock)
  {
    int index = 15;
    while (index >= 12 && ++counterBlock[index] == (byte) 0)
      --index;
  }

  public byte[] Decrypt(byte[] ciphertext, byte[] tag, byte[] iv, byte[] aad)
  {
    if (aad == null)
      aad = new byte[0];
    byte[] numArray1 = new byte[16 /*0x10*/];
    this.EncryptBlock(new byte[16 /*0x10*/], numArray1);
    byte[] numArray2 = new byte[16 /*0x10*/];
    if (iv.Length == 12)
    {
      Array.Copy((Array) iv, 0, (Array) numArray2, 0, 12);
      numArray2[15] = (byte) 1;
    }
    else
      numArray2 = this.GHASH(numArray1, (byte[]) null, iv);
    byte[] numArray3 = new byte[ciphertext.Length];
    byte[] numArray4 = new byte[16 /*0x10*/];
    Array.Copy((Array) numArray2, (Array) numArray4, 16 /*0x10*/);
    int num1 = ciphertext.Length / 16 /*0x10*/;
    int num2 = ciphertext.Length % 16 /*0x10*/;
    int num3 = num2 == 0 ? num1 : num1 + 1;
    for (int index1 = 0; index1 < num3; ++index1)
    {
      this.IncrementCounter(numArray4);
      byte[] output = new byte[16 /*0x10*/];
      this.EncryptBlock(numArray4, output);
      int num4 = index1 < num1 ? 16 /*0x10*/ : num2;
      for (int index2 = 0; index2 < num4; ++index2)
        numArray3[index1 * 16 /*0x10*/ + index2] = (byte) ((uint) ciphertext[index1 * 16 /*0x10*/ + index2] ^ (uint) output[index2]);
    }
    byte[] numArray5 = this.GHASH(numArray1, aad, ciphertext);
    byte[] output1 = new byte[16 /*0x10*/];
    this.EncryptBlock(numArray2, output1);
    byte[] tag2 = new byte[16 /*0x10*/];
    for (int index = 0; index < 16 /*0x10*/; ++index)
      tag2[index] = (byte) ((uint) output1[index] ^ (uint) numArray5[index]);
    if (!this.VerifyTag(tag, tag2))
      throw new Exception("Authentication tag does not match. Decryption failed.");
    return numArray3;
  }

  private bool VerifyTag(byte[] tag1, byte[] tag2)
  {
    if (tag1.Length != tag2.Length)
      return false;
    int num = 0;
    for (int index = 0; index < tag1.Length; ++index)
      num |= (int) tag1[index] ^ (int) tag2[index];
    return num == 0;
  }
}
