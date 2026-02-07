# Fix Build Plan

## Overview
Fix compilation errors in `shadow_core` Rust crate and subsequent C++ loader build failures. The main issue is the incorrect usage of `retour::static_detour!` macro which is failing to resolve. We will refactor this to use `lazy_static!` and `GenericDetour` pattern.

## Phases
| # | Name | Description | Status |
|---|---|---|---|
| 01 | Fix Rust Rootkit | Refactor `rootkit.rs` to use `lazy_static` + `GenericDetour`. | Active |