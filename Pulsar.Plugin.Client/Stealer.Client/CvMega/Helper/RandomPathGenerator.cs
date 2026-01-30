// Decompiled with JetBrains decompiler
// Type: CvMega.Helper.RandomPathGenerator
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Text;

#nullable disable
namespace CvMega.Helper;

internal class RandomPathGenerator
{
  private static readonly Random random = new Random();
  private static readonly string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
  private static readonly string[] extensions = new string[7]
  {
    "",
    ".txt",
    ".png",
    ".jpg",
    ".html",
    ".json",
    ""
  };

  public static string GenerateCompletelyRandomPath()
  {
    int num = RandomPathGenerator.random.Next(1, 11);
    StringBuilder stringBuilder = new StringBuilder();
    for (int index = 0; index < num; ++index)
    {
      int length = RandomPathGenerator.random.Next(3, 20);
      stringBuilder.Append('/');
      stringBuilder.Append(RandomPathGenerator.GenerateRandomString(length));
    }
    if (RandomPathGenerator.random.Next(2) == 1)
      stringBuilder.Append(RandomPathGenerator.extensions[RandomPathGenerator.random.Next(RandomPathGenerator.extensions.Length)]);
    return stringBuilder.ToString();
  }

  private static string GenerateRandomString(int length)
  {
    StringBuilder stringBuilder = new StringBuilder(length);
    for (int index = 0; index < length; ++index)
      stringBuilder.Append(RandomPathGenerator.chars[RandomPathGenerator.random.Next(RandomPathGenerator.chars.Length)]);
    return stringBuilder.ToString();
  }
}
