---
layout: post
title: How to get the IP Address of a device in .NETCF
date: '2009-12-25T15:18:00.002+01:00'
author: Christian Resma Helle
tags:
- How to
- ".NET Compact Framework"
modified_time: '2009-12-25T15:31:16.179+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-7561193013404732791
blogger_orig_url: https://christian-helle.blogspot.com/2009/12/how-to-get-ip-address-of-device-in.html
---

Here's something I see asked every now and then in the community forums. The solution is a one liner that looks like this:

```csharp
IPAddress[] addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
```

The code above retrieves the IP addresses for each connected network adapter of the device. The [Dns](https://learn.microsoft.com/en-us/dotnet/api/system.net.dns) and [IPAddress](https://learn.microsoft.com/en-us/dotnet/api/system.net.ipaddress) classes belong to the [System.Net](https://learn.microsoft.com/en-us/dotnet/api/system.net) namespace.