---
layout: post
title: Programmatically Refreshing the Today Screen
date: '2007-06-06T08:04:00.000+02:00'
author: Christian Resma Helle
tags:
- Windows Mobile
modified_time: '2007-06-11T09:45:04.556+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-9105916737634152947
blogger_orig_url: https://christian-helle.blogspot.com/2007/06/programmatically-refreshing-today.html
---

A simple trick for forcing the today screen to re-read from the registry (or refresh) is by sending the message `WM_WININICHANGE` with the parameter `0x000000F2` to a window called the `DesktopExplorerWindow`

Here's a small code snippet on how to accomplish this programmatically:

```c
void RefreshTodayScreen() 
{
    HWND hWnd = FindWindow(_T("DesktopExplorerWindow"), _T("Desktop"));
    SendMessage(hWnd, WM_WININICHANGE, 0x000000F2, 0);
}
```

and in managed code...

```csharp
[DllImport("coredll.dll")]
static extern IntPtr FindWindow(string class_name, string caption);

[DllImport("coredll.dll")]
static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

const uint WM_WININICHANGE = 0x1a;

void RefreshTodayScreen() {
  IntPtr hWnd = FindWindow("DesktopExplorerWindow", "Desktop");
  SendMessage(hWnd, WM_WININICHANGE, 0x000000F2, 0);
}
```