---
layout: post
title: Widcomm Bluetooth Pairing Prompt
date: '2010-10-20T12:58:00.017+02:00'
author: Christian Resma Helle
tags:
- ".NET Compact Framework"
modified_time: '2010-10-20T13:22:22.073+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-7477684092160171285
blogger_orig_url: https://christian-helle.blogspot.com/2010/10/widcomm-bluetooth-pairing-prompt.html
---

Not so long ago I had a project that involved Bluetooth communication from a windows mobile device to some custom designed hardware. Everything went smoothly with only a few bumps during the scope of the project. When we had the applications on the field, certain users where running on devices that used the Widcomm Bluetooth Stack and that gave us a few headaches. For one, our system doesn't use any security features in bluetooth hence it doesn't not require pairing. Our security architecture is service based and checks for certain keys being passed back and forth to an online service. Very standard stuff. The problem we had with the devices running on a Widcomm Bluetooth stack was that the user was always prompted to pair even though the device did not require pairing. The application was not designed to handle user input aside from logging in/out and some diagnostic features. The application was designed to just run quietly in the background with the device tucked in the users pocket.

Since quite a few devices use the Widcomm Bluetooth Stack I needed a quick fix/hack to avoid the user having to pick up the device and click "Yes" on the security prompt to communicate with the device. The fix had to be as simple as possible and had to run without disrupting the user. My not so clean solution was to create a small application that does nothing but check if the Widcomm security pairing prompt app was running and if so send a keystroke event to simulate the user clicking on "Yes".

The Widcomm security prompt app main window uses the class name **Broadcom_BTWizard** with the window name **Bluetooth**. I check if this window exists and send the F1 keyboard event to simulate clicking on the left hardware button on the device

Here's a code snippet in C#

```csharp
[DllImport("coredll.dll")]
static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
 
[DllImport("coredll")]
static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
 
const int KEYEVENTF_KEYUP = 0x02;
const int KEYEVENTF_KEYDOWN = 0x00;
static bool running = true;
 
static void CloseBroadcomWindowWorker()
{
    while (running)
    {
        var hwnd = FindWindow("Broadcom_BTWizard", "Bluetooth");
        if (hwnd != IntPtr.Zero)
        {
            keybd_event((byte)Keys.F1, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event((byte)Keys.F1, 0, KEYEVENTF_KEYUP, 0);
        }
 
        Thread.Sleep(5000);
    }
}
```

This is of course not the best solution but if you have a similar problem then this might save you some time if your only requirement is to get it to work as soon as possible. Otherwise, I would suggest avoiding the Widcomm Bluetooth Stack and just go for devices that use the Microsoft Bluetooth Stack