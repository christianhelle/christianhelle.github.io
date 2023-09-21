---
layout: post
title: How to enumerate files on a Windows CE based device from the Desktop
date: '2010-04-29T13:02:00.032+02:00'
author: Christian Resma Helle
tags:
- RAPI
- How to
- ".NET Compact Framework"
modified_time: '2010-04-29T19:32:31.886+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-5976418551323130243
blogger_orig_url: https://christian-helle.blogspot.com/2010/04/how-to-enumerate-files-on-windows-ce.html
redirect_from:
- /blog/2010/04/29/how-to-enumerate-files-on-a-windows-ce-based-device-from-the-desktop/
---

In this article I'd like to demonstrate how to enumerate files on a Windows CE based device from the desktop.  
  
Listing the contents of a directly on a Windows CE from the desktop is something I found to be useful every now and then. It involves using the [Remote API](http://learn.microsoft.com/en-us/library/aa920177.aspx?WT.mc_id=DT-MVP-5004822) and ActiveSync / Windows Mobile Device Center  
  
As usual this will involve a few P/Invokes:  

```csharp
[DllImport("rapi.dll", CharSet = CharSet.Unicode)]
static extern int CeRapiInit();
 
[DllImport("rapi.dll", CharSet = CharSet.Unicode)]
static extern int CeRapiUninit();
 
[DllImport("rapi.dll", CharSet = CharSet.Unicode)]
static extern IntPtr CeFindFirstFile(string lpFileName, ref CE_FIND_DATA lpFindFileData);
 
[DllImport("rapi.dll", CharSet = CharSet.Unicode)]
static extern bool CeFindNextFile(IntPtr hFindFile, ref CE_FIND_DATA lpFindFileData);
 
[DllImport("rapi.dll", CharSet = CharSet.Unicode)]
static extern bool CeFindClose(IntPtr hFindFile);
```

We also need the CE_FIND_DATA structure

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
struct CE_FIND_DATA
{
    public int dwFileAttributes;
    public FILETIME ftCreationTime;
    public FILETIME ftLastAccessTime;
    public FILETIME ftLastWriteTime;
    public int nFileSizeHigh;
    public int nFileSizeLow;
    public int dwOID;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string cFileName;
};
 
[StructLayout(LayoutKind.Sequential)]
struct FILETIME
{
    public int dwLowDateTime;
    public int dwHighDateTime;
}
```

The [CeRapiInit](http://learn.microsoft.com/en-us/library/aa922061.aspx?WT.mc_id=DT-MVP-5004822) method has be always called before performing Remote API operations. Once done, the [CeRapiUninit](http://learn.microsoft.com/en-us/library/aa918093.aspx?WT.mc_id=DT-MVP-5004822) must be called. For listing the files in a directory, I use [CeFindFirstFile](http://learn.microsoft.com/en-us/library/aa917424.aspx?WT.mc_id=DT-MVP-5004822), [CeFindNextFile](http://learn.microsoft.com/en-us/library/aa918923.aspx?WT.mc_id=DT-MVP-5004822), and [CeFindClose](http://learn.microsoft.com/en-us/library/aa917593.aspx?WT.mc_id=DT-MVP-5004822). How this works: If the file(s) exists CeFindFirstFile will return a valid handle that can be used for calling CeFindNextFile. After going through all the files CeFindNextFile will return false and a call to CeFindClose needs to be called.  

```csharp
public static string[] GetFiles(string remoteDirectory)
{
    try
    {
        CeRapiInit();
 
        var list = new List<string>();
        var findData = new CE_FIND_DATA();
        var hFindFile = CeFindFirstFile(remoteDirectory + "\\*", ref findData);
 
        if (hFindFile != new IntPtr(-1))
        {
            try
            {
                do
                {
                    if (findData.dwFileAttributes != (int)FileAttributes.Directory)
                        list.Add(findData.cFileName);
                } while (CeFindNextFile(hFindFile, ref findData));
            }
            finally
            {
                CeFindClose(hFindFile);
            }
        }
 
        return list.ToArray();
    }
    finally
    {
        CeRapiUninit();
    }
}
```

I hope you found this useful.