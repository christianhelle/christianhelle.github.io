---
layout: post
title: How to draw a textured rounded rectangle in .NETCF
date: '2010-01-27T07:41:00.039+01:00'
author: Christian Resma Helle
tags:
- Graphics
- How to
- ".NET Compact Framework"
modified_time: '2010-01-27T08:52:50.629+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-7170025817190813789
blogger_orig_url: https://christian-helle.blogspot.com/2010/01/how-to-draw-textured-rounded-rectangle.html
---

I'd like to demonstrate how to draw patterned rounded rectangles by P/Invoking the GDI function [RoundRect](https://learn.microsoft.com/en-us/previous-versions/aa929212(v=msdn.10)?WT.mc_id=DT-MVP-5004822) using a patterned brush instead of a solid brush. Let's create an extension method called FillRoundedTexturedRectangle to the Graphics class. With this method we can fill rectangles with an image. This image will be drawn as tiles to fill the rectangle bounds. This method can come in handy for drawing complex textures or patterns into a rectangle. With a little alpha blending and gradient fills, one can achieve a modern glass effect in the user interface  
  
We create our brush with the [CreatePatternBrush](https://learn.microsoft.com/en-us/library/ms908179.aspx?WT.mc_id=DT-MVP-5004822) function instead of [CreateSolidBrush](https://learn.microsoft.com/en-us/library/ms959979.aspx?WT.mc_id=DT-MVP-5004822) as we did in my previous article on [How to draw a rounded rectangle in .NETCF](/2010/01/how-to-draw-rounded-rectangle-in-netcf.html). We will mostly use P/Invoke for creating and releasing GDI objects.  
  
```csharp
const int PS_SOLID = 0;
const int PS_DASH = 1;
 
[DllImport("coredll.dll")]
static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);
 
[DllImport("coredll.dll")]
static extern int SetBrushOrgEx(IntPtr hdc, int nXOrg, int nYOrg, ref Point lppt);
 
[DllImport("coredll.dll")]
static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobject);
 
[DllImport("coredll.dll")]
static extern bool DeleteObject(IntPtr hObject);
 
[DllImport("coredll.dll")]
static extern IntPtr CreatePatternBrush(IntPtr hbmp);
 
[DllImport("coredll.dll")]
static extern bool RoundRect(
    IntPtr hdc, 
    int nLeftRect, 
    int nTopRect, 
    int nRightRect, 
    int nBottomRect, 
    int nWidth, 
    int nHeight);
 
static uint GetColorRef(Color value)
{
    return 0x00000000 | ((uint)value.B << 16) | ((uint)value.G << 8) | (uint)value.R;
}
```
  
Now we have our P/Invoke definitions in place we can neatly wrap all P/Invoke operations in a single function and let's call that FillRoundedTexturedRectangle()  
  
```csharp
public static void FillRoundedTexturedRectangle(
    this Graphics graphics,
    Pen border,
    Bitmap texture,
    Rectangle rectangle,
    Size ellipseSize)
{
    var lppt = new Point();
    var hdc = graphics.GetHdc();
    var style = border.DashStyle == DashStyle.Solid ? PS_SOLID : PS_DASH;
    var hpen = CreatePen(style, (int)border.Width, GetColorRef(border.Color));
    var hbrush = CreatePatternBrush(texture.GetHbitmap());
 
    try
    {
        SetBrushOrgEx(hdc, rectangle.Left, rectangle.Top, ref lppt);
        SelectObject(hdc, hpen);
        SelectObject(hdc, hbrush);
 
        RoundRect(hdc,
                  rectangle.Left,
                  rectangle.Top,
                  rectangle.Right,
                  rectangle.Bottom,
                  ellipseSize.Width,
                  ellipseSize.Height);
    }
    finally
    {
        SetBrushOrgEx(hdc, lppt.Y, lppt.X, ref lppt);
        DeleteObject(hpen);
        DeleteObject(hbrush);
 
        graphics.ReleaseHdc(hdc);
    }
}
```

To use this extension method you need to create a Bitmap and a Pen. The pen will be used to draw the rounded border, and the Bitmap will be used as a fill (tiled). Here's an example where "e" is an instance of PaintEventArgs  
  
```csharp
using (var pen = new Pen(SystemColors.Highlight, 5))
using (var texture = new Bitmap(@"\windows\msn.gif"))
    e.Graphics.FillRoundedTexturedRectangle(pen, 
                                            texture, 
                                            ClientRectangle, 
                                            new Size(8, 8));
```
  
I hope you found this useful. If you're interested in the full source code then you can grab it [here](/assets/samples/TexturedRoundedRectangle.cs)