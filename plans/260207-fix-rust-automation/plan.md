# Fix Rust and Automate Build Plan

## Overview
Fix compilation errors in the Rust rootkit (`shadow-main`) and update the PowerShell build script to automatically compile the Rust driver if it's missing.

## Status
Created: 2026-02-07
Current Phase: Phase 1
Progress: 0%

## Architecture
- `build.ps1`: Orchestrates the entire build process.
- `shadow-main`: Rust kernel driver project.
- `dist`: Output directory for build artifacts.

## Phases Table
| # | Name | Description | Status |
|---|---|---|---|
| 01 | Fix Rust Code | Resolve macro and struct errors in Rust source files. | Todo |
| 02 | Update Build Script | Automate Rust compilation in `build.ps1`. | Todo |
| 03 | Validation | Verify the entire build flow. | Todo |

## Tech Stack
- PowerShell (Build script)
- Rust (Kernel driver)
- C++ (Loader)
- .NET (Plugin)