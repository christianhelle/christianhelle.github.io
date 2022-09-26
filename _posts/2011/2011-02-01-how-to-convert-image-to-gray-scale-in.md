---
layout: post
title: How to convert an image to gray scale in WPF
date: '2011-02-01T12:22:00.000+01:00'
author: Christian Resma Helle
tags:
- Image Manipulation
- WPF
modified_time: '2011-02-01T12:22:38.038+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-7134900830504549198
blogger_orig_url: https://christian-helle.blogspot.com/2011/02/how-to-convert-image-to-gray-scale-in.html
---

I've been playing with the Windows Presentation Foundation today and I had a task where I needed to convert an image to gray scale to do some image analysis on it. I've done this a bunch of times before using GDI methods or by accessing the BitmapData class in .NET. For this short post I'd like to demonstrate how to manipulate images using the WriteableBitmap class.

The easiest way to convert an image to gray scale is to set the RGB values of every pixel to the average of each pixels RBG values.

```
R = (R + B + G) / 3
G = (R + B + G) / 3
B = (R + B + G) / 3
```

Here's a code snippet for manipulating a BitmapSource object using the WriteableBitmap class into a gray scale image:

```csharp
public unsafe static BitmapSource ToGrayScale(BitmapSource source)
{
    const int PIXEL_SIZE = 4;
    int width = source.PixelWidth;
    int height = source.PixelHeight;
    var bitmap = new WriteableBitmap(source);
 
    bitmap.Lock();
    var backBuffer = (byte*)bitmap.BackBuffer.ToPointer();
    for (int y = 0; y < height; y++)
    {
        var row = backBuffer + (y * bitmap.BackBufferStride);
        for (int x = 0; x < width; x++)
        {
            var grayScale = (byte)(((row[x * PIXEL_SIZE + 1]) + 
                                    (row[x * PIXEL_SIZE + 2]) + 
                                    (row[x * PIXEL_SIZE + 3])) / 3);
            for (int i = 0; i < PIXEL_SIZE; i++)
                row[x * PIXEL_SIZE + i] = grayScale;
        }
    }
    bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
    bitmap.Unlock();
 
    return bitmap;
}
```

Another way to to convert an image to gray scale is to set the RGB values of every pixel to the sum of 30% of the red value, 59% of the green value, and 11% of the blue value. 

Hope you find this useful.