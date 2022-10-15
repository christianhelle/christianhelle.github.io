---
layout: post
title: How to hide the TextBox caret in .NETCF
date: '2009-10-19T10:51:00.020+02:00'
author: Christian Resma Helle
tags:
- Controls
- How to
- ".NET Compact Framework"
- TextBox
modified_time: '2009-12-23T15:34:34.185+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-5789645034309571763
blogger_orig_url: https://christian-helle.blogspot.com/2009/10/how-to-hide-textbox-caret-in-netcf.html
---

I was trying to help a developer today in the smart device forums who wanted to hide the caret in the TextBox control. I started playing around with the Windows Mobile Platform SDK and I stumbled upon the methods [HideCaret()](http://msdn.microsoft.com/en-us/library/ms929930.aspx?WT.mc_id=DT-MVP-5004822) and [ShowCaret()](http://msdn.microsoft.com/en-us/library/aa453729.aspx?WT.mc_id=DT-MVP-5004822).  
  
The outcome is this simple inherited TextBox control I decided to call TextBoxWithoutCaret :)  

```csharp
class TextBoxWithoutCaret : TextBox
{
    [DllImport("coredll.dll")]
    static extern bool HideCaret(IntPtr hwnd);
 
    [DllImport("coredll.dll")]
    static extern bool ShowCaret(IntPtr hwnd);
 
    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        HideCaret(Handle);
    }
 
    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        ShowCaret(Handle);
    }
}
```

Every time the TextBox control is focused I hide the caret and enable it again when focus is lost. This doesn't really make much practical sense and the only reason I do this is because `HideCaret()` is described to perform a cumulative operation meaning `ShowCaret()` must be called the same number of times `HideCaret()` was called for the caret to be visible again.