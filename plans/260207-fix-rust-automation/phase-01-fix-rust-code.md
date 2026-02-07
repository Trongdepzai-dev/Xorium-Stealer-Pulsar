# Phase 01: Fix Rust Code

## Objective
Resolve compilation errors in `shadow-main` caused by incorrect macro usage and missing winapi structs.

## Implementation Steps
1. **Modify rootkit.rs**
   - File: `shadow-main/src/rootkit.rs`
   - Remove `use retour::static_detour;`.
   - Wrap `static NtQuerySystemInformationHook...` in `retour::static_detour! { ... }`.
2. **Modify stealth.rs**
   - File: `shadow-main/src/stealth.rs`
   - Remove `use winapi::um::winnt::{PEB, PPEB, TEB, PTEB};`.
   - Use raw pointers and offsets (e.g., `gs:[0x60]` for PEB on x64) to access process information.
3. **Verify Rust build**
   - Run `cargo build --release` in `shadow-main` to confirm fixes.

## Success Criteria
- `shadow-main` compiles without errors using `cargo build --release`.
