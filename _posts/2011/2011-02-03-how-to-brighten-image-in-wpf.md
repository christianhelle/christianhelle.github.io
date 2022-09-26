---
layout: post
title: How to Brighten an Image in WPF
date: '2011-02-03T10:00:00.010+01:00'
author: Christian Resma Helle
tags:
- Image Manipulation
- WPF
modified_time: '2011-02-03T10:00:17.946+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-1929113951975840048
blogger_orig_url: https://christian-helle.blogspot.com/2011/02/how-to-brighten-image-in-wpf.html
---

Now I'm just getting carried away with playing with image manipulation in WPF. Here's a short post on how to brighten an image using the WriteableBitmap class.

The process is fairly simple, I manipulate each pixel by incrementing each RGB value with the provided level

```csharp
public unsafe static BitmapSource Brighten(BitmapSource image, double level)
{
    const int PIXEL_SIZE = 4;
    int height = image.PixelHeight;
    int width = image.PixelWidth;
 
    var bitmap = new WriteableBitmap(image);            
    bitmap.Lock();
 
    var backBuffer = (byte*)bitmap.BackBuffer.ToPointer();
    for (int y = 0; y < height; y++)
    {
        var row = backBuffer + (y * bitmap.BackBufferStride);
        for (int x = 0; x < width; x++)
            for (int i = 0; i < PIXEL_SIZE; i++)
                row[x * PIXEL_SIZE + i] = (byte)Math.Min(row[x * PIXEL_SIZE + i] + level, 255);
    }
 
    bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
    bitmap.Unlock();
 
    return bitmap;
}
```

Hope you found this useful