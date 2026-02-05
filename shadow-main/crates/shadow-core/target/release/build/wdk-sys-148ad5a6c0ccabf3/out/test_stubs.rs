
use crate::WDFFUNC;

/// Stubbed version of the symbol that `WdfFunctions` links to so that test targets will compile
#[no_mangle]
pub static mut WdfFunctions_01031: *const WDFFUNC = core::ptr::null();
