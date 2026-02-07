use winapi::shared::ntdef::PVOID;
use winapi::shared::basetsd::ULONG_PTR;

// Helper to access PEB (Process Environment Block)
#[inline(always)]
unsafe fn get_peb() -> PVOID {
    let peb: PVOID;
    // On x64, GS:[0x60] points directly to PEB
    std::arch::asm!(
        "mov {}, qword ptr gs:[0x60]", 
        out(reg) peb
    );
    peb
}

/// Unlinks the current module from the PEB Loader Data lists.
pub fn unlink_peb() -> bool {
    unsafe {
        let peb = get_peb();
        if peb.is_null() { return false; }

        // PEB -> Ldr (Offset 0x18 on x64)
        // Use manual pointer arithmetic to avoid missing PEB struct fields
        let ldr_ptr = peb.wrapping_add(0x18) as *mut PVOID;
        if ldr_ptr.is_null() { return false; }
        
        let ldr = *ldr_ptr;
        if ldr.is_null() { return false; }

        // Ldr -> InLoadOrderModuleList (Offset 0x10 on x64)
        // This is a LIST_ENTRY { Flink, Blink }
        let head = ldr.wrapping_add(0x10) as *mut ULONG_PTR;
        if head.is_null() { return false; }
        
        let current = *head as *mut ULONG_PTR;
        if current.is_null() || current == head { return false; }

        // LIST_ENTRY unlinking
        let next = *current as *mut ULONG_PTR;
        let prev = *(current.wrapping_add(1)) as *mut ULONG_PTR;

        if !prev.is_null() {
            *prev = next as ULONG_PTR;
        }
        if !next.is_null() {
            *(next.wrapping_add(1)) = prev as ULONG_PTR;
        }

        true
    }
}
