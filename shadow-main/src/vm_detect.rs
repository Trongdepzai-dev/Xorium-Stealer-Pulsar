use std::arch::asm;
use log;

/// Master function to detect Virtual Environment
pub fn is_vm() -> bool {
    // 1. Check Hypervisor Present Bit (Standard)
    if check_cpuid_hypervisor_bit() {
        log::warn!("VM Detected: Hypervisor Bit Active");
        return true;
    }

    // 2. Check Hypervisor Vendor Strings (Expanded List)
    if check_cpuid_vendor_strings() {
        log::warn!("VM Detected: Hypervisor Vendor Match");
        return true;
    }

    // 3. Timing Attack: VM Exit Differential (Advanced & Serialized)
    if unsafe { check_rdtscp_vmexit_latency() } {
        log::warn!("VM Detected: High VM-Exit Latency");
        return true;
    }

    // 4. Hardware Artifact: CPUID Limit Anomalies
    if check_cpuid_limit_anomaly() {
        log::warn!("VM Detected: CPUID Limit Anomaly");
        return true;
    }

    // 5. Descriptor Table Heuristics (Sidt/Sgdt)
    if unsafe { check_table_heuristics() } {
        log::warn!("VM Detected: Descriptor Table Anomaly");
        return true;
    }

    false
}

/// Checks CPUID Leaf 0x1, ECX Bit 31 (Standard Hypervisor Bit)
#[inline(always)]
fn check_cpuid_hypervisor_bit() -> bool {
    let ecx: u32;
    unsafe {
        // cpuid clobbers eax, ebx, ecx, edx.
        // We need to save rbx because LLVM reserves it.
        asm!(
            "push rbx",
            "cpuid",
            "pop rbx",
            inout("eax") 1 => _,
            lateout("ecx") ecx,
            out("edx") _,
        );
    }
    (ecx >> 31) & 1 == 1
}

/// Checks CPUID Leaf 0x40000000 for Known Vendor Signatures
fn check_cpuid_vendor_strings() -> bool {
    let ebx_val: u32;
    let ecx_val: u32;
    let edx_val: u32;

    unsafe {
        asm!(
            "push rbx",
            "cpuid",
            "mov {0:e}, ebx", // Save ebx to output register
            "pop rbx",
            out(reg) ebx_val,
            lateout("ecx") ecx_val,
            lateout("edx") edx_val,
            inout("eax") 0x40000000u32 => _,
        );
    }

    let brand_bytes = [
        (ebx_val & 0xFF) as u8, ((ebx_val >> 8) & 0xFF) as u8, ((ebx_val >> 16) & 0xFF) as u8, ((ebx_val >> 24) & 0xFF) as u8,
        (ecx_val & 0xFF) as u8, ((ecx_val >> 8) & 0xFF) as u8, ((ecx_val >> 16) & 0xFF) as u8, ((ecx_val >> 24) & 0xFF) as u8,
        (edx_val & 0xFF) as u8, ((edx_val >> 8) & 0xFF) as u8, ((edx_val >> 16) & 0xFF) as u8, ((edx_val >> 24) & 0xFF) as u8,
    ];

    if let Ok(brand) = core::str::from_utf8(&brand_bytes) {
        let signatures = [
            "Microsoft Lv", "VMwareVMware", "XenVMMXenVMM", "KVMKVMKVM\0\0\0",
            "VBoxVBoxVBox", "prl hyperv  ", "TCGTCGTCGTCG", "bhyve bhyve ",
        ];
        for sig in signatures.iter() {
            if brand.contains(sig) {
                return true;
            }
        }
    }
    false
}

unsafe fn check_rdtscp_vmexit_latency() -> bool {
    let mut total_cycles: u64 = 0;
    let iterations = 100;
    
    // Warm-up (rbx save required for cpuid)
    asm!("push rbx", "cpuid", "pop rbx", inout("eax") 0 => _, out("ecx") _, out("edx") _);

    for _ in 0..iterations {
        let start: u64;
        let end: u64;
        
        asm!(
            "lfence",
            "rdtscp",
            "shl rdx, 32",
            "or rax, rdx",
            "mov r8, rax", 
            
            "mov eax, 1",
            "push rbx", // Save rbx before cpuid
            "cpuid",
            "pop rbx",  // Restore rbx
            
            "rdtscp",
            "shl rdx, 32",
            "or rax, rdx",
            
            out("rax") end,
            out("r8") start,
            out("rcx") _, 
            out("rdx") _, 
        );
        total_cycles += end.wrapping_sub(start);
    }

    let avg_cycles = total_cycles / iterations;
    avg_cycles > 1200
}

fn check_cpuid_limit_anomaly() -> bool {
    let max_leaf: u32;
    unsafe {
        asm!(
            "push rbx",
            "cpuid",
            "pop rbx",
            inout("eax") 0 => max_leaf,
            out("ecx") _,
            out("edx") _,
        );
    }

    let invalid_leaf = max_leaf + 0x1000;
    let eax_ret: u32;
    let ebx_ret: u32;
    let ecx_ret: u32;
    let edx_ret: u32;

    unsafe {
        asm!(
            "push rbx",
            "cpuid",
            "mov {0:e}, ebx", // Save result ebx
            "pop rbx",
            out(reg) ebx_ret,
            lateout("eax") eax_ret,
            lateout("ecx") ecx_ret,
            lateout("edx") edx_ret,
            in("eax") invalid_leaf, // Pass invalid_leaf in eax
        );
    }

    eax_ret == 0 && ebx_ret == 0 && ecx_ret == 0 && edx_ret == 0
}

unsafe fn check_table_heuristics() -> bool {
    let mut idtr: [u8; 10] = [0; 10]; 
    
    asm!("sidt [{}]", in(reg) &mut idtr);
    
    let idt_base = u64::from_le_bytes(idtr[2..10].try_into().unwrap());
    
    if idt_base == 0 { return true; }

    let upper_byte = (idt_base >> 56) as u8;
    if upper_byte < 0xF0 { return true; }

    let ldt_sel: u16;
    asm!("sldt {:x}", out(reg) ldt_sel);
    if ldt_sel != 0 { return true; }

    let tr_sel: u16;
    asm!("str {:x}", out(reg) tr_sel);
    if tr_sel == 0x4000 { return true; }

    false
}