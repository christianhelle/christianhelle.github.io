---
layout: post
title: Extending the TextBox Control in .NETCF
date: '2009-10-21T11:59:00.023+02:00'
author: Christian Resma Helle
tags:
- Controls
- ".NET Compact Framework"
- TextBox
modified_time: '2009-10-22T10:14:52.939+02:00'
thumbnail: https://4.bp.blogspot.com/_kVNAYTvQ3QE/SuAURsLv9DI/AAAAAAAACKU/P219Ia9hhRI/s72-c/Extended+TextBox.jpg
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-5141829400233523679
blogger_orig_url: https://christian-helle.blogspot.com/2009/10/extending-textbox-control-in-netcf.html
---

In this article I'm gonna extend the TextBox control. To accomplish this I will wrap the built in TextBox control in a composite control. Wrapping a control in this context means forwarding events and methods to the composite control so the parent of this control will treat it like a normal TextBox.  
  
In this example I will demonstrate how how to display a border around the TextBox when it has focused and show how to add a property for disabling and enabling the caret.  
  
To draw the border I make sure that there is 1 pixel of space around the actual TextBox control. In the OnResize event of the composite control I set the bounds of the inner TextBox the bounds of the composite control minus 1 pixel on each side. If the inner TextBox is not in Multiline mode then the height of the control is forced to match its font size, if so then I resize the composite control to be 1 pixel taller. And draw the border I just clear the drawing surface with the highlight system color if it is Focused, and with white if not.  
  
To enable and disable the caret, we add a boolean property called EnableCaret. This property is checked every time the control receives or loses focus, to call the native [HideCaret()](https://learn.microsoft.com/en-us/library/ms929930.aspx?WT.mc_id=DT-MVP-5004822) and [ShowCaret()](https://learn.microsoft.com/en-us/library/aa453729.aspx?WT.mc_id=DT-MVP-5004822). As I demonstrated in my previous article called [How to hide the TextBox caret in .NETCF](/2009/10/how-to-hide-textbox-caret-in-netcf.html)  
  
Here's how it looks like:  

![](/assets/images/extended-textbox.jpg)
![](/assets/images/extended-textbox-selected.jpg)

And here's the code:

```csharp
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
 
namespace ExtendedTextBox
{
    public class TextBoxEx : Control
    {
        [DllImport("coredll.dll")]
        static extern bool HideCaret(IntPtr hwnd);
 
        [DllImport("coredll.dll")]
        static extern bool ShowCaret(IntPtr hwnd);
 
        private TextBox textBox;
 
        public TextBoxEx()
        {
            textBox = new TextBox();
            textBox.GotFocus += (sender, e) => OnGotFocus(EventArgs.Empty);
            textBox.LostFocus += (sender, e) => OnLostFocus(EventArgs.Empty);
            textBox.TextChanged += (sender, e) => OnTextChanged(EventArgs.Empty);
            textBox.KeyDown += (sender, e) => OnKeyDown(e);
            textBox.KeyPress += (sender, e) => OnKeyPress(e);
            textBox.KeyUp += (sender, e) => OnKeyUp(e);
            Controls.Add(textBox);
        }
 
        public bool EnabledCaret { get; set; }
 
        #region Wrapped Properties
 
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                if (Environment.OSVersion.Platform != PlatformID.WinCE)
                {
                    var font = new Font(value.Name, value.Size, value.Style);
                    base.Font = textBox.Font = font;
                }
                else
                    base.Font = textBox.Font = value;
            }
        }
 
        public override string Text
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }
        public bool AcceptsReturn
        {
            get { return textBox.AcceptsReturn; }
            set { textBox.AcceptsReturn = value; }
        }
 
        public bool AcceptsTab
        {
            get { return textBox.AcceptsTab; }
            set { textBox.AcceptsTab = value; }
        }
 
        public bool CanUndo
        {
            get { return textBox.CanUndo; }
        }
 
        public bool Focused
        {
            get { return textBox.Focused; }
        }
 
        public new IntPtr Handle
        {
            get { return textBox.Handle; }
        }
 
        public bool HideSelection
        {
            get { return textBox.HideSelection; }
            set { textBox.HideSelection = value; }
        }
 
        public int MaxLength
        {
            get { return textBox.MaxLength; }
            set { textBox.MaxLength = value; }
        }
 
        public bool Modified
        {
            get { return textBox.Modified; }
            set { textBox.Modified = value; }
        }
 
        public bool Multiline
        {
            get { return textBox.Multiline; }
            set { textBox.Multiline = value; }
        }
 
        public char PasswordChar
        {
            get { return textBox.PasswordChar; }
            set { textBox.PasswordChar = value; }
        }
 
        public bool ReadOnly
        {
            get { return textBox.ReadOnly; }
            set { textBox.ReadOnly = value; }
        }
 
        public override Color BackColor
        {
            get { return textBox.BackColor; }
            set { textBox.BackColor = value; }
        }
 
        public ScrollBars ScrollBars
        {
            get { return textBox.ScrollBars; }
            set { textBox.ScrollBars = value; }
        }
 
        public string SelectedText
        {
            get { return textBox.SelectedText; }
            set { textBox.SelectedText = value; }
        }
 
        public int SelectionLength
        {
            get { return textBox.SelectionLength; }
            set { textBox.SelectionLength = value; }
        }
 
        public int SelectionStart
        {
            get { return textBox.SelectionStart; }
            set { textBox.SelectionStart = value; }
        }
 
        public HorizontalAlignment TextAlign
        {
            get { return textBox.TextAlign; }
            set { textBox.TextAlign = value; }
        }
 
        public int TextLength
        {
            get { return textBox.TextLength; }
        }
 
        public bool WordWrap
        {
            get { return textBox.WordWrap; }
            set { textBox.WordWrap = value; }
        }
 
        #endregion
 
        #region Wrapped Methods
 
        public void ScrollToCaret()
        {
            textBox.ScrollToCaret();
        }
 
        public void Select(int start, int length)
        {
            textBox.Select(start, length);
        }
 
        public void SelectAll()
        {
            textBox.SelectAll();
        }
 
        public void Undo()
        {
            textBox.Undo();
        }
 
        #endregion
 
        #region Overridden Methods
 
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            textBox.Bounds = new Rectangle(
                1,
                1,
                ClientSize.Width - 2,
                ClientSize.Height - 2);
            Height = textBox.Height + 2;
        }
 
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
 
            if (!EnabledCaret)
                HideCaret(Handle);
        }
 
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
 
            if (!EnabledCaret)
                ShowCaret(Handle);
        }
 
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(textBox.Focused ? SystemColors.Highlight : Color.White);
        }
 
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
 
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            textBox.Dispose();
        }
 
        #endregion
    }
}
```

Hope you find this useful. If you need the Visual Studio solution then you can grab it [here](/assets/samples/ExtendedTextBox.zip).