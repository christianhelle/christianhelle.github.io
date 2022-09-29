---
layout: post
title: Enumerating Bluetooth Devices from .NETCF
date: '2010-07-29T07:47:00.011+02:00'
author: Christian Resma Helle
tags:
- 32feet.NET
- ".NET Compact Framework"
modified_time: '2010-07-29T08:24:56.007+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6095815715489791463
blogger_orig_url: https://christian-helle.blogspot.com/2010/07/enumerating-bluetooth-devices-from.html
---

I recently had a project where I needed to send data to Bluetooth devices. The client applications where to run in several platforms, currently only J2ME (Nokia phones) and Windows Mobile phones. Windows Mobile actually offers a pretty decent Bluetooth stack but not all devices use this. One of the devices I needed to use used the Widcomm stack. Luckily, there is an open source project called [32feet.NET](https://github.com/inthehand/32feet) which came in very handy for providing a layer over the 2 different stacks I use. The 32feet.NET library was also incredibly easy and fun to use.

In this article I'd like to demonstrate how to enumerate Bluetooth devices using .NETCF and the 32feet.NET library. The following code will work on both Microsoft and Widcomm Bluetooth stacks:

```csharp
using System.Diagnostics;
using InTheHand.Net.Sockets;
 
namespace BluetoothSample
{
    static class Program
    {
        private static void Main()
        {
            BluetoothDeviceInfo[] devices;
            using (BluetoothClient sdp = new BluetoothClient())
                devices = sdp.DiscoverDevices();
 
            foreach (BluetoothDeviceInfo deviceInfo in devices)
            {
                Debug.WriteLine(string.Format("{0} ({1})",deviceInfo.DeviceName, deviceInfo.DeviceAddress));
            }
        }
    }
}
```

An interesting thing I had to consider for this project was the CPU architecture or endianness of the device I'm running on and the device I'm sending data to. I needed to reverse the byte order of the numeric data I sent and received.