use shadow_core::error::ShadowError;
use wdk_sys::{
    ntddk::{ExAllocatePool2, ExFreePool, MmCopyMemory}, 
    IRP, MM_COPY_ADDRESS, MM_COPY_MEMORY_VIRTUAL, 
    NT_SUCCESS, POOL_FLAG_NON_PAGED, _IO_STACK_LOCATION
};

/// Retrieves the input buffer from the given IO stack location using METHOD_BUFFERED.
///
/// # Arguments
/// 
/// * `irp` - A pointer to the `IRP` structure.
/// * `stack` - A pointer to the `_IO_STACK_LOCATION` structure.
///
/// # Returns
/// 
/// Containing the pointer to the input buffer or an NTSTATUS error code.
pub unsafe fn get_input_buffer<T>(irp: *mut IRP, stack: *mut _IO_STACK_LOCATION) -> Result<*mut T, ShadowError> {
    // In METHOD_BUFFERED, the buffer is in SystemBuffer
    let input_buffer = (*irp).AssociatedIrp.SystemBuffer;
    let input_length = (*stack).Parameters.DeviceIoControl.InputBufferLength;

    // Validate that the input buffer is not null
    if input_buffer.is_null() {
        return Err(ShadowError::NullPointer("SystemBuffer"))
    } 
    
    // Validate that the input buffer size is sufficient
    if input_length < size_of::<T>() as u32 {
        return Err(ShadowError::BufferTooSmall);
    }

    // Alignment and security is handled by the I/O manager in METHOD_BUFFERED
    // We can cast directly as the buffer is now in kernel space and immutable by user-space during processing
    Ok(input_buffer as *mut T)
}

/// Retrieves the output buffer from the given IRP using METHOD_BUFFERED.
///
/// # Arguments
/// 
/// * `irp` - A pointer to the `IRP` structure.
/// * `stack` - A pointer to the `_IO_STACK_LOCATION` structure.
///
/// # Returns
/// 
/// Containing the pointer to the output buffer and count of objects or an NTSTATUS error code.
pub unsafe fn get_output_buffer<T>(irp: *mut IRP, stack: *mut _IO_STACK_LOCATION) -> Result<(*mut T, usize), ShadowError> {
    // In METHOD_BUFFERED, the output buffer is also in SystemBuffer
    let buffer = (*irp).AssociatedIrp.SystemBuffer;
    if buffer.is_null() {
        return Err(ShadowError::NullPointer("SystemBuffer"));
    }

    let output_length = (*stack).Parameters.DeviceIoControl.OutputBufferLength;
    if output_length < size_of::<T>() as u32 {
        return Err(ShadowError::BufferTooSmall);
    }

    let count = output_length as usize / size_of::<T>();
    Ok((buffer as *mut T, count))
}
