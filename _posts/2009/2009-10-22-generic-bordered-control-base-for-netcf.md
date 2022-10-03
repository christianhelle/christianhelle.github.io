---
layout: post
title: Generic Bordered Control Base for .NETCF
date: '2009-10-22T09:15:00.025+02:00'
author: Christian Resma Helle
tags:
- Controls
- ".NET Compact Framework"
modified_time: '2009-10-22T10:08:36.968+02:00'
thumbnail: http://2.bp.blogspot.com/_kVNAYTvQ3QE/SuASOXn-KaI/AAAAAAAACKE/M8rAy4xfGDo/s72-c/Bordered+TextBox.BMP
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-8654290608807268729
blogger_orig_url: https://christian-helle.blogspot.com/2009/10/generic-bordered-control-base-for-netcf.html
---

For quite some time I've been using Google Chrome as my browser. The behavior of some of the controls are similar with Safari (probably because they use the same rendering engine). Anyway, I really liked the way that a border is drawn around the control whenever it receives focus. This inspired me to create a generic base control that draws a border around any control you use it for. The code I use is pretty much the same with my previous article, [Extending the TextBox Control in .NETCF](/2009/10/extending-textbox-control-in-netcf.html).  
  
To accomplish this I created an abstract control that takes a generic parameter of type Control and has a default constructor. You might notice that in my override Font I check whether the control is in design time or run time. If in design time, we need to create a new Font object for the setter using the values passed. If we directly use the value passed the designer will crash, and at times Visual Studio will crash.  
  
Here's how it can look like:  

![](/assets/images/bordered-textbox.jpg)
![](/assets/images/bordered-combobox.jpg)

And here's the code for the base control:

```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class BorderedControlBase<T> : Control
    where T : Control, new()
{
    protected T innerControl;
 
    protected BorderedControlBase()
    {
        innerControl = new T();
        innerControl.GotFocus += delegate { OnGotFocus(EventArgs.Empty); };
        innerControl.LostFocus += delegate { OnLostFocus(EventArgs.Empty); };
        innerControl.TextChanged += delegate { OnTextChanged(EventArgs.Empty); };
        Controls.Add(innerControl);
    }
 
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        innerControl.Bounds = new Rectangle(1, 1, ClientSize.Width - 2, ClientSize.Height - 2);
        Height = innerControl.Height + 2;
    }
 
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        Invalidate();
    }
 
    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
        innerControl.Focus();
    }
 
    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }
 
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(innerControl.Focused ? SystemColors.Highlight : BackColor);
    }
 
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (Environment.OSVersion.Platform != PlatformID.WinCE)
            base.OnPaint(e);
    }
 
    public override Font Font
    {
        get { return base.Font; }
        set
        {
            if (Environment.OSVersion.Platform != PlatformID.WinCE)
            {
                var font = new Font(value.Name, value.Size, value.Style);
                base.Font = innerControl.Font = font;
            }
            else 
                base.Font = innerControl.Font = value;
        }
    }
 
    public override string Text
    {
        get { return innerControl.Text; }
        set { innerControl.Text = value; }
    }
 
    public override bool Focused
    {
        get { return innerControl.Focused; }
    }
}
```

Now that we have this base control we can easily add borders to any control. Here's an example of how to use the the bordered control base:

```csharp
public class BorderedTextBox : BorderedControlBase<TextBox> 
{
}
 
public class BorderedComboBox : BorderedControlBase<ComboBox> 
{
}
```

Of course you will still have to wrap all the members of the wrapped control you wish to expose to access them. Hope you find this useful. If you need the Visual Studio solution then you can grab it [here](/assets/samples/GenericBorderedControlBase.zip).