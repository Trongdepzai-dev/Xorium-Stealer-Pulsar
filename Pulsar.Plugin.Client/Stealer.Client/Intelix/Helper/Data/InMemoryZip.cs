// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Data.InMemoryZip
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

#nullable disable
namespace Intelix.Helper.Data;

public sealed class InMemoryZip : IDisposable
{
  private readonly ConcurrentDictionary<string, byte[]> _entries = new ConcurrentDictionary<string, byte[]>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
  private readonly object _buildLock = new object();
  private bool _disposed;

  public int Count => this._entries.Count;

  private static string NormalizeEntryName(string name)
  {
    name = !string.IsNullOrWhiteSpace(name) ? name.Replace('\\', '/').Trim('/') : throw new ArgumentException("Entry name is null or empty", nameof (name));
    return name.Length != 0 ? name : throw new ArgumentException("Invalid entry name", nameof (name));
  }

  public void AddFile(string entryPath, byte[] content)
  {
    if (this._disposed)
      throw new ObjectDisposedException(nameof (InMemoryZip));
    if (content == null || content.Length == 0)
      return;
    string key = InMemoryZip.NormalizeEntryName(entryPath);
    byte[] copy = new byte[content.Length];
    Buffer.BlockCopy((Array) content, 0, (Array) copy, 0, content.Length);
    this._entries.AddOrUpdate(key, copy, (Func<string, byte[], byte[]>) ((text, old) => copy));
  }

  public void AddTextFile(string entryPath, string text)
  {
    if (string.IsNullOrEmpty(text))
      return;
    this.AddFile(entryPath, Encoding.UTF8.GetBytes(text));
  }

  public void AddDirectoryFiles(
    string sourceDirectory,
    string targetEntryDirectory = "",
    bool recursive = true)
  {
    if (this._disposed)
      throw new ObjectDisposedException(nameof (InMemoryZip));
    if (string.IsNullOrEmpty(sourceDirectory))
      throw new ArgumentException(nameof (sourceDirectory));
    if (!Directory.Exists(sourceDirectory))
      return;
    SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
    foreach (string file in Directory.GetFiles(sourceDirectory, "*", searchOption))
    {
      string path2 = file.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      string entryPath = (string.IsNullOrEmpty(targetEntryDirectory) ? path2 : Path.Combine(targetEntryDirectory, path2)).Replace('\\', '/');
      try
      {
        byte[] content = File.ReadAllBytes(file);
        this.AddFile(entryPath, content);
      }
      catch
      {
      }
    }
  }

  public byte[] ToArray(CompressionLevel compression = CompressionLevel.Fastest)
  {
    if (this._disposed)
      throw new ObjectDisposedException(nameof (InMemoryZip));
    lock (this._buildLock)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (ZipArchive zipArchive = new ZipArchive((Stream) memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
          foreach (KeyValuePair<string, byte[]> entry in this._entries)
          {
            using (Stream stream = zipArchive.CreateEntry(entry.Key, compression).Open())
            {
              byte[] buffer = entry.Value;
              stream.Write(buffer, 0, buffer.Length);
            }
          }
        }
        return memoryStream.ToArray();
      }
    }
  }

  public void Clear() => this._entries.Clear();

  public void Dispose()
  {
    if (this._disposed)
      return;
    this._disposed = true;
    this._entries.Clear();
  }
}
