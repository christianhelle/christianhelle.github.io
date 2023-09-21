---
layout: post
title: ListView Custom Drawing in .NETCF
date: '2009-10-08T00:41:00.006+02:00'
author: Christian Resma Helle
tags:
- Controls
- ListView
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2010-06-22T15:04:46.553+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-3730277627073804775
blogger_orig_url: https://christian-helle.blogspot.com/2009/10/listview-custom-drawing-in-netcf.html
---

In this article I would like to demonstrate how to do custom drawing in the ListView control that the .NET Compact Framework provides. I'll be extending the code I published last year in the article entitled [ListView Extended Styles in .NETCF](/2008/10/listview-extended-styles-in-netcf.html)  
  
This is normally a very tedious and frustrating task to do and to accomplish this task we'll have to take advantage of the custom drawing service Windows CE provides for certain controls. A very good reference for custom drawing is an MSDN article called [Customizing a Control's Appearance using Custom Draw](https://learn.microsoft.com/en-us/library/bb761817(VS.85).aspx?WT.mc_id=DT-MVP-5004822). Before going any further, I may have to warn you about the extensive interop code involved in this task.  
  
We'll have to handle the ListView windows messages ourselves, and we accomplish this by subclassing this ListView. [Subclassing a window](https://learn.microsoft.com/en-us/library/ms633570(VS.85).aspx#subclassing_window?WT.mc_id=DT-MVP-5004822) means that we assign a new window procedure for messages that are meant for the ListView. This can be done through the [SetWindowLong()](https://learn.microsoft.com/en-us/library/ms633591(VS.85).aspx?WT.mc_id=DT-MVP-5004822) method with the GWL_WNDPROC parameter. When subclassing, the developer is responsible for choosing which messages they want to handle, which to ignore, and which they let operating system handle. To have the operating system handle the message, a call to [CallWindowProc()](https://learn.microsoft.com/en-us/library/ms633571(VS.85).aspx?WT.mc_id=DT-MVP-5004822) is done using a pointer to original window procedure.  
  
Before setting the new window procedure its important to get a pointer to the original one in case the developer wishes to let the operating system handle the message. This is done through [GetWindowLong()](https://learn.microsoft.com/en-us/library/ms633584(VS.85).aspx?WT.mc_id=DT-MVP-5004822)
  
Let's get started...  
  
First we need to define the interop structures for custom drawing

```csharp
struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}

struct NMHDR
{
    public IntPtr hwndFrom;
    public IntPtr idFrom;
    public int code;
}

struct NMCUSTOMDRAW
{
    public NMHDR nmcd;
    public int dwDrawStage;
    public IntPtr hdc;
    public RECT rc;
    public int dwItemSpec;
    public int uItemState;
    public IntPtr lItemlParam;
}

struct NMLVCUSTOMDRAW
{
    public NMCUSTOMDRAW nmcd;
    public int clrText;
    public int clrTextBk;
    public int iSubItem;
    public int dwItemType;
    public int clrFace;
    public int iIconEffect;
    public int iIconPhase;
    public int iPartId;
    public int iStateId;
    public RECT rcText;
    public uint uAlign;
}
```

Note: In C# (and VB and C++), the StructLayout is Sequencial by default, hence I didn't state it

The P/Invoke declarations we need are the following:

```csharp
[DllImport("coredll.dll")]
static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

[DllImport("coredll")]
static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref RECT lParam);

[DllImport("coredll.dll")]
static extern uint SendMessage(IntPtr hwnd, uint msg, uint wparam, uint lparam);

[DllImport("coredll.dll", SetLastError = true)]
static extern int SetWindowLong(IntPtr hWnd, int nIndex, WndProcDelegate newProc);

[DllImport("coredll.dll", SetLastError = true)]
static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
```

And to make life a bit easier, I created some extension methods to the RECT struct we just defined.

```csharp
static class RectangleExtensions
{
    public static Rectangle ToRectangle(this RECT rectangle)
    {
        return Rectangle.FromLTRB(rectangle.left, rectangle.top, rectangle.right, rectangle.bottom);
    }

    public static RectangleF ToRectangleF(this RECT rectangle)
    {
        return new RectangleF(rectangle.left, rectangle.top, rectangle.right, rectangle.bottom);
    }
}
```

We'll need the following constants defined in the windows platform SDK

```csharp
const int GWL_WNDPROC = -4;
const int WM_NOTIFY = 0x4E;
const int NM_CUSTOMDRAW = (-12);
const int CDRF_NOTIFYITEMDRAW = 0x00000020;
const int CDRF_NOTIFYSUBITEMDRAW = CDRF_NOTIFYITEMDRAW;
const int CDRF_NOTIFYPOSTPAINT = 0x00000010;
const int CDRF_SKIPDEFAULT = 0x00000004;
const int CDRF_DODEFAULT = 0x00000000;
const int CDDS_PREPAINT = 0x00000001;
const int CDDS_POSTPAINT = 0x00000002;
const int CDDS_ITEM = 0x00010000;
const int CDDS_ITEMPREPAINT = (CDDS_ITEM | CDDS_PREPAINT);
const int CDDS_SUBITEM = 0x00020000;
const int CDIS_SELECTED = 0x0001;
const int LVM_GETSUBITEMRECT = (0x1000 + 56);
```

Custom drawing in the ListView will only work in the Details view mode. To ensure this, I set the View to View.Details in the constructor method. Since I'm extending my old ListViewEx (Enables ListView Extended Styles) I'm gonna enable Double buffering, Grid lines, and the Gradient background. I'm gonna enable subclassing on the ListView only when the parent is changed, this is because I need to receive messages sent to the parent control of the ListView. We also need a delegate for the new window procedure and a pointer to the original window procedure. And last but not the least we need the actual window procedure method.

```csharp
delegate IntPtr WndProcDelegate(
    IntPtr hWnd, 
    uint msg, 
    IntPtr wParam, 
    IntPtr lParam);
    IntPtr lpPrevWndFunc;
 
public ListViewCustomDraw()
{
    View = View.Details;
    DoubleBuffering = true;
    GridLines = true;
    Gradient = true;

    ParentChanged += delegate
    {
        lpPrevWndFunc = GetWindowLong(Parent.Handle, GWL_WNDPROC);
        SetWindowLong(Parent.Handle, GWL_WNDPROC, WndProc);
    };
}

private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
{
    if (msg == WM_NOTIFY)
    {
        var nmhdr = (NMHDR)Marshal.PtrToStructure(lParam, typeof(NMHDR));
        if (nmhdr.hwndFrom == Handle && nmhdr.code == NM_CUSTOMDRAW)
            return CustomDraw(hWnd, msg, wParam, lParam);

    }

    return CallWindowProc(lpPrevWndFunc, hWnd, msg, wParam, lParam);
}
```

In the new window procedure, we are only really interested in the WM_NOTIFY message, because this is what the NM_CUSTOMDRAW message is sent through. The LPARAM parameter of the message will contain the NMHDR which then contains the NM_CUSTOMDRAW message. The LPARAM also contains the NMLVCUSTOMDRAW which provide state and information about the ListView.

The trickiest part in performing custom drawing in the ListView is handling the drawing stage. We create a method called CustomDraw to handle the different drawing stages of the ListView

```csharp
private IntPtr CustomDraw(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
{
    int result;
    var nmlvcd = (NMLVCUSTOMDRAW)Marshal.PtrToStructure(lParam, typeof(NMLVCUSTOMDRAW));
    switch (nmlvcd.nmcd.dwDrawStage)
    {
        case CDDS_PREPAINT:
            result = CDRF_NOTIFYITEMDRAW;
            break;

        case CDDS_ITEMPREPAINT:
            var itemBounds = nmlvcd.nmcd.rc.ToRectangle();
            if ((nmlvcd.nmcd.uItemState & CDIS_SELECTED) != 0)
            {
                using (var brush = new SolidBrush(SystemColors.Highlight))
                using (var graphics = Graphics.FromHdc(nmlvcd.nmcd.hdc))
                    graphics.FillRectangle(brush, itemBounds);
            }

            result = CDRF_NOTIFYSUBITEMDRAW;
            break;

        case CDDS_SUBITEM | CDDS_ITEMPREPAINT:
            var index = nmlvcd.nmcd.dwItemSpec;
            var rect = new RECT();
            rect.top = nmlvcd.iSubItem;
            SendMessage(Handle, LVM_GETSUBITEMRECT, index, ref rect);
            rect.left += 2;

            Color textColor;
            if ((nmlvcd.nmcd.uItemState & CDIS_SELECTED) != 0)
                textColor = SystemColors.HighlightText;
            else
                textColor = SystemColors.ControlText;

            using (var brush = new SolidBrush(textColor))
            using (var graphics = Graphics.FromHdc(nmlvcd.nmcd.hdc))
                graphics.DrawString(Items[index].SubItems[nmlvcd.iSubItem].Text,
                                    Font,
                                    brush,
                                    rect.ToRectangleF());

            result = CDRF_SKIPDEFAULT | CDRF_NOTIFYSUBITEMDRAW;
            break;

        default:
            result = CDRF_DODEFAULT;
            break;
    }

    return (IntPtr)result;
}
```

In the first stage we handle is the `CDDS_PREPAINT`. Here we return `CDRF_NOTIFYITEMDRAW` to tell that we want to handle drawing of the row ourselves. After this we receive the `CDDS_ITEMPREPAINT` where we can draw the entire row.  
  
We check if the row is selected through the `uItemState` field of `NMCUSTOMDRAW`, if this field has the `CDIS_SELECTED` flag then it means the item is selected, hence we draw a fill rectangle. After handling the `CDDS_ITEMPREPAINT`, we return `CDRF_NOTIFYSUBITEMDRAW` to tell that we want to draw the sub items ourselves.  
  
For drawing the sub items we need to handle `CDDS_SUBITEM | CDDS_ITEMPREPAINT`. We can get the position index of the item through the dwItemSpec field of NMCUSTOMDRAW. To get the bounds of the current sub item we send the [LVM_GETSUBITEMRECT](https://learn.microsoft.com/en-us/library/bb761075(VS.85).aspx?WT.mc_id=DT-MVP-5004822) message to the ListView and pass a pointer to [RECT](https://learn.microsoft.com/en-us/library/dd162897(VS.85).aspx?WT.mc_id=DT-MVP-5004822) as the `LPARAM`. Before sending this message, set the "top" field of the RECT to the index of the sub item (retrieved from `iSubItem` field of `NMLVCUSTOMDRAW`. After drawing the sub item we return `CDRF_SKIPDEFAULT | CDRF_NOTIFYSUBITEMDRAW` to tell that we only care about handling the next sub item.  
  
Well I hope you guys find this interesting and helpful. To keep things simple, I only demonstrated displaying plan text and a plain rectangle for the selection.  
  
If you're interested in the full source code then you can grab it [here](/assets/samples/ListViewCustomDrawing.zip).