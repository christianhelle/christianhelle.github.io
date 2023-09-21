---
layout: post
title: How to enumerate running programs in .NETCF
date: '2009-09-28T20:09:00.015+02:00'
author: Christian Resma Helle
tags:
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2010-06-11T09:44:14.816+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6357627196105166268
blogger_orig_url: https://christian-helle.blogspot.com/2009/09/how-to-enumerate-running-programs-in.html
---

I've been helping someone out in the MSDN Smart Device Forums lately with enumerating running programs. I thought I should share it since I don't much results from internet search engines on the topic. So here we go...  
  
To enumerate top-level windows we use the [EnumWindows](https://learn.microsoft.com/en-us/previous-versions/ms960376(v=msdn.10)?redirectedfrom=MSDN) method. To enumerate the running programs the same way the Task manager in Windows Mobile does, we just filter out what we don't need. What we don't want are windows that:  
  
1. Have a parent window - We check by calling [GetParent()](https://learn.microsoft.com/en-us/previous-versions/ms960750(v=msdn.10)?redirectedfrom=MSDN)  
2. Is not visible - We check by calling [IsWindowVisible()](https://learn.microsoft.com/en-us/previous-versions/ms915286(v=msdn.10)?redirectedfrom=MSDN)  
3. Tool windows - We check by getting the current extended style (through [GetWindowLong()](https://learn.microsoft.com/en-us/library/ms960886.aspx?WT.mc_id=DT-MVP-5004822) of the window and checking if `WS_EX_TOOLWINDOW` is set  
  
Here's a simple smart device console application the loads and displays a list of running programs:

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
 
namespace EnumWindowsNETCF
{
    class Program
    {
        static Dictionary<string, IntPtr> visibleWindows = new Dictionary<string, IntPtr>();
        static StringBuilder lpString;
        static bool visible;
        static bool hasOwner;
        static bool isToolWindow;
 
        delegate int WNDENUMPROC(IntPtr hwnd, uint lParam);
        const int GWL_EXSTYLE = -20;
        const uint WS_EX_TOOLWINDOW = 0x0080;
 
        [DllImport("coredll.dll")]
        static extern int EnumWindows(WNDENUMPROC lpEnumWindow, uint lParam);
 
        [DllImport("coredll.dll")]
        static extern bool IsWindowVisible(IntPtr hwnd);
 
        [DllImport("coredll.dll")]
        static extern IntPtr GetParent(IntPtr hwnd);
 
        [DllImport("coredll.dll")]
        static extern bool GetWindowText(IntPtr hwnd, StringBuilder lpString, int nMaxCount);
 
        [DllImport("coredll.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int nIndex);
 
        static int Callback(IntPtr hwnd, uint lParam)
        {
            hasOwner = GetParent(hwnd) != IntPtr.Zero;
            visible = IsWindowVisible(hwnd);
            isToolWindow = (GetWindowLong(hwnd, GWL_EXSTYLE) & WS_EX_TOOLWINDOW) != 0;
            lpString.Remove(0, lpString.Length);
            GetWindowText(hwnd, lpString, 1024);
 
            string key = lpString.ToString();
            if (!hasOwner &&
                visible &&
                !isToolWindow &&
                !string.IsNullOrEmpty(key) &&
                !visibleWindows.ContainsKey(key))
            {
                visibleWindows.Add(key, hwnd);
            }
 
            return 1;
        }
 
        static void Main()
        {
            lpString = new StringBuilder(1024);
            EnumWindows(Callback, 0);
 
            foreach (var key in visibleWindows.Keys)
            {
                IntPtr hwnd = visibleWindows[key];
                visible = IsWindowVisible(hwnd);
                lpString.Remove(0, lpString.Length);
                GetWindowText(hwnd, lpString, 1024);
 
                Debug.WriteLine("Handle: " + hwnd + "; Is Visible: " + visible + "; Text: " + lpString);
            }
        }
    }
}
```

I hope you find this useful, otherwise it can always be an addition to your utility library.