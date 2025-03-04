#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
#include <stdint.h>
#include <string.h>

// Function to create and launch a process with parameters
HANDLE StartProcess(const wchar_t* exePath, DWORD* processID) {
    STARTUPINFO si = { 0 };
    PROCESS_INFORMATION pi = { 0 };
    si.cb = sizeof(si);

    // Command-line arguments to pass to the process
    wchar_t commandLine[MAX_PATH] = L""; // Buffer for full command
    _snwprintf_s(commandLine, sizeof(commandLine) / sizeof(wchar_t), L"\"%s\" -log -steam_auth PF_TITLEID=2EA46", exePath);

    if (CreateProcess(NULL, commandLine, NULL, NULL, FALSE, CREATE_SUSPENDED, NULL, NULL, &si, &pi)) {
        *processID = pi.dwProcessId;
        printf("Successfully started process: %ls (PID: %lu)\n", exePath, *processID);
        ResumeThread(pi.hThread);
        CloseHandle(pi.hThread);
        return pi.hProcess;
    }
    else {
        printf("Failed to start process. Error: %lu\n", GetLastError());
        return NULL;
    }
}

// Function to get the base address of the main module (PE Image)
uintptr_t GetModuleBaseAddress(DWORD processID) {
    uintptr_t baseAddress = 0;
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, processID);
    if (hSnapshot != INVALID_HANDLE_VALUE) {
        MODULEENTRY32 moduleEntry;
        moduleEntry.dwSize = sizeof(MODULEENTRY32);
        if (Module32First(hSnapshot, &moduleEntry)) {
            baseAddress = (uintptr_t)moduleEntry.modBaseAddr;
        }
        CloseHandle(hSnapshot);
    }
    else {
        printf("Failed to take process snapshot. Error: %lu\n", GetLastError());
    }
    return baseAddress;
}

// Function to get a handle to the target process
HANDLE OpenTargetProcess(DWORD processID) {
    HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, processID);
    if (hProcess == NULL) {
        printf("Failed to open process. Error: %lu\n", GetLastError());
    }
    return hProcess;
}

bool CheckTutorialActorsOffset(HANDLE hProcess, uintptr_t address) {
    BYTE buffer[2];
    SIZE_T bytesRead;
    if (ReadProcessMemory(hProcess, (LPCVOID)address, buffer, sizeof(buffer), &bytesRead)) {
        return buffer[0] == 0x32 && buffer[1] == 0xDB;
    } else {
        DWORD error = GetLastError();
        if (error == 299) {
            printf("Incomplete memory read. Retrying...\n");
            Sleep(1000);
            return CheckTutorialActorsOffset(hProcess, address);
        }
        printf("Failed to read memory. Error: %lu\n", error);
        return false;
    }
}

bool Patch(const wchar_t* processName) {
    DWORD processID;
    uintptr_t imageBase, address;
    BYTE newValues[2] = { 0xB3, 0x01 };
    SIZE_T bytesWritten;

    HANDLE hProcess = StartProcess(processName, &processID);
    if (hProcess == NULL) {
        return false;
    }

    // Wait a bit for PE image to load
    Sleep(5000);
    // Find the PE Image Base Address
    imageBase = GetModuleBaseAddress(processID);
    if (imageBase == 0) {
        printf("Failed to find PE Image Base Address.\n");
        return false;
    }
    printf("PE Image Base Address: 0x%p\n", (void*)imageBase);

    // Season 3 Patch tutorial actors loader: sub_3DA7520 -> loc_3DA75B6
    address = imageBase + 0x3DA75B6;
    bool found = CheckTutorialActorsOffset(hProcess, address);
    if (!found) {
        // Season 2 Patch tutorial actors loader: sub_3D19900 -> loc_3D19996
        address = imageBase + 0x3D19996;
        found = CheckTutorialActorsOffset(hProcess, address);
    }

    if (!found) {
        printf("Failed to find necessary memory chunk!");
        return false;
    }

    DWORD oldProtect;
    if (VirtualProtectEx(hProcess, (LPVOID)address, sizeof(newValues), PAGE_EXECUTE_READWRITE, &oldProtect)) {
        if (WriteProcessMemory(hProcess, (LPVOID)address, newValues, sizeof(newValues), &bytesWritten)) {
            printf("Memory Write Success: Wrote 0xB3 0x01 at address 0x%p\n", (void*)address);
        }
        else {
            printf("Failed to write memory. Error: %lu\n", GetLastError());
        }
        // Restore original protection
        VirtualProtectEx(hProcess, (LPVOID)address, sizeof(newValues), oldProtect, &oldProtect);
    }
    else {
        printf("Failed to change memory protection. Error: %lu\n", GetLastError());
    }

    // Clean up
    CloseHandle(hProcess);
    return true;
}

int main() {
    bool success = Patch(L"Prospect-Win64-Shipping.exe");
    if (!success) {
        // Wait for user input to exit
        printf("An error occurred when patching the executable! Press any key to exit...\n");
        getchar();
        return 1;
    }

    return 0;
}