---
layout: post
title: Generic Multiple CAB File Installer for the Desktop
date: '2007-07-16T15:25:00.000+02:00'
author: Christian Resma Helle
tags: 
modified_time: '2007-07-18T22:56:29.011+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-4732073177296268418
blogger_orig_url: https://christian-helle.blogspot.com/2007/07/generic-multiple-cab-file-installer-for.html
---

In this article I would like to share a nice and simple multiple CAB file installer application for the desktop that I have been using for a few years now. The tool can install up to 10 CAB files and is written in C and has less than a 100 lines of code.  
  
Before we get started with installing multiple CAB files with a generic installer, let's try to go through the process of installing a CAB file from the desktop.  
  
Installing a CAB file from the desktop to the mobile device is pretty simple. All you need to do is create a .INI file that defines the CAB files you wish to install and launch the Application Manager (CeAppMgr.exe) passing the .INI file (full path) as the arguments. Application Manager is included when installing ActiveSync or the Windows Mobile Device Center (Vista)  
  
An Application Manager .INI file contains information that registers an application with the Application Manager. The .INI file uses the following format:  

```ini
[CEAppManager]  
Version = 1.0  
Component = component_name  
  
[component_name]  
Description = descriptive_name  
CabFiles = cab_filename [,cab_filename]  
```

Here's an MSDN link for more details on [Creating an .ini File for the Application Manager](https://topic.alibabacloud.com/a/refereces-creating-an-ini-file-for-the-application-manager_8_8_32123417.html)

Before launching the Application Manager, we should first make sure that it's installed. Once we know that then we get the location of the file. The easiest way to do this programmatically is to look into the registry key `SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\CEAppMgr.exe`. If this key doesn't exist, then the Application Manager is not installed.  
  
The next step is quite interesting yet very simple. In this installer application, I use a configuration file called **Setup.ini** which defines which CAB files I want to install. **Setup.ini** will look like this:  
  
```ini
[CabFiles]  
CABFILE1 = MyApp1.ini  
CABFILE2 = SQLCE.ini  
CABFILE3 = NETCF.ini  
CABFILE4 =  
CABFILE5 =  
CABFILE6 =  
CABFILE7 =  
CABFILE8 =  
CABFILE9 =  
CABFILE10 =  
```

You can easily modifiy the code to install more than 10 CAB files if you think it's appropriate. I used `GetPrivateProfileString()` to read the values from my configuration file.  
  
**Setup.ini**, the .INI files, and the actual CAB files are required to be in the same directory as the generic installer.  
  
Ok, now we have Application Manager command-line arguments ready, we now just have to launch it. I used `CreateProcess()` to launch the application manager and used `WaitForSingleObject()` to wait for the process to end.  
  
Here's the full source code:  
  
[C CODE]  

```c
#include "windows.h"  
#include "tchar.h"  
  
#define CE_APP_MGR TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\CEAppMgr.exe")  
  
LPTSTR GetParameters()  
{  
  TCHAR szParams[2048];  
  szParams[0] = TCHAR(0);  
  
  TCHAR szCurrDir[MAX_PATH];  
  GetCurrentDirectory(MAX_PATH, szCurrDir);  
  
  for (int i = 1; i < 11; i++) {  
    TCHAR buffer[16];  
    TCHAR szKey[16];  
  
    strcpy(szKey, TEXT("CABFILE"));  
    itoa(i, buffer, 16);  
    strcat(szKey, buffer);  
  
    TCHAR szSetupIni[MAX_PATH];  
    strcpy(szSetupIni, szCurrDir);  
    strcat(szSetupIni, TEXT("\\"));  
    strcat(szSetupIni, TEXT("Setup.ini"));  
  
    TCHAR szCabFile[MAX_PATH];  
    ::GetPrivateProfileString(TEXT("CabFiles"), szKey,  
      (TCHAR\*)"", szCabFile, sizeof(szCabFile), szSetupIni);  
  
    if (0 != strcmp(szCabFile, (TCHAR\*)"")) {  
      strcat(szParams, TEXT(" \""));  
      strcat(szParams, szCurrDir);  
      strcat(szParams, TEXT("\\"));  
      strcat(szParams, szCabFile);  
      strcat(szParams, TEXT(" \""));  
    }  
  }  
  
  return szParams;  
}  
  
int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)  
{  
  HKEY hkey;  
  DWORD dwDataSize = MAX_PATH;  
  DWORD dwType = REG_SZ;  
  TCHAR szCEAppMgrPath[MAX_PATH];  
  
  if(ERROR_SUCCESS == RegOpenKeyEx(HKEY_LOCAL_MACHINE, CE_APP_MGR, 0, KEY_READ, &hkey)) {  
    if(ERROR_SUCCESS != RegQueryValueEx(hkey, NULL, NULL,  
      &dwType, (PBYTE)szCEAppMgrPath, &dwDataSize))  
    {  
      MessageBox(NULL, TEXT("Unable to find Application Manager for Pocket PC Applications"),  
        TEXT("Error"), MB_ICONEXCLAMATION | MB_OK);  
      return 1;  
    }  
  } else {  
    MessageBox(NULL, TEXT("Unable to find Application Manager for Pocket PC Applications"),  
      TEXT("Error"), MB_ICONEXCLAMATION | MB_OK);  
    return 1;  
  }  
  
  
  RegCloseKey(hkey);  
  
  STARTUPINFO startup_info = {0};  
  PROCESS_INFORMATION pi = {0};  
  startup_info.cb = sizeof(startup_info);  
  
  if (CreateProcess(szCEAppMgrPath, GetParameters(), NULL,  
    NULL, FALSE, 0, NULL, NULL, &startup_info, &pi))  
  {  
    WaitForSingleObject(pi.hProcess, INFINITE);  
  } else {  
    MessageBox(NULL, TEXT("Unable to Launch Application Manager for Pocket PC Applications"),  
      TEXT("Error"), MB_ICONEXCLAMATION | MB_OK);  
    return 2;  
  }  
  
  return 0;  
}  
```
  
When deplying my applications in this manner, I like packaging them in a self-extracting zip file that is configured for software installation. I'm still holding to an old version of Winzip Self-Extractor to accomplish this task.  
  
Another reason I use WaitForSingleObject is because a self-extracting zip installer file deletes the all the temporary files it has extracted once the installer process ends. This means that your .INI and CAB files will be deleted even before they get copied to the device.