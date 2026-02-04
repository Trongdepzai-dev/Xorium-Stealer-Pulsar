use std::arch::asm;

/// Master function to detect Virtual Environment
pub fn is_vm_hardcore() -> bool {
    // 1. Check Hypervisor Present Bit (Standard)
    if check_cpuid_hypervisor_bit() {
        log::warn!("🚩 VM Detected: Hypervisor Bit Active");
        return true;
    }

    // 2. Check Hypervisor Vendor Strings (Expanded List)
    if check_cpuid_vendor_strings() {
        log::warn!("🚩 VM Detected: Hypervisor Vendor Match");
        return true;
    }

    // 3. Timing Attack: VM Exit Differential (Advanced & Serialized)
    // Measures the cost of forcing a VM Exit vs a standard instruction.
    if unsafe { check_rdtscp_vmexit_latency() } {
        log::warn!("🚩 VM Detected: High VM-Exit Latency");
        return true;
    }

    // 4. Hardware Artifact: CPUID Limit Anomalies
    // Real hardware handles out-of-bounds CPUID differently than generic VMs.
    if check_cpuid_limit_anomaly() {
        log::warn!("🚩 VM Detected: CPUID Limit Anomaly");
        return true;
    }

    // 5. Descriptor Table Heuristics (Sidt/Sgdt)
    if unsafe { check_table_heuristics() } {
        log::warn!("🚩 VM Detected: Descriptor Table Anomaly");
        return true;
    }

    false
}

/// Checks CPUID Leaf 0x1, ECX Bit 31 (Standard Hypervisor Bit)
#[inline(always)]
fn check_cpuid_hypervisor_bit() -> bool {
    let ecx: u32;
    unsafe {
        asm!(
            "mov eax, 1",
            "cpuid",
            lateout("ecx") ecx,
            out("eax") _,
            out("ebx") _,
            out("edx") _,
        );
    }
    // Bit 31 set = Running under hypervisor
    (ecx >> 31) & 1 == 1
}

/// Checks CPUID Leaf 0x40000000 for Known Vendor Signatures
fn check_cpuid_vendor_strings() -> bool {
    let mut ebx: u32;
    let mut ecx: u32;
    let mut edx: u32;

    unsafe {
        asm!(
            "mov eax, 0x40000000",
            "cpuid",
            lateout("ebx") ebx,
            lateout("ecx") ecx,
            lateout("edx") edx,
            out("eax") _,
        );
    }

    let brand_bytes = [
        (ebx & 0xFF) as u8, ((ebx >> 8) & 0xFF) as u8, ((ebx >> 16) & 0xFF) as u8, ((ebx >> 24) & 0xFF) as u8,
        (ecx & 0xFF) as u8, ((ecx >> 8) & 0xFF) as u8, ((ecx >> 16) & 0xFF) as u8, ((ecx >> 24) & 0xFF) as u8,
        (edx & 0xFF) as u8, ((edx >> 8) & 0xFF) as u8, ((edx >> 16) & 0xFF) as u8, ((edx >> 24) & 0xFF) as u8,
    ];

    if let Ok(brand) = core::str::from_utf8(&brand_bytes) {
        let signatures = [
            "Microsoft Lv", // Hyper-V
            "VMwareVMware", // VMWare
            "XenVMMXenVMM", // Xen
            "KVMKVMKVM\0\0\0", // KVM
            "VBoxVBoxVBox", // VirtualBox
            "prl hyperv  ", // Parallels
            "TCGTCGTCGTCG", // QEMU
            "bhyve bhyve ", // FreeBSD bhyve
        ];

        for sig in signatures.iter() {
            if brand.contains(sig) {
                return true;
            }
        }
    }
    false
}

/// ADVANCED TIMING ATTACK
/// Uses RDTSCP (Serialized) instead of RDTSC to prevent out-of-order execution.
/// Measures average latency of CPUID (which causes VM-Exit) over multiple iterations.
unsafe fn check_rdtscp_vmexit_latency() -> bool {
    let mut total_cycles: u64 = 0;
    let iterations = 100;
    
    // Warm-up cache and TLB
    asm!("cpuid", out("eax") _, out("ebx") _, out("ecx") _, out("edx") _);

    for _ in 0..iterations {
        let start: u64;
        let end: u64;
        
        // Start Measurement
        // rdtscp reads timestamp AND IA32_TSC_AUX. It forces serialization.
        // lfence ensures all previous instructions retired.
        asm!(
            "lfence",
            "rdtscp",
            "shl rdx, 32",
            "or rax, rdx",
            "mov r8, rax", // Store start time in R8
            
            // Payload: CPUID causes a mandatory VM-Exit (context switch to Hypervisor)
            // This is very expensive on a VM, cheap on Bare Metal.
            "mov eax, 1",
            "cpuid",
            
            // End Measurement
            "rdtscp",
            "shl rdx, 32",
            "or rax, rdx",
            
            out("rax") end,
            out("r8") start,
            out("rcx") _, // rdtscp clobbers rcx
            out("rbx") _, // cpuid clobbers rbx
            out("rdx") _, // rdtscp clobbers rdx
        );
        total_cycles += end.wrapping_sub(start);
    }

    let avg_cycles = total_cycles / iterations;

    // Thresholds (Calibrated for modern CPUs ~3-4GHz)
    // Bare Metal: Typically < 500-700 cycles (cached)
    // Virtual Machine: Typically > 1500-2000+ cycles (due to VM-Exit overhead)
    avg_cycles > 1200
}

/// Checks for anomalies in CPUID Max Leaf behavior.
/// On real Intel HW, requesting a CPUID leaf > Max_Leaf returns data for the Max_Leaf.
/// Many VMs simply return 0s for undefined leaves.
fn check_cpuid_limit_anomaly() -> bool {
    let max_leaf: u32;
    unsafe {
        asm!(
            "mov eax, 0x0", // Get Max Standard Leaf
            "cpuid",
            out("eax") max_leaf,
            out("ebx") _,
            out("ecx") _,
            out("edx") _,
        );
    }

    let invalid_leaf = max_leaf + 0x1000; // Way out of bounds
    let mut eax_ret: u32;
    let mut ebx_ret: u32;
    let mut ecx_ret: u32;
    let mut edx_ret: u32;

    unsafe {
        asm!(
            "mov eax, {0:e}",
            "cpuid",
            lateout("eax") eax_ret,
            lateout("ebx") ebx_ret,
            lateout("ecx") ecx_ret,
            lateout("edx") edx_ret,
            in(reg) invalid_leaf,
        );
    }

    // Heuristic: If we get all zeros for an invalid leaf, it's suspicious (likely a cheap emulator).
    // Real hardware usually returns *something* (often the values of the highest valid leaf).
    eax_ret == 0 && ebx_ret == 0 && ecx_ret == 0 && edx_ret == 0
}

/// Red Pill: Checks Interrupt/Global Descriptor Table locations.
/// On x64 Windows, IDT base is in Kernel Space.
/// Some VMs (especially older ones or specific configs) relocate this significantly.
unsafe fn check_table_heuristics() -> bool {
    let mut idtr: [u8; 10] = [0; 10]; // 16-bit limit + 64-bit base
    
    asm!("sidt [{}]", in(reg) &mut idtr);
    
    // Extract Base Address (Bytes 2-9)
    let idt_base = u64::from_le_bytes(idtr[2..10].try_into().unwrap());
    
    // Check 1: Null IDT (Impossible on running OS)
    if idt_base == 0 {
        return true;
    }

    // Check 2: High Memory Mapping (Common VM signature)
    let upper_byte = (idt_base >> 56) as u8;
    if upper_byte < 0xF0 {
        return true; 
    }

    // Check 3: Local Descriptor Table (SLDT)
    let ldt_sel: u16;
    asm!("sldt {:x}", out(reg) ldt_sel);
    if ldt_sel != 0 {
        return true;
    }

    // Check 4: Task Register (STR)
    let tr_sel: u16;
    asm!("str {:x}", out(reg) tr_sel);
    if tr_sel == 0x4000 {
        return true;
    }

    false
}
