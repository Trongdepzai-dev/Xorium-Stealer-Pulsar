// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.IpApi
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System.Net;

#nullable disable
namespace Intelix.Helper;

public static class IpApi
{
  private static string _cachedIp;
  private static readonly object _lock = new object();

  public static string GetPublicIp()
  {
    if (!string.IsNullOrEmpty(IpApi._cachedIp))
      return IpApi._cachedIp;
    lock (IpApi._lock)
    {
      if (!string.IsNullOrEmpty(IpApi._cachedIp))
        return IpApi._cachedIp;
      try
      {
        using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
        {
          string str = client.GetStringAsync("http://icanhazip.com").Result;
          if (!string.IsNullOrEmpty(str))
            IpApi._cachedIp = str.Trim();
        }
      }
      catch
      {
        IpApi._cachedIp = "Request failed";
      }
      return IpApi._cachedIp;
    }
  }
}
