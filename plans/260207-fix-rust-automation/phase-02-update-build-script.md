# Phase 02: Update Build Script

## Objective
Update `build.ps1` to automatically compile the Rust driver if the artifact is missing in `shadow-main`.

## Implementation Steps
1. **Update Build-RustDriver function**
   - File: `build.ps1`
   - Modify the logic to check if `shadow_core.sys` exists.
   - If missing, use `Push-Location` to enter `shadow-main`, run `cargo build --release`, and then `Pop-Location`.
2. **Test automation**
   - Delete `shadow-main/target/release/shadow_core.sys`.
   - Run `build.ps1` and verify that the driver is built and copied to `dist`.

## Success Criteria
- `build.ps1` successfully builds the Rust driver when it is missing.
