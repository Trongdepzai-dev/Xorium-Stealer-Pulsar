//! Anti-VM and Anti-Debug Module (Hardened)
//!
//! This module provides robust techniques to detect virtual machine environments
//! and kernel-level debuggers to evade analysis.
//!
//! TARGET: Windows Kernel Mode (x64)

use core::arch::asm;
use wdk_sys::{NTSTATUS, STATUS_SUCCESS, STATUS_ACCESS_DENIED};

use crate::error::{ShadowError, ShadowResult};

// Địa chỉ cố định của KUSER_SHARED_DATA trên Windows x64 (luôn map ở đây)
const KUSER_SHARED_DATA_ADDRESS: u64 = 0xFFFFF78000000000;
// Offset của trường KdDebuggerEnabled
const KD_DEBUGGER_ENABLED_OFFSET: u64 = 0x2D4;

/// Detects common hypervisor presence bit via CPUID.
#[inline(always)]
pub unsafe fn check_hypervisor_bit() -> bool {
    let mut ecx: u32;
    asm!(
        "mov eax, 1",
        "cpuid",
        lateout("ecx") ecx,
        out("eax") _,
        out("ebx") _,
        out("edx") _,
        options(nostack, nomem)
    );
    // Bit 31 of ECX
    (ecx >> 31) & 1 == 1
}

/// Checks specific vendor strings from CPUID Leaf 0x40000000.
/// Covers: Hyper-V, VMware, VirtualBox, KVM, QEMU, Xen, Parallels.
pub unsafe fn check_vendor_signatures() -> Option<&'static str> {
    let mut ebx: u32;
    let mut ecx: u32;
    let mut edx: u32;

    asm!(
        "mov eax, 0x40000000",
        "cpuid",
        lateout("ebx") ebx,
        lateout("ecx") ecx,
        lateout("edx") edx,
        out("eax") _,
        options(nostack, nomem)
    );

    let brand_bytes = [
        (ebx & 0xFF) as u8, ((ebx >> 8) & 0xFF) as u8, ((ebx >> 16) & 0xFF) as u8, ((ebx >> 24) & 0xFF) as u8,
        (ecx & 0xFF) as u8, ((ecx >> 8) & 0xFF) as u8, ((ecx >> 16) & 0xFF) as u8, ((ecx >> 24) & 0xFF) as u8,
        (edx & 0xFF) as u8, ((edx >> 8) & 0xFF) as u8, ((edx >> 16) & 0xFF) as u8, ((edx >> 24) & 0xFF) as u8,
    ];

    if let Ok(brand) = core::str::from_utf8(&brand_bytes) {
        if brand.contains("Microsoft Hv") { return Some("Hyper-V"); }
        if brand.contains("VMwareVMware") { return Some("VMware"); }
        if brand.contains("VBoxVBoxVBox") { return Some("VirtualBox"); }
        if brand.contains("KVMKVMKVM")    { return Some("KVM"); }
        if brand.contains("TCGTCGTCGTCG") { return Some("QEMU"); }
        if brand.contains("XenVMMXenVMM") { return Some("Xen"); }
        if brand.contains("prl hyperv")   { return Some("Parallels"); }
    }
    
    None
}

/// Advanced Timing Attack using RDTSCP (Serialized).
///
/// Measures the latency of a CPUID instruction (which causes a VM-Exit).
/// This is robust against Out-of-Order execution due to `lfence`.
pub unsafe fn check_timing_anomaly() -> bool {
    let mut total_cycles: u64 = 0;
    let iterations = 20; // Average over 20 runs to filter noise
    
    // Warm-up cache
    asm!("cpuid", out("eax") _, out("ebx") _, out("ecx") _, out("edx") _);

    for _ in 0..iterations {
        let start: u64;
        let end: u64;
        
        // LFENCE ensures previous instructions retire.
        // RDTSCP forces serialization of the read itself.
        asm!(
            "lfence",
            "rdtscp",
            "shl rdx, 32",
            "or rax, rdx",
            "mov r8, rax", // r8 = start
            
            // Payload: CPUID -> Forced VM Exit
            "mov eax, 1",
            "cpuid",
            
            "rdtscp",
            "shl rdx, 32",
            "or rax, rdx", // rax = end
            
            out("rax") end,
            out("r8") start,
            out("rcx") _,
            out("rbx") _,
            out("rdx") _,
            options(nostack, nomem) 
        );
        total_cycles += end.wrapping_sub(start);
    }

    let avg = total_cycles / iterations;
    
    // Bare metal usually < 500-700 cycles.
    // VM usually > 1500 cycles (overhead of context switch).
    avg > 1200
}

/// Detects generic emulators (QEMU/Bochs) via CPUID Limit Anomaly.
/// 
/// Real hardware handles out-of-bounds CPUID leaves differently than emulators.
pub unsafe fn check_cpuid_limit_anomaly() -> bool {
    let max_leaf: u32;
    asm!(
        "mov eax, 0",
        "cpuid",
        out("eax") max_leaf,
        out("ebx") _, out("ecx") _, out("edx") _,
        options(nostack, nomem)
    );

    let invalid_leaf = max_leaf + 0x10000; 
    let mut a: u32; let mut b: u32; let mut c: u32; let mut d: u32;

    asm!(
        "mov eax, {0:e}",
        "cpuid",
        lateout("eax") a, lateout("ebx") b, lateout("ecx") c, lateout("edx") d,
        in(reg) invalid_leaf,
        options(nostack, nomem)
    );

    // If all registers are 0, it's likely a lazy emulator (QEMU default behavior).
    // Real hardware usually returns the value of the Max Supported Leaf.
    a == 0 && b == 0 && c == 0 && d == 0
}

/// Detects Kernel Debugger via KUSER_SHARED_DATA.
/// 
/// This is the "official" way Windows tracks debugger state.
/// Located at 0xFFFFF780000002D4.
pub unsafe fn check_kernel_debugger() -> bool {
    let addr = (KUSER_SHARED_DATA_ADDRESS + KD_DEBUGGER_ENABLED_OFFSET) as *const u8;
    
    // Read the byte: 1 = Enabled, 0 = Disabled, 2 = Not Present (rare)
    let kd_status = *addr;
    
    if kd_status == 0x1 || kd_status == 0x3 {
        log::warn!("🐛 Kernel Debugger Detected via KUSER_SHARED_DATA");
        return true;
    }
    
    false
}

/// Master function: Orchestrates all checks.
pub unsafe fn comprehensive_security_check() -> ShadowResult<()> {
    // 1. Check Debugger First (Lowest cost)
    if check_kernel_debugger() {
        return Err(ShadowError::SecurityViolation("Kernel Debugger Attached"));
    }

    // 2. Check Hypervisor Bit
    if check_hypervisor_bit() {
        // 3. Check Specific Signatures
        if let Some(vendor) = check_vendor_signatures() {
            log::warn!("🖥️ Hypervisor Signature Found: {}", vendor);
            return Err(ShadowError::SecurityViolation("Known Hypervisor Detected"));
        }
        
        // If bit is set but no vendor, it might be generic.
        // We continue to timing attack to be sure.
    }

    // 4. Heuristic: CPUID Limit Anomaly (Catches QEMU/Bochs)
    if check_cpuid_limit_anomaly() {
        return Err(ShadowError::SecurityViolation("Emulator Artifact (CPUID Limit)"));
    }

    // 5. Advanced: Timing Attack (Catches Stealth/Custom Hypervisors)
    if check_timing_anomaly() {
        log::warn!("⏱️ Timing Analysis indicates virtualization.");
        return Err(ShadowError::SecurityViolation("Timing Anomaly Detected"));
    }

    log::info!("✅ Environment appears Clean (Bare Metal / No Debugger).");
    Ok(())
}

/// Entry point for self-defense mechanism.
/// Returns generic STATUS_ACCESS_DENIED if threats are found.
pub unsafe fn verify_environment_integrity() -> NTSTATUS {
    match comprehensive_security_check() {
        Ok(_) => STATUS_SUCCESS,
        Err(e) => {
            log::error!("🚫 Environment Integrity Check Failed: {:?}", e);
            // In real malware, you would trigger a silent exit or decoy payload here.
            STATUS_ACCESS_DENIED
        }
    }
}
