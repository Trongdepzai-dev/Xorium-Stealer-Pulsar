# Phase 02: Fileless Implementation

## Objective
Implement RAM-only execution for all payloads to ensure zero disk footprint.

## Tasks
1. [ ] **Task 2.1: .NET Memory Loader (CLR Hosting)**
   - Add C++ code to host the .NET runtime and execute `nvhda64v_inner.exe` from memory.
2. [ ] **Task 2.2: Process Hollowing Engine**
   - Implement a hollowing routine to run `GodPotato.exe` inside a sacrificial process.
3. [ ] **Task 2.3: Kernel Manual Mapper (Rust Rootkit Integration)**
   - Implement the manual mapping logic to load `shadow_core.sys` from `shadow-main` directly into kernel memory without disk extraction.
4. [ ] **Task 2.4: Resource-to-RAM Bridge**
   - Update `ExtractResource` to return a memory buffer instead of writing to a file.

## Deliverables
- Patched source code.
- Corrected build sequence.
