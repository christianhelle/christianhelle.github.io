---
layout: post
title: How to Vibrate a Smartphone / Windows Mobile Standard device in .NETCF
date: '2009-12-23T15:20:00.004+01:00'
author: Christian Resma Helle
tags:
- How to
- ".NET Compact Framework"
modified_time: '2009-12-25T15:19:13.010+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-8474329956653460126
blogger_orig_url: https://christian-helle.blogspot.com/2009/12/how-to-vibrate-smartphone-windows.html
---

The Vibrate API is only available in the Windows Mobile Standard or Smartphone platform. This has been so since Smartphone 2002. Calling Vibrate() will cause the device to vibrate, and VibrateStop() will stop it.

Here's a simple managed wrapper for the Vibrate API

```csharp
public class Vibrate
{
    [DllImport("aygshell.dll")]
    static extern int Vibrate(int cvn, IntPtr rgvn, bool fRepeat, uint dwTimeout);
 
    [DllImport("aygshell.dll")]
    static extern int VibrateStop();
 
    const uint INFINITE = 0xffffffff;
 
    public static bool Play()
    {
        VibratePlay(0, IntPtr.Zero, true, INFINITE);
    }
 
    public static void Stop()
    {
        VibrateStop();
    }
}
```

For more information on the Vibrate API, you might want to check the [MSDN Documentation](https://msdn.microsoft.com/en-us/library/bb416473.aspx?WT.mc_id=DT-MVP-5004822) out.