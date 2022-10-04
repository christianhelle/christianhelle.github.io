---
layout: post
title: Displaying the Calendar view on a DateTimePicker Control in .NETCF
date: '2007-07-23T20:30:00.000+02:00'
author: Christian Resma Helle
tags:
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2007-07-24T08:46:21.711+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-3866100767263295435
blogger_orig_url: https://christian-helle.blogspot.com/2007/07/displaying-calendar-view-on.html
---

I've recently made a solution where the customer requested to be able to bring up the calendar view in a `DateTimePicker` control by pressing on a specific button on the screen. The solution to that was really simple: Create a control that inherits from `System.Windows.Forms.DateTimePicker` and add a method called `ShowCalendar()` which I call to bring up the Calendar view.

```csharp
public class DateTimePickerEx : DateTimePicker
{
    [DllImport("coredll.dll")]
    static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, int lParam);

    const int WM_LBUTTONDOWN = 0x0201;

    public void ShowCalendar() 
    {
        int x = Width - 10;
        int y = Height / 2;
        int lParam = x + y * 0x00010000;

        SendMessage(Handle, WM_LBUTTONDOWN, 1, lParam);
    }
}
```