---
layout: post
title: Cropping an Image in .NETCF
date: '2010-07-26T15:35:00.005+02:00'
author: Christian Resma Helle
tags:
- How to
- ".NET Compact Framework"
modified_time: '2010-07-26T15:40:17.490+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6669003286060208652
blogger_orig_url: https://christian-helle.blogspot.com/2010/07/cropping-image-in-netcf.html
---

I've seen some people ask how to crop an image in .NET Compact Framework in the community. Although the operation is pretty simple, I'll share a code snippet anyway.

```csharp
public static Image Crop(Image image, Rectangle bounds)
{
    Image newImage = new Bitmap(bounds.Width, bounds.Height);
 
    using (Graphics g = Graphics.FromImage(newImage))
        g.DrawImage(image, 
                    new Rectangle(0, 0, newImage.Width, newImage.Height), 
                    bounds, 
                    GraphicsUnit.Pixel);
 
    return newImage; 
}
```

What the code above does is to create a new image and draw part of the source image specified in the bounds to the new image.