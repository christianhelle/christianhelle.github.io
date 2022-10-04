---
layout: post
title: Copying Files from the Device to the Desktop using .NET
date: '2007-06-16T17:38:00.001+02:00'
author: Christian Resma Helle
tags:
- RAPI
modified_time: '2010-08-23T12:42:18.173+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-9184502135880847787
blogger_orig_url: https://christian-helle.blogspot.com/2007/06/copying-files-from-device-to-desktop.html
---

Recently, I’ve been working on a tool that creates a backup of a SQL Server Compact Edition database on the device to the desktop. To accomplish this, I used the Remote API (RAPI). Unfortunately, the Remote API is not yet available in managed code. In this article I would like to demonstrate how to P/Invoke methods from the Remote API for copying files from the device to the desktop using managed code.

First, we’ll need some P/Invokes to rapi.dll

```csharp
[DllImport("rapi.dll")]
static extern int CeRapiInit();

[DllImport("rapi.dll")]
static extern int CeRapiUninit();

[DllImport("rapi.dll")]
static extern int CeCloseHandle(IntPtr hObject);

[DllImport("rapi.dll", CharSet = CharSet.Unicode)]
static extern IntPtr CeCreateFile(
    string lpFileName,
    uint dwDesiredAccess,
    int dwShareMode,
    int lpSecurityAttributes,
    int dwCreationDisposition,
    int dwFlagsAndAttributes,
    int hTemplateFile);

[DllImport("rapi.dll", CharSet = CharSet.Unicode)]
static extern int CeReadFile(
    IntPtr hFile,
    byte[] lpBuffer,
    int nNumberOfbytesToRead,
    ref int lpNumberOfbytesRead,
    int lpOverlapped);

const int ERROR_SUCCESS = 0;
const int OPEN_EXISTING = 3;
const int INVALID_HANDLE_VALUE = -1;
const int FILE_ATTRIBUTE_NORMAL = 0x80;
const uint GENERIC_READ = 0x80000000;
```

Now let’s create a method called CopyFromDevice(string remote_file, string local_file). The remote_file parameter is the source file on the device that you wish to copy. The local_file parameter is the destination filename on the desktop.

```csharp
public static void CopyFromDevice(string remote_file, string local_file)
{
    bool rapi = CeRapiInit() == ERROR_SUCCESS;
    if (!rapi) {
        return;
    }

    IntPtr remote_file_ptr = CeCreateFile(
        remote_file,
        GENERIC_READ,
        0,
        0,
        OPEN_EXISTING,
        FILE_ATTRIBUTE_NORMAL,
        0);

    if (remote_file_ptr.ToInt32() == INVALID_HANDLE_VALUE) {
        return;
    }

    FileStream local_file_stream = new FileStream(
    local_file,
    FileMode.Create,
    FileAccess.Write);

    int read = 0;
    int size = 1024 * 4;
    byte[] data = new byte[size];

    CeReadFile(remote_file_ptr, data, size, ref read, 0);

    while (read > 0) {
        local_file_stream.Write(data, 0, read);
        if (CeReadFile(remote_file_ptr, data, size, ref read, 0) == 0) 
        {
            CeCloseHandle(remote_file_ptr);
            local_file_stream.Close();
            return;
        }
    }

    CeCloseHandle(remote_file_ptr);
    local_file_stream.Flush();
    local_file_stream.Close();

    if (rapi) {
        CeRapiUninit();
    }

    if (!File.Exists(local_file)) {
        throw new FileNotFoundException("The file was not copied to the desktop");
    }
}
```

To use the code above you will have to know the full path of the file on the device. The way I did it was to read the registry on the device and check if my application was installed, if it was then I get the path of the application and pass as the path in my remote_file parameter.