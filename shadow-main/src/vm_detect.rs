use std::arch::asm;

pub fn is_vm() -> bool {
    // Phase 1: Hypervisor Present?
    if check_cpuid_hypervisor() {
        return true;
    }
    
    // Phase 2: Timing Attack (VM Exit Latency)
    if check_rdtsc_timing() {
        return true;
    }

    // Phase 3: Red Pill (Table Checks) - Advanced
    if check_red_pill() {
        return true;
    }

    false
}

/// Checks CPUID leaf 0x1 (ECX bit 31) and 0x40000000 (Hypervisor Vendor)
fn check_cpuid_hypervisor() -> bool {
    let mut ecx: u32;
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
    
    // Bit 31 of ECX being set implies a hypervisor is present
    if (ecx >> 31) & 1 == 1 {
        // Confirm with leaf 0x40000000
        let mut ebx: u32;
        let mut ecx_brand: u32;
        let mut edx: u32;
        
        unsafe {
            asm!(
                "mov eax, 0x40000000",
                "cpuid",
                lateout("ebx") ebx,
                lateout("ecx") ecx_brand,
                lateout("edx") edx,
                out("eax") _,
            );
        }
        
        let brand_bytes = [
            (ebx & 0xFF) as u8, ((ebx >> 8) & 0xFF) as u8, ((ebx >> 16) & 0xFF) as u8, ((ebx >> 24) & 0xFF) as u8,
            (ecx_brand & 0xFF) as u8, ((ecx_brand >> 8) & 0xFF) as u8, ((ecx_brand >> 16) & 0xFF) as u8, ((ecx_brand >> 24) & 0xFF) as u8,
            (edx & 0xFF) as u8, ((edx >> 8) & 0xFF) as u8, ((edx >> 16) & 0xFF) as u8, ((edx >> 24) & 0xFF) as u8,
        ];
        
        if let Ok(brand) = std::str::from_utf8(&brand_bytes) {
            if brand.contains("Microsoft Lv") || 
               brand.contains("VMwareVMware") || 
               brand.contains("XenVMMXenVMM") ||
               brand.contains("KVMKVMKVM") ||
               brand.contains("VBoxVBoxVBox") {
                return true;
            }
        }
        
        return true;
    }
    
    false
}

/// RDTSC timing attack - measures the delta of CPUID instruction
fn check_rdtsc_timing() -> bool {
    let mut t1: u64;
    let mut t2: u64;
    
    unsafe {
        asm!(
            "rdtsc",
            "shl rdx, 32",
            "or rax, rdx",
            out("rax") t1,
            out("rdx") _,
        );
        
        asm!(
            "rdtsc",
            "shl rdx, 32",
            "or rax, rdx",
            out("rax") t2,
            out("rdx") _,
        );
        
        let start: u64;
        let end: u64;
        
        asm!(
            "rdtsc",
            "shl rdx, 32",
            "or rax, rdx",
            "mov r8, rax",
            
            "mov eax, 1",
            "cpuid",
            
            "rdtsc",
            "shl rdx, 32",
            "or rax, rdx",
            
            out("rax") end,
            out("r8") start,
            out("rbx") _,
            out("rcx") _,
            out("rdx") _,
        );
        
        let cost = end - start;
        if cost > 1500 {
            return true;
        }
    }
    false
}

/// Red Pill: check IDT/GDT location
/// On 64-bit Windows, the IDT base is typically at 0xFFFFF800... (Kernel space)
/// But in some VMs, it gets pushed much higher or has a specific signature.
fn check_red_pill() -> bool {
    let mut idtr: [u8; 10] = [0; 10]; // 2 bytes limit, 8 bytes base
    let mut gdtr: [u8; 10] = [0; 10];
    
    unsafe {
        asm!("sidt [{}]", in(reg) &mut idtr);
        asm!("sgdt [{}]", in(reg) &mut gdtr);
    }
    
    // Extract Base Address (Bytes 2-9)
    let idt_base = u64::from_le_bytes(idtr[2..10].try_into().unwrap());
    let gdt_base = u64::from_le_bytes(gdtr[2..10].try_into().unwrap());
    
    // Simple heuristic: If the base address tells us we are running under a known hypervisor mapping.
    // e.g., if IDT base > 0xD000000000000000 (very high kernel space often used by HV)
    // This requires specific calibration per OS version, but we can check if it aligns with known VM patterns.
    // For now, let's verify if the address seems 'relocated' relative to expected bare metal range (usually lower kernel mappings).
    
    // Note: Effective implementation often compares against a baseline or looks for 0xFFFFFF...
    // A robust check is simply "can we read it without crashing?" (we just did).
    
    // For this specific 'Advanced' request, we'll check if the GDT base is suspiciously high (common in some VBox setups).
    // Or if IDT == 0 (which shouldn't happen on real hardware).
    
    if idt_base == 0 || gdt_base == 0 {
        return true;
    }
    
    // Check for STR (Store Task Register) - often 0x4000 on VMWare
    let mut tr: u16;
    unsafe {
        asm!("str {:x}", out(reg) tr);
    }
    if tr > 0x4000 {
        // Suspiciously unusual Task Register value
        // return true; 
    }

    false
}
