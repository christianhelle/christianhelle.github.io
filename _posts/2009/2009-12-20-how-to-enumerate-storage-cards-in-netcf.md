---
layout: post
title: How to enumerate storage cards in .NETCF
date: '2009-12-20T23:38:00.003+01:00'
author: Christian Resma Helle
tags:
- How to
- ".NET Compact Framework"
modified_time: '2009-12-20T23:45:02.416+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6144268673907065572
blogger_orig_url: https://christian-helle.blogspot.com/2009/12/how-to-enumerate-storage-cards-in-netcf.html
---

In Windows Mobile, external storage cards are usually represented as the `'\SD Card'` or `'\Storage Card'` folder, this varies from device to device. A simple way to enumerate the external storage cards is to check all directories under the root directory that have the temporary attribute flag set.

Here's a quick way to do so in .NETCF:

```csharp
public static List<string> GetStorageCards()
{
    var list = new List<string>();
    var root = new DirectoryInfo("\\");
    foreach (DirectoryInfo directory in root.GetDirectories()) 
    {
        if (FileAttributes.Temporary == (directory.Attributes & FileAttributes.Temporary))
            list.Add(directory.Name);
    }
    return list;
}
```