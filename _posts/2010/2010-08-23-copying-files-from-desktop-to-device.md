---
layout: post
title: Copying files from the Desktop to the Device using .NET
date: '2010-08-23T12:18:00.014+02:00'
author: Christian Resma Helle
tags:
- RAPI
modified_time: '2010-08-23T15:11:56.583+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6097183768724829755
blogger_orig_url: https://christian-helle.blogspot.com/2010/08/copying-files-from-desktop-to-device.html
---

Someone asked me recently how to copy files from the desktop to the device after reading my old article called [How to copy files from the Device to the Desktop using .NET](/2007/06/copying-files-from-device-to-desktop.html). Well here's how to do it:  
  
First, we'll need our P/Invokes to **rapi.dll**

```csharp
[DllImport("rapi.dll")]
static extern int CeRapiInit();
 
[DllImport("rapi.dll")]
static extern int CeRapiUninit();
 
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
internal static extern int CeWriteFile(
    IntPtr hFile,
    byte[] lpBuffer,
    int nNumberOfbytesToWrite,
    ref int lpNumberOfbytesWritten,
    int lpOverlapped);
 
[DllImport("rapi.dll", CharSet = CharSet.Unicode)]
internal static extern int CeSetFileTime(
    IntPtr hFile,
    ref long lpCreationTime,
    ref long lpLastAccessTime,
    ref long lpLastWriteTime);
 
const int BUFFER_SIZE = 1024 * 5; // 5k transfer buffer
const int CREATE_ALWAYS = 2;
const int ERROR_SUCCESS = 0;
const int FILE_ATTRIBUTE_NORMAL = 0x80;
const uint GENERIC_WRITE = 0x40000000;
const int INVALID_HANDLE_VALUE = -1;
```

Now let's wrap all those in a method called CopyToDevice(string localFile, string remoteFile). The localFile is the file located on the desktop and the remoteFile is the destination filename on the device.

```csharp
void CopyToDevice(string localFile, string remoteFile)
{
    var rapi = CeRapiInit() == ERROR_SUCCESS;
    if (!rapi)
        return;
 
    try
    {
        var filePtr = CeCreateFile(remoteFile, GENERIC_WRITE, 0, 0, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0);
        if (filePtr == new IntPtr(INVALID_HANDLE_VALUE))
            return;
 
        using (var localFileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
        {
            var byteswritten = 0;
            var position = 0;
            var buffer = new byte[BUFFER_SIZE];
 
            var bytesread = localFileStream.Read(buffer, position, BUFFER_SIZE);
            while (bytesread > 0)
            {
                position += bytesread;
                if (CeWriteFile(filePtr, buffer, bytesread, ref byteswritten, 0) == ERROR_SUCCESS)
                    return;
 
                try
                {
                    bytesread = localFileStream.Read(buffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    bytesread = 0;
                }
            }
        }
    }
    finally
    {
        CeRapiUninit();
    }
}  
```

To use the code above you will have to know the full path of the file on the desktop.  
I hope you found this useful.