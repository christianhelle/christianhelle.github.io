---
layout: post
title: How to Darken an Image in WPF
date: '2011-02-04T01:00:00.002+01:00'
author: Christian Resma Helle
tags:
- Image Manipulation
- WPF
modified_time: '2011-02-04T01:00:04.655+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-7904233113719038040
blogger_orig_url: https://christian-helle.blogspot.com/2011/02/how-to-darken-image-in-wpf.html
redirect_from:
- /blog/2011/02/04/how-to-darken-an-image-in-wpf/
- /2011/02/04/how-to-darken-an-image-in-wpf/
- /2011/02/how-to-darken-an-image-in-wpf/
- /2011/how-to-darken-an-image-in-wpf/
- /how-to-darken-an-image-in-wpf/
---

I'm really getting carried away with playing with image manipulation in WPF. Here's a short post on how to darken an image using the WriteableBitmap class.  
  
The process is fairly simple, I manipulate each pixel by decrementing each RGB value with the provided level  
  
```csharp
public unsafe static BitmapSource Darken(BitmapSource image, double level)
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
                row[x * PIXEL_SIZE + i] = (byte)Math.Max(row[x * PIXEL_SIZE + i] - level, 0);
    }
 
    bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
    bitmap.Unlock();
 
    return bitmap;
}
```  
  
Hope you found this useful.