
/// A procedural macro that allows WDF functions to be called by name.
///
/// This function parses the name of the WDF function, finds it function
/// pointer from the WDF function table, and then calls it with the
/// arguments passed to it
///
/// # Safety
/// Function arguments must abide by any rules outlined in the WDF
/// documentation. This macro does not perform any validation of the
/// arguments passed to it., beyond type validation.
///
/// # Examples
///
/// ```rust, no_run
/// use wdk_sys::*;
/// 
/// pub unsafe extern "system" fn driver_entry(
///     driver: &mut DRIVER_OBJECT,
///     registry_path: PCUNICODE_STRING,
/// ) -> NTSTATUS {
/// 
///     let mut driver_config = WDF_DRIVER_CONFIG {
///         Size: core::mem::size_of::<WDF_DRIVER_CONFIG>() as ULONG,
///         ..WDF_DRIVER_CONFIG::default()
///     };
///     let driver_handle_output = WDF_NO_HANDLE as *mut WDFDRIVER;
///
///     unsafe {
///         call_unsafe_wdf_function_binding!(
///             WdfDriverCreate,
///             driver as PDRIVER_OBJECT,
///             registry_path,
///             WDF_NO_OBJECT_ATTRIBUTES,
///             &mut driver_config,
///             driver_handle_output,
///         )
///     }
/// }
/// ```
#[macro_export]
macro_rules! call_unsafe_wdf_function_binding {
    ( $($tt:tt)* ) => {
        $crate::__proc_macros::call_unsafe_wdf_function_binding! (
            r"D:\Xorium_Stealer_Pulsar\Xorium Stealer Pulsar\shadow-main\crates\shadow-core\target\release\build\wdk-sys-148ad5a6c0ccabf3\out\types.rs",
            $($tt)*
        )
    }
}