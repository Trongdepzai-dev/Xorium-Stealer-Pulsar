// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.Asn1Der
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;

#nullable disable
namespace Intelix.Helper.Encrypted;

public class Asn1Der
{
  public Asn1DerObject Parse(byte[] toParse)
  {
    Asn1DerObject asn1DerObject = new Asn1DerObject();
    for (int index = 0; index < toParse.Length; ++index)
    {
      switch ((Asn1Der.Type) toParse[index])
      {
        case Asn1Der.Type.Integer:
          asn1DerObject.Objects.Add(new Asn1DerObject()
          {
            Type = Asn1Der.Type.Integer,
            Lenght = (int) toParse[index + 1]
          });
          byte[] destinationArray1 = new byte[(int) toParse[index + 1]];
          int length1 = index + 2 + (int) toParse[index + 1] > toParse.Length ? toParse.Length - (index + 2) : (int) toParse[index + 1];
          Array.Copy((Array) toParse, index + 2, (Array) destinationArray1, 0, length1);
          Asn1DerObject[] array1 = asn1DerObject.Objects.ToArray();
          asn1DerObject.Objects[array1.Length - 1].Data = destinationArray1;
          index = index + 1 + asn1DerObject.Objects[array1.Length - 1].Lenght;
          break;
        case Asn1Der.Type.OctetString:
          asn1DerObject.Objects.Add(new Asn1DerObject()
          {
            Type = Asn1Der.Type.OctetString,
            Lenght = (int) toParse[index + 1]
          });
          byte[] destinationArray2 = new byte[(int) toParse[index + 1]];
          int length2 = index + 2 + (int) toParse[index + 1] > toParse.Length ? toParse.Length - (index + 2) : (int) toParse[index + 1];
          Array.Copy((Array) toParse, index + 2, (Array) destinationArray2, 0, length2);
          Asn1DerObject[] array2 = asn1DerObject.Objects.ToArray();
          asn1DerObject.Objects[array2.Length - 1].Data = destinationArray2;
          index = index + 1 + asn1DerObject.Objects[array2.Length - 1].Lenght;
          break;
        case Asn1Der.Type.ObjectIdentifier:
          asn1DerObject.Objects.Add(new Asn1DerObject()
          {
            Type = Asn1Der.Type.ObjectIdentifier,
            Lenght = (int) toParse[index + 1]
          });
          byte[] destinationArray3 = new byte[(int) toParse[index + 1]];
          int length3 = index + 2 + (int) toParse[index + 1] > toParse.Length ? toParse.Length - (index + 2) : (int) toParse[index + 1];
          Array.Copy((Array) toParse, index + 2, (Array) destinationArray3, 0, length3);
          Asn1DerObject[] array3 = asn1DerObject.Objects.ToArray();
          asn1DerObject.Objects[array3.Length - 1].Data = destinationArray3;
          index = index + 1 + asn1DerObject.Objects[array3.Length - 1].Lenght;
          break;
        case Asn1Der.Type.Sequence:
          byte[] numArray;
          if (asn1DerObject.Lenght == 0)
          {
            asn1DerObject.Type = Asn1Der.Type.Sequence;
            asn1DerObject.Lenght = toParse.Length - (index + 2);
            numArray = new byte[asn1DerObject.Lenght];
          }
          else
          {
            asn1DerObject.Objects.Add(new Asn1DerObject()
            {
              Type = Asn1Der.Type.Sequence,
              Lenght = (int) toParse[index + 1]
            });
            numArray = new byte[(int) toParse[index + 1]];
          }
          int length4 = numArray.Length > toParse.Length - (index + 2) ? toParse.Length - (index + 2) : numArray.Length;
          Array.Copy((Array) toParse, index + 2, (Array) numArray, 0, length4);
          asn1DerObject.Objects.Add(this.Parse(numArray));
          index = index + 1 + (int) toParse[index + 1];
          break;
      }
    }
    return asn1DerObject;
  }

  public enum Type
  {
    Integer = 2,
    OctetString = 4,
    ObjectIdentifier = 6,
    Sequence = 48, // 0x00000030
  }
}
