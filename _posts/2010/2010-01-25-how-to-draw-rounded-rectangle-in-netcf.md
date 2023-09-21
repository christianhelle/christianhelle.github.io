---
layout: post
title: How to draw a rounded rectangle in .NETCF
date: '2010-01-25T22:29:00.035+01:00'
author: Christian Resma Helle
tags:
- Graphics
- How to
- ".NET Compact Framework"
modified_time: '2010-01-25T23:25:45.390+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-5297336445844260040
blogger_orig_url: https://christian-helle.blogspot.com/2010/01/how-to-draw-rounded-rectangle-in-netcf.html
---

In this short article I'd like to demonstrate how to draw rounded rectangles by P/Invoking the GDI function [RoundRect](http://learn.microsoft.com/en-us/library/aa929212.aspx?WT.mc_id=DT-MVP-5004822). Let's create an extension method called FillRoundedRectangle to the Graphics class.  
  
In order to use the function we need to create a few GDI objects: a Pen to draw the border, and a Brush to fill the rectangle. We will mostly use P/Invoke for creating and releasing GDI objects  
  
```csharp
const int PS_SOLID = 0;
const int PS_DASH = 1;
 
[DllImport("coredll.dll")]
static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);
 
[DllImport("coredll.dll")]
static extern int SetBrushOrgEx(IntPtr hdc, int nXOrg, int nYOrg, ref Point lppt);
 
[DllImport("coredll.dll")]
static extern IntPtr CreateSolidBrush(uint color);
 
[DllImport("coredll.dll")]
static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobject);
 
[DllImport("coredll.dll")]
static extern bool DeleteObject(IntPtr hgdiobject);
 
[DllImport("coredll.dll")]
static extern bool RoundRect(
    IntPtr hdc, 
    int nLeftRect, 
    int nTopRect, 
    int nRightRect, 
    int nBottomRect, 
    int nWidth, 
    int nHeight);
```

The CreateSolidBrush function in native code actually takes a [COLORREF](http://learn.microsoft.com/en-us/library/aa923096.aspx?WT.mc_id=DT-MVP-5004822) parameter, and the developer would normally use the [RGB](http://learn.microsoft.com/en-us/library/aa927387.aspx?WT.mc_id=DT-MVP-5004822) macro to create it. We need to translate that macro into a .NET function  
  
```csharp
static uint GetColorRef(Color value)
{
    return 0x00000000 | ((uint)value.B << 16) | ((uint)value.G << 8) | (uint)value.R;
}
```
  
Now we have our P/Invoke definitions in place we can neatly wrap all P/Invoke operations in a single function and let's call that FillRoundedRectangle()  
  
```csharp
public static void FillRoundedRectangle(
    this Graphics graphics,
    Pen border,
    Color color,
    Rectangle rectangle,
    Size ellipseSize)
{
    var lppt = new Point();
    var hdc = graphics.GetHdc();
    var style = border.DashStyle == DashStyle.Solid ? PS_SOLID : PS_DASH;
    var hpen = CreatePen(style, (int)border.Width, GetColorRef(border.Color));
    var hbrush = CreateSolidBrush(GetColorRef(color));
 
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
  
Using this extension method should be straight forward, but here's an example where "e" is an instance of PaintEventArgs:  

```csharp
using (var pen = new Pen(Color.Blue))
    e.Graphics.FillRoundedRectangle(
        pen, 
        Color.LightBlue, 
        new Rectangle(10, 10, 100, 100), 
        new Size(16, 16));
```  

I hope you found this useful. If you're interested in the full source code then you can grab it [here](/assets/samples/RoundedRectangle.cs)