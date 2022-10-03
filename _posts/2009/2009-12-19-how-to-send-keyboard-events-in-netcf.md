---
layout: post
title: How to send keyboard events in .NETCF
date: '2009-12-19T23:27:00.000+01:00'
author: Christian Resma Helle
tags:
- How to
- ".NET Compact Framework"
modified_time: '2009-12-21T20:40:52.506+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-1571210236470570394
blogger_orig_url: https://christian-helle.blogspot.com/2009/12/how-to-send-keyboard-events-in-netcf.html
---

I've seen some people ask how to how to send keyboard events in .NET Compact Framework in the community. Although the operation is pretty simple, I'll share a code snippet anyway.

```csharp
const int KEYEVENTF_KEYPRESS = 0x0000;
const int KEYEVENTF_KEYUP = 0x0002;
 
[DllImport("coredll.dll")]
static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
 
void SendKey(Keys key)
{
    keybd_event((byte) key, 0, KEYEVENTF_KEYPRESS, 0);
    keybd_event((byte) key, 0, KEYEVENTF_KEYUP, 0);
}
```

The code above will send the keyboard event to currently focused window or control