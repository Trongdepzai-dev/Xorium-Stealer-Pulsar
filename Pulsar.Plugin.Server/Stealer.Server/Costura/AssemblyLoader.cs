// Decompiled with JetBrains decompiler
// Type: Costura.AssemblyLoader
// Assembly: Stealer.Server, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D55D4852-5F7A-42EA-B9D3-2DEEE645AD6A
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Server.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading;

#nullable disable
namespace Costura;

[CompilerGenerated]
internal static class AssemblyLoader
{
  private static object nullCacheLock = new object();
  private static Dictionary<string, bool> nullCache = new Dictionary<string, bool>();
  private static Dictionary<string, string> assemblyNames = new Dictionary<string, string>();
  private static Dictionary<string, string> symbolNames = new Dictionary<string, string>();
  private static int isAttached;

  private static string CultureToString(CultureInfo culture)
  {
    return culture == null ? string.Empty : culture.Name;
  }

  private static Assembly ReadExistingAssembly(AssemblyName name)
  {
    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
      AssemblyName name1 = assembly.GetName();
      // ISSUE: reference to a compiler-generated method
      // ISSUE: reference to a compiler-generated method
      if (string.Equals(name1.Name, name.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals(AssemblyLoader.CultureToString(name1.CultureInfo), AssemblyLoader.CultureToString(name.CultureInfo), StringComparison.InvariantCultureIgnoreCase))
        return assembly;
    }
    return (Assembly) null;
  }

  private static string GetAssemblyResourceName(AssemblyName requestedAssemblyName)
  {
    string lowerInvariant = requestedAssemblyName.Name.ToLowerInvariant();
    if (requestedAssemblyName.CultureInfo != null && !string.IsNullOrEmpty(requestedAssemblyName.CultureInfo.Name))
    {
      // ISSUE: reference to a compiler-generated method
      lowerInvariant = $"{AssemblyLoader.CultureToString(requestedAssemblyName.CultureInfo)}.{lowerInvariant}".ToLowerInvariant();
    }
    return lowerInvariant;
  }

  private static void CopyTo(Stream source, Stream destination)
  {
    byte[] numArray = new byte[81920 /*0x014000*/];
    int num;
    while ((num = source.Read(numArray, 0, numArray.Length)) != 0)
      destination.Write(numArray, 0, num);
  }

  private static Stream LoadStream(string fullName)
  {
    Assembly executingAssembly = Assembly.GetExecutingAssembly();
    if (!fullName.EndsWith(".compressed"))
      return executingAssembly.GetManifestResourceStream(fullName);
    using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(fullName))
    {
      using (DeflateStream source = new DeflateStream(manifestResourceStream, CompressionMode.Decompress))
      {
        MemoryStream destination = new MemoryStream();
        // ISSUE: reference to a compiler-generated method
        AssemblyLoader.CopyTo((Stream) source, (Stream) destination);
        ((Stream) destination).Position = 0L;
        return (Stream) destination;
      }
    }
  }

  private static Stream LoadStream(Dictionary<string, string> resourceNames, string name)
  {
    string fullName;
    // ISSUE: reference to a compiler-generated method
    return resourceNames.TryGetValue(name, out fullName) ? AssemblyLoader.LoadStream(fullName) : (Stream) null;
  }

  private static byte[] ReadStream(Stream stream)
  {
    byte[] numArray = new byte[stream.Length];
    stream.Read(numArray, 0, numArray.Length);
    return numArray;
  }

  private static Assembly ReadFromEmbeddedResources(
    Dictionary<string, string> assemblyNames,
    Dictionary<string, string> symbolNames,
    AssemblyName requestedAssemblyName)
  {
    // ISSUE: reference to a compiler-generated method
    string assemblyResourceName = AssemblyLoader.GetAssemblyResourceName(requestedAssemblyName);
    byte[] numArray1;
    // ISSUE: reference to a compiler-generated method
    using (Stream stream = AssemblyLoader.LoadStream(assemblyNames, assemblyResourceName))
    {
      if (stream == null)
        return (Assembly) null;
      // ISSUE: reference to a compiler-generated method
      numArray1 = AssemblyLoader.ReadStream(stream);
    }
    // ISSUE: reference to a compiler-generated method
    using (Stream stream = AssemblyLoader.LoadStream(symbolNames, assemblyResourceName))
    {
      if (stream != null)
      {
        // ISSUE: reference to a compiler-generated method
        byte[] numArray2 = AssemblyLoader.ReadStream(stream);
        return Assembly.Load(numArray1, numArray2);
      }
    }
    return Assembly.Load(numArray1);
  }

  public static Assembly ResolveAssembly(
    AssemblyLoadContext assemblyLoadContext,
    AssemblyName assemblyName)
  {
    string name = assemblyName.Name;
    // ISSUE: reference to a compiler-generated field
    lock (AssemblyLoader.nullCacheLock)
    {
      // ISSUE: reference to a compiler-generated field
      if (AssemblyLoader.nullCache.ContainsKey(name))
        return (Assembly) null;
    }
    // ISSUE: reference to a compiler-generated method
    Assembly assembly1 = AssemblyLoader.ReadExistingAssembly(assemblyName);
    if (assembly1 != null)
      return assembly1;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated method
    Assembly assembly2 = AssemblyLoader.ReadFromEmbeddedResources(AssemblyLoader.assemblyNames, AssemblyLoader.symbolNames, assemblyName);
    if (assembly2 == null)
    {
      // ISSUE: reference to a compiler-generated field
      lock (AssemblyLoader.nullCacheLock)
      {
        // ISSUE: reference to a compiler-generated field
        AssemblyLoader.nullCache[name] = true;
      }
      if ((assemblyName.Flags & AssemblyNameFlags.Retargetable) != AssemblyNameFlags.None)
        assembly2 = Assembly.Load(assemblyName);
    }
    return assembly2;
  }

  static AssemblyLoader()
  {
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("bouncycastle.crypto", "costura.bouncycastle.crypto.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("discordrpc", "costura.discordrpc.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("dnlib", "costura.dnlib.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("glwpfcontrol", "costura.glwpfcontrol.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("gma.system.mousekeyhook", "costura.gma.system.mousekeyhook.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("livechartscore.skiasharpview.wpf", "costura.livechartscore.skiasharpview.wpf.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("messagepack.annotations", "costura.messagepack.annotations.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("messagepack", "costura.messagepack.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("microsoft.data.sqlite", "costura.microsoft.data.sqlite.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("microsoft.net.stringtools", "costura.microsoft.net.stringtools.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("mono.cecil", "costura.mono.cecil.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("naudio.core", "costura.naudio.core.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("naudio.winforms", "costura.naudio.winforms.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("naudio.winmm", "costura.naudio.winmm.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("newtonsoft.json", "costura.newtonsoft.json.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("open.nat", "costura.open.nat.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("opentk", "costura.opentk.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("pulsar.common", "costura.pulsar.common.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("pulsar", "costura.pulsar.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("skiasharp", "costura.skiasharp.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("skiasharp.views.desktop.common", "costura.skiasharp.views.desktop.common.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("skiasharp.views.wpf", "costura.skiasharp.views.wpf.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("sqlitepclraw.core", "costura.sqlitepclraw.core.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("vestris.resourcelib", "costura.vestris.resourcelib.dll.compressed");
  }

  public static void Attach(bool subscribe)
  {
    // ISSUE: reference to a compiler-generated field
    if (Interlocked.Exchange(ref AssemblyLoader.isAttached, 1) == 1 || !subscribe)
      return;
    AssemblyLoadContext.Default.Resolving += (Func<AssemblyLoadContext, AssemblyName, Assembly>) ((assemblyLoadContext, assemblyName) =>
    {
      string name = assemblyName.Name;
      // ISSUE: reference to a compiler-generated field
      lock (AssemblyLoader.nullCacheLock)
      {
        // ISSUE: reference to a compiler-generated field
        if (AssemblyLoader.nullCache.ContainsKey(name))
          return (Assembly) null;
      }
      // ISSUE: reference to a compiler-generated method
      Assembly assembly1 = AssemblyLoader.ReadExistingAssembly(assemblyName);
      if (assembly1 != null)
        return assembly1;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated method
      Assembly assembly2 = AssemblyLoader.ReadFromEmbeddedResources(AssemblyLoader.assemblyNames, AssemblyLoader.symbolNames, assemblyName);
      if (assembly2 == null)
      {
        // ISSUE: reference to a compiler-generated field
        lock (AssemblyLoader.nullCacheLock)
        {
          // ISSUE: reference to a compiler-generated field
          AssemblyLoader.nullCache[name] = true;
        }
        if ((assemblyName.Flags & AssemblyNameFlags.Retargetable) != AssemblyNameFlags.None)
          assembly2 = Assembly.Load(assemblyName);
      }
      return assembly2;
    });
  }
}
