---
layout: post
title: How to toggle the Wi-fi radio
date: '2010-01-20T13:01:00.036+01:00'
author: Christian Resma Helle
tags:
- Native Code
- How to
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2010-01-20T14:22:23.637+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-9155466612543890030
blogger_orig_url: https://christian-helle.blogspot.com/2010/01/how-to-toggle-wi-fi-radio.html
redirect_from:
- /blog/2010/01/20/how-to-toggle-the-wi-fi-radio/
- /2010/01/20/how-to-toggle-the-wi-fi-radio/
- /2010/01/how-to-toggle-the-wi-fi-radio/
- /2010/how-to-toggle-the-wi-fi-radio/
- /how-to-toggle-the-wi-fi-radio/
---

I had a task to complete today where I was to create an application to toggle the Wi-Fi radio. I had two major requirements for this task; I was supposed to not spend more an hour on this and it must run on older devices running Pocket PC 2003 (or older)  
  
This is what I came up with, 1 function (the entry point) and it uses only 3 power management API calls; [GetDevicePower](https://learn.microsoft.com/en-us/previous-versions/ms889220(v=msdn.10)?WT.mc_id=DT-MVP-5004822), [DevicePowerNotify](https://learn.microsoft.com/en-us/previous-versions/ms896927(v=msdn.10)?WT.mc_id=DT-MVP-5004822), and [SetDevicePower](https://learn.microsoft.com/en-us/previous-versions/ms889493(v=msdn.10)?WT.mc_id=DT-MVP-5004822)  
  
Basically I spent most of the time finding the device name for the wireless device. It seems to be pretty used for Intermec devices as I tested it on 3 different devices (or it could also be that only those 3 devices used the same device name)  
  
Anyway, here's the code (works only for Intermec devices):  
  
```c
#include <windows.h>
#include <pm.h>
 
#define INTERMEC_WIFI_DEVICE    L"{98C5250D-C29A-4985-AE5F-AFE5367E5006}\\BCMCF1"
 
int _tmain(int argc, _TCHAR* argv[])
{
    CEDEVICE_POWER_STATE state;
    GetDevicePower(INTERMEC_WIFI_DEVICE, POWER_NAME, &state);
    CEDEVICE_POWER_STATE newState = (state == D0) ? D4 : D0;
 
    DevicePowerNotify(INTERMEC_WIFI_DEVICE, newState, POWER_NAME);
    SetDevicePower(INTERMEC_WIFI_DEVICE, POWER_NAME, newState);
}
```  
  
Normally when I experiment with the platform SDK, I just create native console applications and test how the function works. Since my application was simple and didn't need a UI, I just shipped it in native code.  
  
But for the sake of sharing knowledge I ported my tiny application to the .NET Compact Framework. Here's the code (works only for Intermec devices):  
  
```csharp
[DllImport("coredll.dll", SetLastError = true)]
static extern int DevicePowerNotify(string name, CEDEVICE_POWER_STATE state, int flags);
 
[DllImport("coredll.dll", SetLastError = true)]
static extern int SetDevicePower(string pvDevice, int dwDeviceFlags, CEDEVICE_POWER_STATE DeviceState);
 
[DllImport("coredll.dll", SetLastError = true)]
static extern int GetDevicePower(string pvDevice, int dwDeviceFlags, ref CEDEVICE_POWER_STATE pDeviceState);
 
enum CEDEVICE_POWER_STATE : int
{
    PwrDeviceUnspecified = -1,
    D0 = 0,
    D1 = 1,
    D2 = 2,
    D3 = 3,
    D4 = 4,
    PwrDeviceMaximum = 5
}
 
const int POWER_NAME = 0x00000001;
const string ADAPTER_NAME = "{98C5250D-C29A-4985-AE5F-AFE5367E5006}\\BCMCF1";
 
static void Main()
{
    CEDEVICE_POWER_STATE state = CEDEVICE_POWER_STATE.PwrDeviceUnspecified;
    GetDevicePower(ADAPTER_NAME, POWER_NAME, ref state);
    CEDEVICE_POWER_STATE newState = (state == CEDEVICE_POWER_STATE.D0)
        ? CEDEVICE_POWER_STATE.D4
        : CEDEVICE_POWER_STATE.D0;
 
    DevicePowerNotify(ADAPTER_NAME, newState, POWER_NAME);
    SetDevicePower(ADAPTER_NAME, POWER_NAME, newState);
}
```

There are smarter, better, and non-OEM specific ways to do this, both in native and managed code. In native code, one can use the wireless device functions (GetWirelessDevice, ChangeRadioState, FreeDeviceList) in the Wireless Device Power Management API (OSSVCS.dll) as described in [this](https://www.codeproject.com/Articles/103104/Radio-Power) article. And in managed code, one can take advantage of the [OpenNETCF Smart Device Framework](https://github.com/ctacke/sdf).  
  
Here's an example of how to use the OpenNETCF.WindowsMobile namespace in the Smart Device Framework for toggling the state of wireless devices:
  
```csharp
using System.Linq;
using OpenNETCF.WindowsMobile;
 
static class Program
{
    static void Main()
    {
        var wifi = from radio in Radios.GetRadios()
                   where radio.RadioType == RadioType.WiFi
                   select radio;
 
        foreach (var radio in wifi)
            radio.RadioState = (radio.RadioState == RadioState.On) ? RadioState.On : RadioState.Off;
    }
}
``` 
  
I hope you found this article useful