// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.ConcurrentLong
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System.Threading;

#nullable disable
namespace Intelix.Helper;

public struct ConcurrentLong(long initial)
{
  private long _value = initial;

  public long Value
  {
    get => Interlocked.Read(ref this._value);
    set => Interlocked.Exchange(ref this._value, value);
  }

  public static ConcurrentLong operator ++(ConcurrentLong x)
  {
    Interlocked.Increment(ref x._value);
    return x;
  }

  public static ConcurrentLong operator --(ConcurrentLong x)
  {
    Interlocked.Decrement(ref x._value);
    return x;
  }

  public static implicit operator long(ConcurrentLong x) => x.Value;

  public static implicit operator ConcurrentLong(long v) => new ConcurrentLong(v);

  public static ConcurrentLong operator +(ConcurrentLong x, long y)
  {
    Interlocked.Add(ref x._value, y);
    return x;
  }

  public static ConcurrentLong operator -(ConcurrentLong x, long y)
  {
    Interlocked.Add(ref x._value, -y);
    return x;
  }
}
