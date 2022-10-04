---
layout: post
title: Retrieving the Icon Image within the System Image List in .NETCF 2.0
date: '2007-06-12T19:07:00.001+02:00'
author: Christian Resma Helle
tags:
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2007-06-14T20:39:39.543+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-9075719262103873058
blogger_orig_url: https://christian-helle.blogspot.com/2007/06/retrieving-icon-image-within-system_295.html
---

Here's a nice trick for retrieving the icon image of a file or folder from the system image list. All we actually need is to P/Invoke `SHGetFileInfo` and use `Icon.FromHandle()` to get the Icon.  
  
First, we need to declare our P/Invokes.  
  
```csharp
[StructLayout(LayoutKind.Sequential)]  
stru-ct SHFILEINFO  
{  
  public IntPtr hIcon;  
  public IntPtr iIcon;  
  public uint dwAttributes;  
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]  
  public string szDisplayName;  
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]  
  public string szTypeName;  
}  
  
const uint SHGFI_ICON = 0x000000100;  
const uint SHGFI_LARGEICON = 0x000000000;  
const uint SHGFI_SMALLICON = 0x000000001;  
const uint SHGFI_SELECTICON = 0x000040000;  
  
[DllImport("coredll.dll")]  
static extern IntPtr SHGetFileInfo(
    string pszPath, 
    uint dwFileAttributes,  
    ref SHFILEINFO psfi, 
    uint cbSizeFileInfo, 
    uint uFlags);  
```
  
To get an instance of System.Drawing.Icon for the small icon of a file  
  
```csharp
Icon GetSystemIconSmall(string file)  
{  
  SHFILEINFO shinfo = new SHFILEINFO();  
  IntPtr i = SHGetFileInfo(file, 0, ref shinfo,  
    (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);  
  
  return Icon.FromHandle(shinfo.hIcon);  
}  
```
  
For the large icon of a file  
  
```csharp
Icon GetSystemIconLarge(string file)  
{  
  SHFILEINFO shinfo = new SHFILEINFO();  
  IntPtr i = SHGetFileInfo(file, 0, ref shinfo,  
    (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);  
  
  return Icon.FromHandle(shinfo.hIcon);  
}  
```  
  
For the small icon of a file when it is selected  

```csharp 
Icon GetSystemIconSmallSelected(string file)  
{  
  SHFILEINFO shinfo = new SHFILEINFO();  
  IntPtr i = SHGetFileInfo(file, 0, ref shinfo,  
    (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON | SHGFI_SELECTICON);  
  
  return Icon.FromHandle(shinfo.hIcon);  
}  
```
  
And last for the large icon of a file when it is selected  
  
```csharp
Icon GetSystemIconLargeSelected(string file)  
{  
  SHFILEINFO shinfo = new SHFILEINFO();  
  IntPtr i = SHGetFileInfo(file, 0, ref shinfo,  
    (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON | SHGFI_SELECTICON);  
  
  return Icon.FromHandle(shinfo.hIcon);  
}  
```

Ok, now how is this helpful? Well if you want to implement a File Explorer-ish control, then wouldn't have to include Icons and other images in your application. You can just use the icons in the system image list