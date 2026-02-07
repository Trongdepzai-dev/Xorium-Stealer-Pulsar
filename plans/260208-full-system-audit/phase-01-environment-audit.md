# Phase 01: Environment Audit

## Objective
Ensure all required build tools and dependencies are present and paths are correctly configured.

## Tasks
1. [ ] **Task 1.1: Toolchain Verification**
   - Check `dotnet --version`
   - Check `rustc --version`
   - Check `vcvars64.bat` location
2. [ ] **Task 1.2: Dependency Integrity**
   - Verify `ConfuserEx-CLI\Confuser.CLI.exe`
   - Verify `GodPotato.exe`
   - Verify `unDefender.exe`
3. [ ] **Task 1.3: Path Decoupling**
   - Update `build.ps1` to use dynamic paths for tools instead of hardcoded D:\ paths.

## Deliverables
- Validated build environment.
- Updated `build.ps1` with better path handling.

## Validation Criteria
- Build script can identify all tools without manual path editing.
