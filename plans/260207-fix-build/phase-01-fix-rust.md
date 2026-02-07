# Phase 01: Fix Rust Rootkit

## Objective
Refactor `shadow-main/src/rootkit.rs` to fix `retour::static_detour!` compilation errors.

## Tasks
1. **Refactor rootkit.rs**
   - File: `shadow-main/src/rootkit.rs`
   - Remove `retour::static_detour!` macro usage.
   - Import `retour::GenericDetour` and `lazy_static::lazy_static`.
   - Define `NtQuerySystemInformationHook` using `lazy_static!` as a `GenericDetour`.
   - Update `install_hooks` to initialize the `GenericDetour`.
   - Update `nt_query_system_information_detour` to call the hook using `NtQuerySystemInformationHook.call`.

2. **Verify Compilation**
   - Run `build.ps1` to ensure Rust crate compiles successfully.
