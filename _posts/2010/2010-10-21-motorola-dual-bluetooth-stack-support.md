---
layout: post
title: Motorola Dual Bluetooth Stack Support
date: '2010-10-21T23:44:00.005+02:00'
author: Christian Resma Helle
tags:
- Windows Mobile
modified_time: '2010-10-21T23:59:03.290+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-2363429546681266049
blogger_orig_url: https://christian-helle.blogspot.com/2010/10/motorola-dual-bluetooth-stack-support.html
redirect_from:
- /blog/2010/10/21/motorola-dual-bluetooth-stack-support/
- /2010/10/21/motorola-dual-bluetooth-stack-support/
- /2010/10/motorola-dual-bluetooth-stack-support/
- /2010/motorola-dual-bluetooth-stack-support/
- /motorola-dual-bluetooth-stack-support/
---

Apparently most Motorola devices support 2 Bluetooth Stacks: Microsoft and StoneStreet. To switch which stack to use you would have to make some changes in the registry and restart the device.  
  
```
[HKEY_LOCAL_MACHINE\SOFTWARE\SymbolBluetooth]  
"SSStack"=DWORD:1
```

0 = Microsoft Stack  
1 = StoneStreet One Stack  
  
The StoneStreet One Stack is supported in all devices except the ES400 and MC65. For the Microsoft stack, any device running Windows Mobile 6.1, Windows Mobile 6.5.x and Windows CE 6.0  
  
Motorola recommendeds using the Microsoft stack for new development and I strongly agree with Motorola on this!