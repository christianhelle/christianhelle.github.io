---
layout: post
title: How to Alpha Blend 2 Images in WPF
date: '2011-02-02T00:22:00.000+01:00'
# author: Christian Resma Helle
tags:
- Image Manipulation
- WPF
modified_time: '2011-02-02T00:22:46.986+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-1726784415064280006
blogger_orig_url: https://christian-helle.blogspot.com/2011/02/how-to-alpha-blend-2-images-in-wpf.html
redirect_from:
- /2011/02/how-to-alpha-blend-2-images-in-wpf/
- /2011/02/how-to-alpha-blend-2-images-in-wpf
---

After having such fun trying to find optimal ways of manipulating images in WPF I decided to write another short post on image manipulation. This time I'd like to demonstrate how to alpha blend 2 images using the WriteableBitmap class.

I'm probably not the best one to explain how alpha blending is done but here's the idea in a nutshell. I get the RGB values of every pixel for the each image and write them to a new bitmap where I manipulate each color information by applying the following formula:

```
r = ((image1 pixel (red) * alpha level) + (image2 pixel (red) * inverse alpha level)) / 256
b = ((image1 pixel (blue) * alpha level) + (image2 pixel (blue) * inverse alpha level)) / 256
g = ((image1 pixel (green) * alpha level) + (image2 pixel (green) * inverse alpha level)) / 256
```

```csharp
public unsafe static WriteableBitmap AlphaBlend(BitmapSource image1, BitmapSource image2, int alphaLevel)
{
    const int PIXEL_SIZE = 4;
    int ialphaLevel = 256 - alphaLevel;
    int height = Math.Min(image1.PixelHeight, image2.PixelHeight);
    int width = Math.Min(image1.PixelWidth, image2.PixelWidth);
 
    var bitmap = new WriteableBitmap(width, height, image1.DpiX, image1.DpiY, PixelFormats.Bgr32, null);
    var bitmap1 = new WriteableBitmap(image1);
    var bitmap2 = new WriteableBitmap(image2);
 
    bitmap.Lock();
    bitmap1.Lock();
    bitmap2.Lock();
 
    var backBuffer = (byte*)bitmap.BackBuffer.ToPointer();
    var bitmap1Buffer = (byte*)bitmap1.BackBuffer.ToPointer();
    var bitmap2Buffer = (byte*)bitmap2.BackBuffer.ToPointer();
 
    for (int y = 0; y < height; y++)
    {
        var row = backBuffer + (y * bitmap.BackBufferStride);
        var img1Row = bitmap1Buffer + (y * bitmap1.BackBufferStride);
        var img2Row = bitmap2Buffer + (y * bitmap2.BackBufferStride);
 
        for (int x = 0; x < width; x++)
            for (int i = 0; i < PIXEL_SIZE; i++)
                row[x * PIXEL_SIZE + i] = (byte)(((img1Row[x * PIXEL_SIZE + i] * alphaLevel) + (img2Row[x * PIXEL_SIZE + i] * ialphaLevel)) >> 8);
    }
 
    bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
    bitmap2.Unlock();
    bitmap1.Unlock();
    bitmap.Unlock();
 
    return bitmap;
}
```

The method above will probably work best if the 2 images are of the same size. I hope you found this information useful.