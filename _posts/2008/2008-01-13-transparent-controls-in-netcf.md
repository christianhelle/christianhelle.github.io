---
layout: post
title: Transparent Controls in .NETCF
date: '2008-01-13T17:21:00.002+01:00'
author: Christian Resma Helle
tags:
- Controls
- Transparent Controls
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2009-10-12T16:00:03.529+02:00'
thumbnail: http://4.bp.blogspot.com/_kVNAYTvQ3QE/R4dD2V0d_VI/AAAAAAAAAu0/nxNdlVu_suI/s72-c/pc_capture4.jpg
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-7179409956875335575
blogger_orig_url: https://christian-helle.blogspot.com/2008/01/transparent-controls-in-netcf.html
---

I work a lot with a graphic artist for developing solutions. The better the graphic artist you work with is, the harder it is to implement their designs to your application. One thing that my solutions have in common is that they all require transparent controls. My graphic artist loves having a image buttons on top of fancy background.

Here's some screen shots of what I've made with my graphic artist:

![](/assets/images/transparent-controls-inspectionlog-1.jpg)
![](/assets/images/transparent-controls-inspectionlog-2.jpg)
![](/assets/images/transparent-controls-inspectionlog-3.jpg)

![](/assets/images/transparent-controls-timetracker-1.jpg)
![](/assets/images/transparent-controls-timetracker-2.jpg)

In these screen shots I heavily use a transparent label control over a Form that has a background image. I normally set my controls to be designer visible so I can drag and drop while designing. Visual Studio 2005 and 2008 will automatically load all Custom Controls and UserControls

### Implementing Transparent Controls

For creating transparent controls, I need the following:

1) `IControlBackground` interface - contains `BackgroundImage { get; }`
2) `TransparentControlBase` - draws the `BackgroundImage` of an `IControlBackground` form
3) Transparent Control - Inherits from `TransparentControlBase`
4) `FormBase` Form - implements `IControlBackground` and draws the background image to the form

Let's start off with the `IControlBackground` interface. Like I mentioned above, it only contains a property called `BackgroundImage`.

```csharp
public interface IControlBackground
{
  Image BackgroundImage { get; }
}
```

Next we will need to create the `TransparentControlBase`. Let's create a class that inherits from Control. We then need to override the `OnPaintBackground()` event to draw the `IControlBackground.BackgroundImage` of the Parent control. To do this, we create an instance of IControlBackground from the Parent. Once we have the BackgroundImage, we draw part of the BackgroundImage where the transparent control is lying on.

We also override the `OnTextChanged()` and `OnParentChanged()` events to force a re-draw whenever the text or parent of the control is changed.

```csharp
public class TransparentControlBase : Control
{
  protected bool HasBackground = false;

  protected override void OnPaintBackground(PaintEventArgs e)
  {
    IControlBackground form = Parent as IControlBackground;
    if (form == null) {
      base.OnPaintBackground(e);
      return;
    } else {
      HasBackground = true;
    }

    e.Graphics.DrawImage(
      form.BackgroundImage,
      0,
      0,
      Bounds,
      GraphicsUnit.Pixel);
  }

  protected override void OnTextChanged(EventArgs e)
  {
    base.OnTextChanged(e);
    Invalidate();
  }

  protected override void OnParentChanged(EventArgs e)
  {
    base.OnParentChanged(e);
    Invalidate();
  }
}
```

Now we need to create a control that inherits from `TransparentControlBase`. I'll create a simple `TransparentLabel` control for this example. The control will have the same behavior as the standard Label control, except that it can be transparent when used over a form or control that implements `IControlBackground`.

```csharp
public class TransparentLabel : TransparentControlBase
{
  ContentAlignment alignment = ContentAlignment.TopLeft;
  StringFormat format = null;
  Bitmap off_screen = null;

  public TransparentLabel()
  {
    format = new StringFormat();
  }

  public ContentAlignment TextAlign
  {
    get { return alignment; }
    set
    {
      alignment = value;
      switch (alignment) {
        case ContentAlignment.TopCenter:
          format.Alignment = StringAlignment.Center;
          format.LineAlignment = StringAlignment.Center;
          break;
        case ContentAlignment.TopLeft:
          format.Alignment = StringAlignment.Near;
          format.LineAlignment = StringAlignment.Near;
          break;
        case ContentAlignment.TopRight:
          format.Alignment = StringAlignment.Far;
          format.LineAlignment = StringAlignment.Far;
          break;
      }
    }
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    if (!base.HasBackground) {
      if (off_screen == null) {
        off_screen = new Bitmap(ClientSize.Width, ClientSize.Height);
      }
      using (Graphics g = Graphics.FromImage(off_screen)) {
        using (SolidBrush brush = new SolidBrush(Parent.BackColor)) {
          g.Clear(BackColor);
          g.FillRectangle(brush, ClientRectangle);
        }
      }
    } else {
      using (SolidBrush brush = new SolidBrush(ForeColor)) {
        e.Graphics.DrawString(
          Text,
          Font,
          brush,
          new Rectangle(0, 0, Width, Height),
          format);
      }
    }
  }
}
```

Now that we have our transparent controls, we need to create a Form that will contain these controls. First we need to create a base class that will implement `IControlBackground` and inherit from Form.

In this example, I added a background image to the solution and as an embedded resource. My default namespace is called TransparentSample and my background image is located at the root folder with the filename background.jpg

```csharp
public class FormBase : Form, IControlBackground
{
  Bitmap background;

  public FormBase()
  {
    background = new Bitmap(
      Assembly.GetExecutingAssembly().GetManifestResourceStream(
      "TransparentSample.background.jpg"));
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

For the last step, we need to create a Form that will contain these transparent controls. To start, let's add a new Form to our project and let it inherit from FormBase instead of Form.

Now we can add our transparent controls to the main form.

```csharp
public class MainForm : FormBase
{
  TransparentLabel label;

  public MainForm()
  {
    label = new TransparentLabel();
    label.Font = new Font("Arial", 16f, FontStyle.Bold);
    label.ForeColor = Color.White;
    label.Text = "Transparent Label";
    label.Bounds = new Rectangle(20, 60, 200, 50);
    Controls.Add(label);
  }
}
```

That wasn't very complicated, was it? Having a nice and intuitive UI offers a very good user experience. Being creative, imaginative, and learning to work with a graphic artist can really pay off.