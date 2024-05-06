---
layout: post
title: Working with Native Bitmap pixel buffers in Xamarin.Forms
date: '2016-09-01T00:30:00.000+02:00'
author: Christian Resma Helle
tags: 
- Xamarin
modified_time: '2016-09-02T14:42:24.871+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6303647706094978203
blogger_orig_url: https://christian-helle.blogspot.com/2016/09/working-with-native-bitmap-pixel.html
redirect_from:
- /blog/2016/08/31/working-with-native-bitmap-pixel-buffers-in-xamarin-forms/
- /2016/08/31/working-with-native-bitmap-pixel-buffers-in-xamarin-forms/
- /2016/08/working-with-native-bitmap-pixel-buffers-in-xamarin-forms/
- /2016/working-with-native-bitmap-pixel-buffers-in-xamarin-forms/
- /working-with-native-bitmap-pixel-buffers-in-xamarin-forms/
---

I mentioned in my previous post that extracting pixel buffers from native Bitmap API’s can be quite tricky. In this post I would like to share the approach that I took for extracting native Bitmap pixel buffers into an collection of Xamarin.Forms.Colors objects so it can be used from a portable class library. I wrote and used a more complex version of the code mentioned in this post on my last project where I was working with image detection and color analysis for an app using Xamarin.Forms, in this project all my color analysis was done in a Portable Class Library using an abstraction over the native bitmap data.

## BitmapData abstraction

In .NET you had access to an API called [System.Drawing.Bitmap](https://learn.microsoft.com/en-us/dotnet/api/system.drawing.bitmap?view=net-8.0&viewFallbackFrom=dotnet-plat-ext-7.0&%3FWT.mc_id=DT-MVP-5004822) which encapsulates a low-level Windows API called Bitmap from GDI. The managed Bitmap class exposed a method called [LockBits](https://learn.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.lockbits?view=net-8.0&viewFallbackFrom=dotnet-plat-ext-7.0&redirectedfrom=MSDN#System_Drawing_Bitmap_LockBits_System_Drawing_Rectangle_System_Drawing_Imaging_ImageLockMode_System_Drawing_Imaging_PixelFormat_&WT.mc_id=DT-MVP-5004822) which in return gave you a [BitmapData](https://learn.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.lockbits?view=net-8.0&viewFallbackFrom=dotnet-plat-ext-7.0&redirectedfrom=MSDN#System_Drawing_Bitmap_LockBits_System_Drawing_Rectangle_System_Drawing_Imaging_ImageLockMode_System_Drawing_Imaging_PixelFormat_&WT.mc_id=DT-MVP-5004822) instance. BitmapData exposed information that allows you to manipulate the pixel buffer at a pointer level and is the fastest and recommended way to analyze and manipulate pixel information. I loved the BitmapData class but my portable class library implementation will not contain anything but a pixel buffer in ARGB and a Get/SetPixel(x,y, Color) method and a method for getting the average color of a certain area in the Bitmap to demonstrate what this can be used for

Here's the code

```csharp
public class BitmapData
{
    public BitmapData(Size size, int[] pixelBuffer)
    {
        Size = size;
        PixelBuffer = pixelBuffer;
    }

    public int[] PixelBuffer { get; }

    public Size Size { get; }

    public Color GetPixel(Point point) => GetPixel(point.X, point.Y);

    public Color GetPixel(double x, double y) => Color.FromUint((uint)PixelBuffer[(int)x * (int)y]);

    public void SetPixel(Point point, Color color) => SetPixel((int)point.X, (int)point.Y, color);

    public void SetPixel(double x, double y, Color color) => PixelBuffer[(int)(x * y)] = (int)(color.A * byte.MaxValue) << 24 |
                                                                                        ((int)color.R * byte.MaxValue) << 16 |
                                                                                        ((int)color.G * byte.MaxValue) << 8 |
                                                                                        ((int)color.B * byte.MaxValue) << 0;

    public Color GetAverageColor(params Rectangle[] rectangles)
    {
        var colors = new List<Color>();
        foreach (var rectangle in rectangles)
            for (var y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
                for (var x = (int)rectangle.X; x < (int)rectangle.X + (int)rectangle.Width; x++)
                    colors.Add(GetPixel(x, y));

        var red = (int)(colors.Average(c => c.R) * byte.MaxValue);
        var blue = (int)(colors.Average(c => c.G) * byte.MaxValue);
        var green = (int)(colors.Average(c => c.B) * byte.MaxValue);
        var alpha = (int)(colors.Average(c => c.A) * byte.MaxValue);
            
        return Color.FromRgba(red, blue, green, alpha);
    }
}
```

## UIImage to BitmapData (iOS)

To get the pixel buffer from a [UIImage](https://learn.microsoft.com/en-us/dotnet/api/uikit.uiimage?view=xamarin-ios-sdk-12&WT.mc_id=DT-MVP-5004822) instance we need to draw it to a new drawing surface by calling [DrawImage](https://learn.microsoft.com/en-us/dotnet/api/coregraphics.cgcontext.drawimage?view=xamarin-ios-sdk-12&WT.mc_id=DT-MVP-5004822) on an [CGBitmapContext](https://learn.microsoft.com/en-us/dotnet/api/coregraphics.cgbitmapcontext?view=xamarin-ios-sdk-12&WT.mc_id=DT-MVP-5004822) instance. When we construct the drawing surface we specify the pixel format and provide a pointer or an array of bytes in which the data will be written to. We need to specify that the pixels will contain a byte for each component, 4 bytes per pixel, and that the byte order is 32-bit Big Endian. We can also specify whether we specify the alpha component is in the most or least significant bits of each pixel, but for this example I will put it in the end since when I was researching about this, most of the examples I found used the least significant bit to store the alpha component.

Here's the code

```csharp
public BitmapData Convert(object nativeBitmap)
{
    var image = (UIImage)nativeBitmap;
    return new BitmapData(new Xamarin.Forms.Size(image.Size.Width, image.Size.Height), GetPixels(image));
}

private static int[] GetPixels(UIImage image)
{
    const int bytesPerPixel = 4;
    const int bitsPerComponent = 8;
    const CGBitmapFlags flags = CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast;

    var width = (int)image.CGImage.Width;
    var height = (int)image.CGImage.Height;
    var bytesPerRow = bytesPerPixel * width;
    var buffer = new byte[bytesPerRow * height];
    var pixels = new int[width * height];

    var handle = GCHandle.Alloc(buffer);
    try
    {
        using (var colorSpace = CGColorSpace.CreateGenericRgb())
        using (var context = new CGBitmapContext(buffer, width, height, bitsPerComponent, bytesPerRow, colorSpace, flags))
            context.DrawImage(new RectangleF(0, 0, width, height), image.CGImage);

        for (var y = 0; y < height; y++)
        {
            var offset = y * width;
            for (var x = 0; x < width; x++)
            {
                var idx = bytesPerPixel * (offset + x);
                var r = buffer[idx + 0];
                var g = buffer[idx + 1];
                var b = buffer[idx + 2];
                var a = buffer[idx + 3];
                pixels[x * y] = a << 24 | r << 16 | g << 8 | b << 0;
            }
        }
    }
    finally
    {
        handle.Free();
    }

    return pixels;
}
```

## Bitmap to BitmapData (Android)

This is pretty easy to do in Android as the [Bitmap](https://learn.microsoft.com/en-us/dotnet/api/android.graphics.bitmap?view=xamarin-android-sdk-13&WT.mc_id=DT-MVP-5004822) class exposes the [GetPixels](https://learn.microsoft.com/en-us/dotnet/api/android.graphics.bitmap.getpixels?view=xamarin-android-sdk-13&WT.mc_id=DT-MVP-5004822) method to get the pixel buffer and the pixel information is conveniently stored in ***ARGB***

Here's the code

```csharp
public BitmapData Convert(object nativeBitmap)
{
    var bitmap = (Bitmap)nativeBitmap;
    var info = bitmap.GetBitmapInfo();
    var pixels = new int[info.Width * info.Height];
    bitmap.GetPixels(pixels, 0, (int)info.Width, 0, 0, (int)info.Width, (int)info.Height);
    return new BitmapData(new Xamarin.Forms.Size(info.Width, info.Height), pixels);
}
```

## WriteableBitmap to BitmapData (Universal Windows Platform)

To do this using the Universal Windows Platform is a bit similar to iOS but is less complex. The [WriteableBitmap](https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.imaging.writeablebitmap?view=winrt-22621&WT.mc_id=DT-MVP-5004822) class exposes a [PixelBuffer](https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.imaging.writeablebitmap.pixelbuffer?view=winrt-22621&WT.mc_id=DT-MVP-5004822) as an [IBuffer](https://learn.microsoft.com/en-us/uwp/api/windows.storage.streams.ibuffer?view=winrt-22621&WT.mc_id=DT-MVP-5004822) in which you can call the extension method `ToArray()` on to get an array of integers. The interesting part about the WriteableBitmap PixelBuffer is that it doesn't really say anywhere in the documentation (at least not directly) that the component order is BGRA, I only figured this out by reading the sample code provided in the WriteableBitmap documentation where it says in a code comment that ***WriteableBitmap uses BGRA format***.

Here's the code

```csharp
public BitmapData Convert(object nativeBitmap)
{
    var imageSource = (WriteableBitmap)nativeBitmap;
    var pixelData = GetPixelDataFromImage(imageSource).ToArray();
    return new BitmapData(new Size(imageSource.PixelWidth, imageSource.PixelHeight), pixelData);
}

private static IEnumerable<int> GetPixelDataFromImage(WriteableBitmap imageSource)
{
    const int bytesPerPixel = 4;
    var pixelHeight = imageSource.PixelHeight;
    var pixelWidth = imageSource.PixelWidth;
    var buffer = imageSource.PixelBuffer.ToArray();
    var pixels = new int[buffer.Length];

    for (var y = 0; y < pixelHeight; y++)
    {
        var offset = y * pixelWidth;
        for (var x = 0; x < pixelWidth; x++)
        {
            var idx = bytesPerPixel * (offset + x);
            var b = buffer[idx + 0];
            var g = buffer[idx + 1];
            var r = buffer[idx + 2];
            var a = buffer[idx + 3];
            pixels[x * y] = a << 24 | r << 16 | g << 8 | b << 0;
        }
    }

    return pixels;
}
```

I remember struggling quite a bit when I was figuring out what I just shared and I hope that some one out there might be able to make some good use of it.