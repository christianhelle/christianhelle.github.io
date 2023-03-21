---
layout: post
title: ListView Extended Styles  in .NETCF
date: '2008-10-28T10:53:00.062+01:00'
author: Christian Resma Helle
tags:
- Controls
- ListView
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2009-10-08T16:25:33.886+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-3665746400716942844
blogger_orig_url: https://christian-helle.blogspot.com/2008/10/listview-extended-styles-in-netcf.html
redirect_from:
- /blog/2008/10/28/listview-extended-styles-in-netcf/
---

In this article I would like to demonstrate how to extend the ListView control in the .NET Compact Framework. We will focus on enabling some of the ListView Extended Styles. If we take a look at the Windows Mobile 5.0 Pocket PC SDK we will see that there are certain features of ListView that aren't provided by the .NET Compact Framework.

An example of the ListView extended styles is displaying gridlines around items and subitems, double buffering, and drawing a gradient background. These extended styles can be enabled in native code by using the ListView_SetExtendedListViewStyle macro or by sending `LVM_SETEXTENDEDLISTVIEWSTYLE` messages to the ListView.

### Send Message

We will be using a lot of P/Invoking so let's start with creating an internal static class called NativeMethods. We need a P/Invoke declaration for `SendMessage(HWND, UINT, UINT, UINT)`.

```csharp
internal static class NativeMethods
{
  [DllImport("coredll.dll")]
  public static extern uint SendMessage(IntPtr hwnd, uint msg, uint wparam, uint lparam);
}
```

### Enabling and Disabling Extended Styles

Now that we have our SendMessage P/Invoke declaration in place, we can begin extending the ListView control. Let's start off with creating a class called ListViewEx that inherits from ListView. We need to look into the native header files of the Pocket PC SDK to get the ListView Messages. For now we will only need `LVM_GETEXTENDEDLISTVIEWSTYLE` or `LVM_SETEXTENDEDLISTVIEWSTYLE` message which will be the main focus of all the examples. I will declare my class as a partial class and create all the pieces one by one for each example. Let's create a private method called SetStyle(), this method will enable/disable extended styles for the ListView

```csharp
public partial class ListViewEx : ListView
{
  private const uint LVM_FIRST = 0x1000;
  private const uint LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
  private const uint LVM_GETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 55;

  private void SetStyle(uint style, bool enable)
  {
    uint currentStyle = NativeMethods.SendMessage(
      Handle,
      LVM_GETEXTENDEDLISTVIEWSTYLE,
      0,
      0);

    if (enable)
      NativeMethods.SendMessage(
        Handle,
        LVM_SETEXTENDEDLISTVIEWSTYLE,
        0,
        currentStyle | style);
    else
      NativeMethods.SendMessage(
        Handle,
        LVM_SETEXTENDEDLISTVIEWSTYLE,
        0,
        currentStyle & ~style);
  }
}
```

### Grid Lines

For my first example, let's enable GridLines in the `ListView` control. We can do this by using `LVS_EX_GRIDLINES`. This displays gridlines around items and sub-items and is available only in conjunction with the Details mode.

```csharp
public partial class ListViewEx : ListView
{
  private const uint LVS_EX_GRIDLINES = 0x00000001;

  private bool gridLines = false;
  public bool GridLines
  {
    get { return gridLines; }
    set
    {
      gridLines = value;
      SetStyle(LVS_EX_GRIDLINES, gridLines);
    }
  }
}
```

What the code above did was add the `LVS_EX_GRIDLINES` style to the existing extended styles by using the `SetStyle()` helper method we first created.

An interesting discovery to this is that the Design Time attributes of the Compact Framework ListView control includes the GridLines property. Now that we created the property in the code, when we open the Visual Studio Properties Window for our ListViewEx we will notice that GridLines property we created falls immediately under the "Appearance" category and even includes a description :)

### Double Buffering

Do you notice that when you populate a ListView control with a lot of items, the drawing flickers a lot when you scroll up and down the list? Although it is not in the Pocket PC documentation for Windows Mobile 5.0, the ListView actually has an extended style called `LVS_EX_DOUBLEBUFFER`. Enabling the `LVS_EX_DOUBLEBUFFER` solves the flickering issue and gives the user a more smooth scrolling experience.

```csharp
public partial class ListViewEx : ListView
{
  private const uint LVS_EX_DOUBLEBUFFER = 0x00010000;

  private bool doubleBuffering = false;
  public bool DoubleBuffering
  {
    get { return doubleBuffering; }
    set
    {
      doubleBuffering = value;
      SetStyle(LVS_EX_DOUBLEBUFFER, doubleBuffering);
    }
  }
}
```

### Gradient Background

Another cool extended style is the LVS_EX_GRADIENT. This extended style draws a gradient background similar to the one found in Pocket Outlook. It uses the system colors and fades from right to left. But what is really cool about this is that this is done by the OS. All we had to do was enable the style.

```csharp
public partial class ListViewEx : ListView
{
  private const uint LVS_EX_GRADIENT = 0x20000000;

  private bool gradient = false;
  public bool Gradient
  {
    get { return gradient; }
    set
    {
      gradient = value;
      SetStyle(LVS_EX_GRADIENT, gradient);
    }
  }
}
```

If you want to look more into extended styles then I suggest you check out the Pocket PC Platform SDK documentation. There a few other extended styles that I did not discuss that might be useful for you. You can get the definitions in a file called `commctrl.h` in your Windows Mobile SDK `INCLUDE` directory.