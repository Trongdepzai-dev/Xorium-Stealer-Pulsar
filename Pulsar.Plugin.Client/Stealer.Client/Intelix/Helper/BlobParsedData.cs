// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.BlobParsedData
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

#nullable disable
namespace Intelix.Helper;

public class BlobParsedData
{
  public byte Flag { get; set; }

  public byte[] Iv { get; set; }

  public byte[] Ciphertext { get; set; }

  public byte[] Tag { get; set; }

  public byte[] EncryptedAesKey { get; set; }
}
