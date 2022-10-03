---
layout: post
title: Resizing an Image in .NETCF
date: '2009-10-18T08:21:00.001+02:00'
author: Christian Resma Helle
tags:
- Image Manipulation
- How to
- ".NET Compact Framework"
modified_time: '2009-12-20T23:32:34.109+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-7669035359479446515
blogger_orig_url: https://christian-helle.blogspot.com/2009/10/resizing-image-in-netcf.html
---

I've seen some people ask how to resize an image or how to stretch an image in .NET Compact Framework in the community. Although the operation is pretty simple, I'll share a code snippet anyway.

```csharp
public static Image Resize(Image image, Size size)
{
    Image bmp = new Bitmap(size.Width, size.Height);
    using (var g = Graphics.FromImage(bmp))
    {
        g.DrawImage(
            image,
            new Rectangle(0, 0, size.Width, size.Height),
            new Rectangle(0, 0, image.Width, image.Height),
            GraphicsUnit.Pixel);
    }
    return bmp;
}
```

What the code above does is to create a new image with the specified new size and draw the source image to fit the new image.