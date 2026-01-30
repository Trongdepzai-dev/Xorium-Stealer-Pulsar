// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.RC4Crypt
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

#nullable disable
namespace Intelix.Helper.Encrypted;

public class RC4Crypt
{
  public static byte[] Decrypt(byte[] key, byte[] data)
  {
    if (key == null)
      return (byte[]) null;
    if (key.Length == 0)
      return (byte[]) null;
    if (data == null)
      return (byte[]) null;
    byte[] arr = new byte[256 /*0x0100*/];
    for (int index = 0; index < 256 /*0x0100*/; ++index)
      arr[index] = (byte) index;
    int b1 = 0;
    for (int a = 0; a < 256 /*0x0100*/; ++a)
    {
      b1 = b1 + (int) arr[a] + (int) key[a % key.Length] & (int) byte.MaxValue;
      RC4Crypt.Swap(arr, a, b1);
    }
    byte[] numArray = new byte[data.Length];
    int a1 = 0;
    int b2 = 0;
    for (int index = 0; index < data.Length; ++index)
    {
      a1 = a1 + 1 & (int) byte.MaxValue;
      b2 = b2 + (int) arr[a1] & (int) byte.MaxValue;
      RC4Crypt.Swap(arr, a1, b2);
      byte num = arr[(int) arr[a1] + (int) arr[b2] & (int) byte.MaxValue];
      numArray[index] = (byte) ((uint) data[index] ^ (uint) num);
    }
    return numArray;
  }

  private static void Swap(byte[] arr, int a, int b)
  {
    byte num = arr[a];
    arr[a] = arr[b];
    arr[b] = num;
  }
}
