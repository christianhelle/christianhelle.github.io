---
layout: post
title: ButtonEx - Owner Drawn Button Control
date: '2007-09-30T15:08:00.002+02:00'
author: Christian Resma Helle
tags:
- Controls
- ".NET Compact Framework"
modified_time: '2008-10-30T15:41:33.247+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-2320776853180879041
blogger_orig_url: https://christian-helle.blogspot.com/2007/09/buttonex-owner-drawn-button-control.html
---

Wow 2 posts in 1 day. I must have a lot of spare time today! Actually I'm down with a cold and I just feel sorry for myself when I stay in bed.

In this article I'd like to share another piece of code that I use quite often. Its a simple owner drawn button control that can have a nice 3D shadow. Drawing the 3D shadow effect was by complete accident though. I was trying to just draw a simple control that acts like a button. When I was drawing the borders the first time I drawn the rectangle a pixel bigger hence the control had a 1 pixel line of just black on the right and bottom side. Anyway, my graphic artist loved it and decided that we keep it.

The control has the following properties added:
  1) PushedColor (Color) - The color the control will be filled with once the MouseDown is fired
  2) DrawShadow (Boolean) - A flag whether to draw the 3D border

The default settings of the control is best on a form with a blue-ish background.

And here's the code:

```csharp
public class ButtonEx : Control
{
    private Bitmap off_screen;
    private SolidBrush back_brush;
    private SolidBrush selected_brush;
    private SolidBrush text_brush;
    private Pen border_pen;
    private bool pushed;
    private bool shadow;

    public ButtonEx()
    {
        BackColor = Color.FromArgb(2, 32, 154);
        ForeColor = Color.White;

        back_brush = new SolidBrush(Color.FromArgb(48, 88, 198));
        selected_brush = new SolidBrush(Color.FromArgb(15, 51, 190));
        border_pen = new Pen(Color.White);
        text_brush = new SolidBrush(Color.White);
    }

    public override Color BackColor
    {
        get { return base.BackColor; }
        set
        {
            base.BackColor = value;
            back_brush = new SolidBrush(value);
        }
    }

    public override Color ForeColor
    {
        get { return base.ForeColor; }
        set
        {
            base.ForeColor = value;
            border_pen = new Pen(value);
            text_brush = new SolidBrush(value);
        }
    }

    public Color PushedColor
    {
        get { return selected_brush.Color; }
        set { selected_brush = new SolidBrush(value); }
    }

    public bool DrawShadow
    {
        get { return shadow; }
        set { shadow = value; }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (off_screen == null)
        {
            off_screen = new Bitmap(ClientSize.Width, ClientSize.Height);
        }

        using (Graphics gxOff = Graphics.FromImage(off_screen))
        {
            gxOff.DrawRectangle(
                border_pen,
                0,
                0,
                ClientSize.Width - (shadow ? 0 : 1),
                ClientSize.Height - (shadow ? 0 : 1)
            );

            Rectangle rect = new Rectangle(
                1,
                1,
                ClientRectangle.Width - 2,
                ClientRectangle.Height - 2
            );

            gxOff.FillRectangle(pushed ? back_brush : selected_brush, rect);

            if (!string.IsNullOrEmpty(Text))
            {
                SizeF size = gxOff.MeasureString(Text, Font);
                gxOff.DrawString(
                    Text,
                    Font,
                    text_brush,
                    (ClientSize.Width - size.Width) / 2,
                    (ClientSize.Height - size.Height) / 2
                );
            }
        }

        e.Graphics.DrawImage(off_screen, 0, 0);
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

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

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);

        Invalidate();
    }
}
```