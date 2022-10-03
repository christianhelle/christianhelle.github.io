---
layout: post
title: ListView Background Image in .NETCF
date: '2009-10-11T22:00:00.006+02:00'
author: Christian Resma Helle
tags:
- Controls
- ListView
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2009-10-12T16:35:50.803+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-138828835341635689
blogger_orig_url: https://christian-helle.blogspot.com/2009/10/listview-background-image.html
---

In this short entry I'd like to demonstrate how to display a background image in the ListView control. For this we will send the [LVM_SETBKIMAGE](http://msdn.microsoft.com/en-us/library/aa453514.aspx) or the [LVM_GETBKIMAGE](http://msdn.microsoft.com/en-us/library/aa453498.aspx) message to the ListView control with the [LVBKIMAGE](http://msdn.microsoft.com/en-us/library/aa453422.aspx) struct as the `LPARAM`. Unfortunately, the Windows CE version of `LVBKIMAGE` does not support `LVBKIF_SOURCE_URL` flag which allows using an image file on the file system for the background image of the ListView.  
  
The layout of the background image can be either tiled or specified by an offset percentage. The background image is not affected by custom drawing, unless of course you decide to fill each sub item rectangle. For setting the background image we use the `LVBKIF_SOURCE_HBITMAP` flag together with the layout which is either `LVBKIF_STYLE_TILE` or `LVBKIF_STYLE_NORMAL`. If we set the layout to `LVBKIF_STYLE_NORMAL`, then we have the option of setting where the image will be drawn by setting the value of xOffsetPercentage and yOffsetPercentage.  
  
In this example I'd like to make use of extension methods to add the SetBackgroundImage() and GetBackgroundImage() methods to ListView. This can of course be easily used to in a property to an inherited ListView. 

```csharp
public static class ListViewExtensions
{
    [DllImport("coredll")]
    static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref LVBKIMAGE lParam);
 
    const int LVM_FIRST = 0x1000;
    const int LVM_SETBKIMAGE = (LVM_FIRST + 138);
    const int LVM_GETBKIMAGE = (LVM_FIRST + 139);
    const int LVBKIF_SOURCE_NONE = 0x00000000;
    const int LVBKIF_SOURCE_HBITMAP = 0x00000001;
    const int LVBKIF_STYLE_TILE = 0x00000010;
    const int LVBKIF_STYLE_NORMAL = 0x00000000;
 
    struct LVBKIMAGE
    {
        public int ulFlags;
        public IntPtr hbm;
        public IntPtr pszImage; // not supported
        public int cchImageMax;
        public int xOffsetPercent;
        public int yOffsetPercent;
    }
 
    public static void SetBackgroundImage(this ListView listView, Bitmap bitmap)
    {
        SetBackgroundImage(listView, bitmap, false);
    }
 
    public static void SetBackgroundImage(this ListView listView, Bitmap bitmap, bool tileLayout)
    {
        SetBackgroundImage(listView, bitmap, tileLayout, 0, 0);
    }
 
    public static void SetBackgroundImage(
        this ListView listView,
        Bitmap bitmap,
        bool tileLayout,
        int xOffsetPercent,
        int yOffsetPercent)
    {
        LVBKIMAGE lvBkImage = new LVBKIMAGE();
        if (bitmap == null)
            lvBkImage.ulFlags = LVBKIF_SOURCE_NONE;
        else
        {
            lvBkImage.ulFlags = LVBKIF_SOURCE_HBITMAP | (tileLayout ? LVBKIF_STYLE_TILE : LVBKIF_STYLE_NORMAL);
            lvBkImage.hbm = bitmap.GetHbitmap();
            lvBkImage.xOffsetPercent = xOffsetPercent;
            lvBkImage.yOffsetPercent = yOffsetPercent;
        }
 
        SendMessage(listView.Handle, LVM_SETBKIMAGE, 0, ref lvBkImage);
    }
 
    public static Bitmap GetBackgroundImage(this ListView listView)
    {
        LVBKIMAGE lvBkImage = new LVBKIMAGE();
        lvBkImage.ulFlags = LVBKIF_SOURCE_HBITMAP;
 
        SendMessage(listView.Handle, LVM_GETBKIMAGE, 0, ref lvBkImage);
 
        if (lvBkImage.hbm == IntPtr.Zero)
            return null;
        else
            return Bitmap.FromHbitmap(lvBkImage.hbm);
    }
}
```

Here's an example of exposing the background image as a property in an inherited ListView by using the extension methods above.

```csharp
class ListViewEx : ListView
{
    public Bitmap BackgroundImage
    {
        get { return this.GetBackgroundImage(); }
        set { this.SetBackgroundImage(value, BackgroundLayout == BackgroundImageLayout.Tile); }
    }
 
    public BackgroundImageLayout BackgroundLayout { get; set; }
 
    public enum BackgroundImageLayout
    {
        Tile,
        Center
    }
}
```

A small catch with the ListView background image is that it is only supported in Windows CE 5.0 and later. Hope you found this information useful.