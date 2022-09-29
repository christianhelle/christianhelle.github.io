---
layout: post
title: Semi-Transparent Controls in .NETCF
date: '2010-01-19T11:17:00.001+01:00'
author: Christian Resma Helle
tags:
- Controls
- Transparent Controls
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2010-01-24T21:33:48.078+01:00'
thumbnail: http://3.bp.blogspot.com/_kVNAYTvQ3QE/SVH_9-UkIrI/AAAAAAAABjE/5_UGXBzV1ig/s72-c/danfoss+rfid+mobile+1.JPG
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-2869394435067622677
blogger_orig_url: https://christian-helle.blogspot.com/2010/01/semi-transparent-controls-in-netcf.html
---

Some time ago, I wrote an article called [Transparent Controls in .NETCF](/2008/01/transparent-controls-in-netcf.html) which describes how to accomplish transparency in the .NET Compact Framework. In this article I'd like to discuss how to control the opacity levels of transparent controls. This is made possible by alpha blending the parent controls background image with the child controls background color or image.  
  
Here is an example of semi-transparent controls.  
  
![](/assets/images/danfoss-rfid-mobile.jpg)
  
The screen shots above contain a semi-transparent PictureBox, Label, and Button controls. The button control is very Vista inspired, having 2 vertical fading gradient fills and then alpha blending it with the background image  
  
**Implementing Semi-Transparent Controls**  
  
The container or parent control will be the same code I used in my [previous article](/2008/01/transparent-controls-in-netcf.html).  
  
Windows Mobile 5 and higher offers 2 ways of alpha blending, one through the function [AlphaBlend (GDI)](http://msdn.microsoft.com/en-us/library/aa452850.aspx) and through the [Imaging API (COM)](http://msdn.microsoft.com/en-us/library/ms925314.aspx). In this example let's use the AlphaBlend function.  
  
To begin with we need to define the [BLENDFUNCTION](http://msdn.microsoft.com/en-us/library/aa452889.aspx) structure that we'll use as a parameter to AlphaBlend.  
  
```csharp
struct BLENDFUNCTION
{
    public byte BlendOp;
    public byte BlendFlags;
    public byte SourceConstantAlpha;
    public byte AlphaFormat;
}
 
enum BlendOperation : byte
{
    AC_SRC_OVER = 0x00
}
 
enum BlendFlags : byte
{
    Zero = 0x00
}
 
enum SourceConstantAlpha : byte
{
    Transparent = 0x00,
    Opaque = 0xFF
}
 
enum AlphaFormat : byte
{
    AC_SRC_ALPHA = 0x01
}
```  

Next step is to define our P/Invoke declaration for AlphaBlend  
  
```csharp
[DllImport("coredll.dll")]
static extern bool AlphaBlend(
    IntPtr hdcDest,
    int xDest,
    int yDest,
    int cxDest,
    int cyDest,
    IntPtr hdcSrc,
    int xSrc,
    int ySrc,
    int cxSrc,
    int cySrc,
    BLENDFUNCTION blendfunction);
```

We'll be using the [GradientFill](http://msdn.microsoft.com/en-us/library/aa453192.aspx) method as well in this example. For this we need to define 2 structures, [TRIVERTEX](http://msdn.microsoft.com/en-us/library/aa453818.aspx) and [GRADIENT\_RECT](http://msdn.microsoft.com/en-us/library/aa453193.aspx)  
  
```csharp
struct TRIVERTEX
{
    private int x;
    private int y;
    private ushort Red;
    private ushort Green;
    private ushort Blue;
    private ushort Alpha;
 
    public TRIVERTEX(int x, int y, Color color)
        : this(x, y, color.R, color.G, color.B, color.A)
    {
    }
 
    public TRIVERTEX(
        int x, int y,
        ushort red, ushort green, ushort blue,
        ushort alpha)
    {
        this.x = x;
        this.y = y;
        Red = (ushort)(red << 8);
        Green = (ushort)(green << 8);
        Blue = (ushort)(blue << 8);
        Alpha = (ushort)(alpha << 8);
    }
}
 
struct GRADIENT_RECT
{
    private uint UpperLeft;
    private uint LowerRight;
 
    public GRADIENT_RECT(uint ul, uint lr)
    {
        UpperLeft = ul;
        LowerRight = lr;
    }
}
```  

Using the 2 structures above we can now define our P/Invoke for GradientFill  
  
```csharp
[DllImport("coredll.dll")]
static extern bool GradientFill(
    IntPtr hdc,
    TRIVERTEX[] pVertex,
    uint dwNumVertex,
    GRADIENT_RECT[] pMesh,
    uint dwNumMesh,
    uint dwMode);
```
  
And then lets wrap those neatly in some extension methods to the Graphics class  
  
```csharp
public static class GraphicsExtension
{
    public static void AlphaBlend(this Graphics graphics, Image image, byte opacity)
    {
        AlphaBlend(graphics, image, opacity, Point.Empty);
    }
 
    public static void AlphaBlend(this Graphics graphics, Image image, byte opacity, Point location)
    {
        using (var imageSurface = Graphics.FromImage(image))
        {
            var hdcDst = graphics.GetHdc();
            var hdcSrc = imageSurface.GetHdc();
 
            try
            {
                var blendFunction = new BLENDFUNCTION
                {
                    BlendOp = ((byte)BlendOperation.AC_SRC_OVER),
                    BlendFlags = ((byte)BlendFlags.Zero),
                    SourceConstantAlpha = opacity,
                    AlphaFormat = 0
                };
                AlphaBlend(
                    hdcDst,
                    location.X == 0 ? 0 : -location.X,
                    location.Y == 0 ? 0 : -location.Y,
                    image.Width,
                    image.Height,
                    hdcSrc,
                    0,
                    0,
                    image.Width,
                    image.Height,
                    blendFunction);
            }
            finally
            {
                graphics.ReleaseHdc(hdcDst);
                imageSurface.ReleaseHdc(hdcSrc);
            }
        }
    }
 
    public static void GradientFill(
        this Graphics graphics,
        Rectangle rectangle,
        Color startColor,
        Color endColor,
        GradientFillDirection direction)
    {
        var tva = new TRIVERTEX[2];
        tva[0] = new TRIVERTEX(rectangle.Right, rectangle.Bottom, endColor);
        tva[1] = new TRIVERTEX(rectangle.X, rectangle.Y, startColor);
        var gra = new[] { new GRADIENT_RECT(0, 1) };
 
        var hdc = graphics.GetHdc();
        try
        {
            GradientFill(
                hdc,
                tva,
                (uint)tva.Length,
                gra,
                (uint)gra.Length,
                (uint)direction);
        }
        finally
        {
            graphics.ReleaseHdc(hdc);
        }
    }
 
    public enum GradientFillDirection
    {
        Horizontal = 0x00000000,
        Vertical = 0x00000001
    }
}
```  

Now that we can call the AlphaBlend API we can start designing a simple UI. Let's create a container to for the semi-transparent control that exposes its background image allowing the child control to alpha blend part of the background image. Before we create our alpha button control, let's take a quick review on [How to implement transparency in .NETCF](/2008/01/transparent-controls-in-netcf.html).  
  
In my old article I expose the background image through an interface that a container control must implement, I called it IControlBackground  
  
```csharp
interface IControlBackground
{
    Image BackgroundImage { get; }
}
```
  
Now that it's possible to retrieve the parent controls background image through a child control we can create our alpha button control. This control will be the usual button control where we will use a boolean flag to store the pushed state of the control, this state is changed during mouse events. The control is painted using 2 gradient fills using shades of black and the text is drawn in the center. Since the entire control is painted in the OnPaint event I override OnPaintBackground with blank code.  
  
```csharp
class AlphaButton : Control
{
    private bool pushed;
 
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        pushed = true;
        Invalidate();
    }
 
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        pushed = false;
        Invalidate();
    }
 
    protected override void OnPaint(PaintEventArgs e)
    {
        using (var backdrop = new Bitmap(Width, Height))
        {
            using (var gxOff = Graphics.FromImage(backdrop))
            {
                var topRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height / 2);
                var bottomRect = new Rectangle(0, topRect.Height, ClientSize.Width, ClientSize.Height / 2);
 
                if (!pushed)
                {
                    gxOff.GradientFill(
                        bottomRect,
                        Color.FromArgb(0, 0, 11),
                        Color.FromArgb(32, 32, 32),
                        GraphicsExtension.GradientFillDirection.Vertical);
 
                    gxOff.GradientFill(
                        topRect,
                        Color.FromArgb(176, 176, 176),
                        Color.FromArgb(32, 32, 32),
                        GraphicsExtension.GradientFillDirection.Vertical);
                }
                else
                {
                    gxOff.GradientFill(
                        topRect,
                        Color.FromArgb(0, 0, 11),
                        Color.FromArgb(32, 32, 32),
                        GraphicsExtension.GradientFillDirection.Vertical);
 
                    gxOff.GradientFill(
                        bottomRect,
                        Color.FromArgb(176, 176, 176),
                        Color.FromArgb(32, 32, 32),
                        GraphicsExtension.GradientFillDirection.Vertical);
                }
 
                using (var border = new Pen(Color.White))
                    gxOff.DrawRectangle(border, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
 
                if (!string.IsNullOrEmpty(Text))
                {
                    var size = gxOff.MeasureString(Text, Font);
                    using (var text = new SolidBrush(Color.White))
                        gxOff.DrawString(
                            Text,
                            Font,
                            text,
                            (ClientSize.Width - size.Width) / 2,
                            (ClientSize.Height - size.Height) / 2);
                }
 
                try
                {
                    var bgOwner = Parent as IControlBackground;
                    if (bgOwner != null && bgOwner.BackgroundImage != null)
                        gxOff.AlphaBlend(bgOwner.BackgroundImage, 70, Location);
                }
                catch (MissingMethodException ex)
                {
                    throw new PlatformNotSupportedException(
                        "AlphaBlend is not a supported GDI feature on this device", 
                        ex);
                }
            }
 
            e.Graphics.DrawImage(backdrop, 0, 0);
        }
    }
 
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
}
```  

I implement the IControlBackground interface in a simple Form class and draw my background image on the Form. I can use this directly as a container or create inherited classes and re-use the code in several occasions.  
  
```csharp
class FormBase : Form, IControlBackground
{
    Bitmap background;
 
    public FormBase()
    {
        background = new Bitmap(
            Assembly.GetExecutingAssembly().GetManifestResourceStream(
            "SemiTransparentSample.background.jpg"));
    }
 
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.DrawImage(background, 0, 0);
    }
 
    public Image BackgroundImage
    {
        get { return background; }
    }
}
```
  
To finish it up let's create a Form that will contain the alpha buttons.  
  
```csharp
class MainForm : FormBase
{
    public MainForm()
    {
        Controls.Add(new AlphaButton
        {
            Font = new Font("Arial", 16f, FontStyle.Bold),
            ForeColor = Color.White,
            Text = "Alpha Button 1",
            Bounds = new Rectangle(20, 20, 200, 50)
        });
        Controls.Add(new AlphaButton
        {
            Font = new Font("Arial", 16f, FontStyle.Bold),
            ForeColor = Color.White,
            Text = "Alpha Button 2",
            Bounds = new Rectangle(20, 90, 200, 50)
        });
        Controls.Add(new AlphaButton
        {
            Font = new Font("Arial", 16f, FontStyle.Bold),
            ForeColor = Color.White,
            Text = "Alpha Button 3",
            Bounds = new Rectangle(20, 160, 200, 50)
        });
        Controls.Add(new AlphaButton
        {
            Font = new Font("Arial", 16f, FontStyle.Bold),
            ForeColor = Color.White,
            Text = "Alpha Button 4",
            Bounds = new Rectangle(20, 230, 200, 50)
        });
    }
}
```
  
I hope you found this interesting and insightful. If you're interested in the Visual Studio solution then you can download it [here](/assets/samples/SemiTransparentSample.zip).