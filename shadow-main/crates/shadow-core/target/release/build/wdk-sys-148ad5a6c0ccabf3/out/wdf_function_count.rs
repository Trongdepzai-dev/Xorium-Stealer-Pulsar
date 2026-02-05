#[allow(clippy::must_use_candidate)]
/// Returns the number of functions available in the WDF function table.
/// Should not be used in public API.
pub fn get_wdf_function_count() -> usize {
    // SAFETY: `crate::WdfFunctionCount` is generated as a mutable static, but is not supposed to be ever mutated by WDF.
    (unsafe { crate::WdfFunctionCount }) as usize
}