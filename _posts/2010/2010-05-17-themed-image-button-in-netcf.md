---
layout: post
title: Themed Image Button in .NETCF
date: '2010-05-17T14:49:00.034+02:00'
author: Christian Resma Helle
tags:
- Controls
- Graphics
- How to
- Transparent Controls
- ".NET Compact Framework"
modified_time: '2010-05-17T20:57:02.158+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-5663003056614670748
blogger_orig_url: https://christian-helle.blogspot.com/2010/05/themed-image-button-in-netcf.html
redirect_from:
- /2010/05/17/themed-image-button-in-netcf/
- /2010/05/17/themed-image-button-in-netcf
- /2010/05/themed-image-button-in-netcf/
- /2010/05/themed-image-button-in-netcf
- /2010/themed-image-button-in-netcf/
- /2010/themed-image-button-in-netcf
- /themed-image-button-in-netcf/
- /themed-image-button-in-netcf
---

In this article I'd like to demonstrate how to create a themed image button control. I'll be using the same techniques in my previous article, [How to draw a textured rounded rectangle](/2010/01/how-to-draw-textured-rounded-rectangle.html) and [Semi-Transparent Controls in .NETCF](/2010/01/semi-transparent-controls-in-netcf.html).  
  
First, we need to define our theme. We get the start and end gradient colors from alpha blending the `SystemColors.Highlight` color and `Color.White`

```csharp
class Theme
{
    public static Color AlphaBlend(Color value1, Color value2, int alpha)
    {
        int ialpha = 256 - alpha;
        return Color.FromArgb((value1.R * alpha) + (value2.R * ialpha) >> 8,
                              (value1.G * alpha) + (value2.G * ialpha) >> 8,
                              (value1.B * alpha) + (value2.B * ialpha) >> 8);
    }
 
    public static Color GradientLight
    {
        get
        {
            var color = AlphaBlend(SystemColors.Highlight, Color.White, 100);
            return AlphaBlend(Color.White, color, 50); ;
        }
    }
 
    public static Color GradientDark
    {
        get
        {
            var color = AlphaBlend(SystemColors.Highlight, Color.Black, 256);
            return AlphaBlend(Color.White, color, 50); ;
        }
    }
}
```

We'll be using the [GradientFill](http://msdn.microsoft.com/en-us/library/aa453192.aspx?WT.mc_id=DT-MVP-5004822) method as well in this example. For this we need to define 2 structures, [TRIVERTEX](http://msdn.microsoft.com/en-us/library/aa453818.aspx?WT.mc_id=DT-MVP-5004822) and [GRADIENT_RECT](http://msdn.microsoft.com/en-us/library/aa453193.aspx?WT.mc_id=DT-MVP-5004822)  

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
    int dwNumVertex, 
    GRADIENT_RECT[] pMesh, 
    int dwNumMesh, 
    int dwMode);
```

Let's wrap the P/Invoke call to GradientFill in a nice method

```csharp
const int GRADIENT_FILL_RECT_V = 0x00000001;
 
public static void GradientFill(
    this Graphics graphics,
    Rectangle rect,
    Color startColor,
    Color endColor)
{
    var tva = new TRIVERTEX[2];
    tva[0] = new TRIVERTEX(rect.X, rect.Y, startColor);
    tva[1] = new TRIVERTEX(rect.Right, rect.Bottom, endColor);
    var gra = new GRADIENT_RECT[] { new GRADIENT_RECT(0, 1) };
 
    var hdc = graphics.GetHdc();
    GradientFill(hdc, tva, tva.Length, gra, gra.Length, GRADIENT_FILL_RECT_V);
    graphics.ReleaseHdc(hdc);
}
```

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
static extern IntPtr CreatePatternBrush(IntPtr HBITMAP);
 
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

We'll draw a textured rounded rectangle with a gradient fill that uses the current theme's Highlight color as the base color.

```csharp
public static void DrawThemedGradientRectangle(
    this Graphics graphics,
    Pen border,
    Rectangle area,
    Size ellipseSize)
{
    using (var texture = new Bitmap(area.Right, area.Bottom))
    {
        using (var g = Graphics.FromImage(texture))
            GradientFill(g, area, Theme.GradientLight, Theme.GradientDark);
 
        FillRoundedTexturedRectangle(graphics, border, texture, area, ellipseSize);
    }
}
 
static IntPtr CreateGdiPen(Pen pen)
{
    var style = pen.DashStyle == DashStyle.Solid ? PS_SOLID : PS_DASH;
    return CreatePen(style, (int)pen.Width, GetColorRef(pen.Color));
}
 
public static void FillRoundedTexturedRectangle(
    this Graphics graphics,
    Pen border,
    Bitmap texture,
    Rectangle rect,
    Size ellipseSize)
{
    Point old = new Point();
 
    var hdc = graphics.GetHdc();
    var hpen = CreateGdiPen(border);
    var hbitmap = texture.GetHbitmap();
    var hbrush = CreatePatternBrush(hbitmap);
 
    SetBrushOrgEx(hdc, rect.Left, rect.Top, ref old);
    SelectObject(hdc, hpen);
    SelectObject(hdc, hbrush);
 
    RoundRect(hdc, rect.Left, rect.Top, rect.Right, rect.Bottom, ellipseSize.Width, ellipseSize.Height);
 
    SetBrushOrgEx(hdc, old.Y, old.X, ref old);
    DeleteObject(hpen);
    DeleteObject(hbrush);
 
    graphics.ReleaseHdc(hdc);
}
```

Let's wrap all the code above as extension methods to the Graphics class and use them in our owner drawn button control. A button control is one of the easiest owner drawn controls to create. Let's keep it as simple as possible and only have 2 states for our button: pressed and not pressed.

```csharp
class ThemedImageButton : Control
{
    bool pushed = false;
    private Bitmap image;
    private Bitmap offScreen;
 
    public Bitmap Image
    {
        get { return image; }
        set
        {
            image = value;
            Invalidate();
        }
    }
 
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
 
        if (offScreen != null)
        {
            offScreen.Dispose();
            offScreen = null;
        }
        offScreen = new Bitmap(ClientSize.Width, ClientSize.Height);
    }
 
    protected override void OnPaint(PaintEventArgs e)
    {
        if (offScreen == null)
            offScreen = new Bitmap(ClientSize.Width, ClientSize.Height);
 
        using (var attributes = new ImageAttributes())
        using (var g = Graphics.FromImage(offScreen))
        {
            if (pushed)
            {
                using (var pen = new Pen(SystemColors.Highlight))
                    g.DrawThemedGradientRectangle(pen, ClientRectangle, new Size(4, 4));
            }
            else
                g.Clear(Parent.BackColor);
 
            var textSize = g.MeasureString(Text, Font);
            var textArea = new RectangleF(
                (ClientSize.Width - textSize.Width) / 2,
                (ClientSize.Height - textSize.Height),
                textSize.Width,
                textSize.Height);
 
            if (Image != null)
            {
                var imageArea = new Rectangle(
                    (ClientSize.Width - Image.Width) / 2,
                    (ClientSize.Height - Image.Height) / 2,
                    Image.Width,
                    Image.Height);
 
                var key = Image.GetPixel(0, 0);
                attributes.SetColorKey(key, key);
 
                g.DrawImage(
                    Image,
                    imageArea,
                    0, 0, Image.Width, Image.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }
 
            using (var brush = new SolidBrush(ForeColor))
                g.DrawString(Text, Font, brush, textArea);
 
            if (pushed)
            {
                var key = offScreen.GetPixel(0, 0);
                attributes.SetColorKey(key, key);
            }
            else
                attributes.ClearColorKey();
 
            e.Graphics.DrawImage(
                offScreen,
                ClientRectangle,
                0, 0, offScreen.Width, offScreen.Height,
                GraphicsUnit.Pixel,
                attributes);
        }
    }
 
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
 
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
 
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        Invalidate();
    }
 
    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
}
```

I hope you found this useful. If you're interested in the full source code then you can grab it [here](/assets/samples/ThemedRoundedRectangle.zip)