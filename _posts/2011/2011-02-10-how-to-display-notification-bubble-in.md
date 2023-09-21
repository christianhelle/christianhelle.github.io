---
layout: post
title: How to display a Notification Bubble in Windows Mobile using .NETCF
date: '2011-02-10T15:15:00.001+01:00'
author: Christian Resma Helle
tags:
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2011-02-10T15:18:39.363+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-8867165319107967945
blogger_orig_url: https://christian-helle.blogspot.com/2011/02/how-to-display-notification-bubble-in.html
---

Yesterday, I found myself using an old piece of code that I wrote ages ago. It's something I've used every now and then for past few years. Since I myself find it useful, I might as well share it. All the code does is display a Notification Bubble in Windows Mobile. To do this you use the [Notification](https://learn.microsoft.com/en-us/library/microsoft.windowsce.forms.notification.aspx?WT.mc_id=DT-MVP-5004822) class in the Microsoft.WindowsCE.Forms namespace. Even though the Notification class is very straight forward and easy to use, I created a helper class so that I only need to write one line of code for displaying a notification bubble: 

```csharp
NotificationBubble.Show(2, "Caption", "Text");  
```

Implementation:

```csharp
/// <summary>
/// Used for displaying a notification bubble
/// </summary>
public static class NotificationBubble
{
    /// <summary>
    /// Displays a notification bubble
    /// </summary>
    /// <param name="duration">Duration in which the notification bubble is shown (in seconds)</param>
    /// <param name="caption">Caption</param>
    /// <param name="text">Body</param>
    public static void Show(int duration, string caption, string text)
    {
        var bubble = new Notification
        {
            InitialDuration = duration,
            Caption = caption,
            Text = text
        };
 
        bubble.BalloonChanged += OnBalloonChanged;
        bubble.Visible = true;
    }
 
    private static void OnBalloonChanged(object sender, BalloonChangedEventArgs e)
    {
        if (!e.Visible)
            ((Notification)sender).Dispose();
    }
}
```

Hope you found this helpful.