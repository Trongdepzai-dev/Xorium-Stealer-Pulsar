// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.RegistryParser
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Microsoft.Win32;
using System;
using System.Collections.Generic;

#nullable disable
namespace Intelix.Helper;

public static class RegistryParser
{
  public static List<string> ParseKey(RegistryKey key)
  {
    List<string> key1 = new List<string>();
    if (key == null)
      return key1;
    foreach (string valueName in key.GetValueNames())
    {
      object obj = key.GetValue(valueName);
      string str;
      switch (key.GetValueKind(valueName))
      {
        case RegistryValueKind.String:
        case RegistryValueKind.ExpandString:
          str = obj?.ToString() ?? "null";
          break;
        case RegistryValueKind.Binary:
          str = !(obj is byte[] numArray) ? "null" : BitConverter.ToString(numArray).Replace("-", "");
          break;
        case RegistryValueKind.DWord:
        case RegistryValueKind.QWord:
          str = obj.ToString();
          break;
        case RegistryValueKind.MultiString:
          str = !(obj is string[] strArray) ? "null" : string.Join(", ", strArray);
          break;
        default:
          str = obj?.ToString() ?? "null";
          break;
      }
      key1.Add($"{valueName}: {str}");
    }
    return key1;
  }
}
