---
layout: post
title: Programmatically Minimize an Application in .NET CF 2.0
date: '2007-06-06T13:53:00.000+02:00'
author: Christian Resma Helle
tags:
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2007-06-11T09:45:53.706+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-3618804870290246980
blogger_orig_url: https://christian-helle.blogspot.com/2007/06/programmatically-minimize-application.html
---

I once made a solution that runs on full screen. The solution was written completely in managed code (except for the CE Setup and other small stuff..). Since I took over the screen completely, I don't have access to the (X) button in the upper right corner of the screen. I wanted the application running at all times, but I also wanted the user to be able to get in and out of the application. Since the user won't be able to access the Start button, I added a "Close" button to my application. This "Close" button won't exit the application, instead it will just Minimize the application.

In the .NET Compact Framework 2.0, you can't just set the form's `WindowState` to `WindowState.Minimized` since the `WindowState` enum only contains `Normal` and `Maximized`. Currently, the only way you can programmatically minimize an application is by doing a P/Invoke to ShowWindow and passing `SW_MINIMIZE` to specify how the window will be displayed. It is also required that your Form has the Taskbar visible, this is done by setting the following properties:

```csharp
  FormBorderStyle = FormBorderStyle.FixedDialog;
  WindowState = FormWindowState.Normal;
  ControlBox = true;
  MinimizeBox = true;
  MaximizeBox = true;
```

Here's a small code snippet of how to minimize your application

```csharp
[DllImport("coredll.dll")]
static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

const int SW_MINIMIZED = 6;

void Minimize() {
    // The Taskbar must be enabled to be able to do a Smart Minimize
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.WindowState = FormWindowState.Normal;
    this.ControlBox = true;
    this.MinimizeBox = true;
    this.MaximizeBox = true;

    // Since there is no WindowState.Minimize, we have to P/Invoke ShowWindow
    ShowWindow(this.Handle, SW_MINIMIZED);
}
```