---
layout: post
title: How to switch on/off the speaker phone in .NETCF
date: '2009-12-21T19:59:00.000+01:00'
author: Christian Resma Helle
tags:
- How to
- ".NET Compact Framework"
modified_time: '2009-12-21T20:41:39.393+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-7595127709834734069
blogger_orig_url: https://christian-helle.blogspot.com/2009/12/how-to-switch-onoff-speaker-phone-in.html
---

A few years back, I stumbled upon [this article](http://www.teksoftco.com/articles/article%20006/speakerphone.htm) while trying to find a solution on how to switch on/off the speaker phone. It uses a DLL found in Windows Mobile 5 (and higher) devices called ossvcs.dll. This library exposes some pretty neat API's for controlling communication related features in the device (like controlling the wireless/bluetooth radio and other cool stuff).

Here's a quick way for switching the speaker on/off in .NETCF

```csharp
DllImport("ossvcs.dll", EntryPoint = "#218")]        
static extern int SetSpeakerMode(uint mode);
 
void EnableSpeakerPhone()
{
    SetSpeakerMode(1);
}
 
void DisableSpeakerPhone()
{
    SetSpeakerMode(0);
}
```

Unfortunately, **ossvcs.dll** is not documented and might not exist in the future. But for now, it pretty much works in all devices I've tried