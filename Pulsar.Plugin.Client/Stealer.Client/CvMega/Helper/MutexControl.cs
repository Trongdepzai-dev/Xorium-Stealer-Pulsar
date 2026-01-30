// Decompiled with JetBrains decompiler
// Type: CvMega.Helper.MutexControl
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System.Threading;

#nullable disable
namespace CvMega.Helper;

public static class MutexControl
{
  public static Mutex currentApp;
  public static bool createdNew;

  public static bool CreateMutex(string mtx)
  {
    MutexControl.currentApp = new Mutex(false, mtx, out MutexControl.createdNew);
    return MutexControl.createdNew;
  }
}
