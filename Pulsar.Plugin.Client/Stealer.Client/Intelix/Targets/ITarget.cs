// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.ITarget
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;

#nullable disable
namespace Intelix.Targets;

public interface ITarget
{
  void Collect(InMemoryZip zip, Counter counter);
}
