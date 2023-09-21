---
layout: post
title: Multi-platform Mobile Development - Creating a List Based UI
date: '2011-01-19T16:05:00.002+01:00'
author: Christian Resma Helle
tags:
- Multi-platform Mobile Development
- Windows Phone 7
- Android
- Windows Mobile
modified_time: '2011-01-19T21:42:08.095+01:00'
thumbnail: https://2.bp.blogspot.com/_kVNAYTvQ3QE/TTRCGWEKb6I/AAAAAAAACss/MAOLmek1544/s72-c/wp7%2Blist.png
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6203645804872199357
blogger_orig_url: https://christian-helle.blogspot.com/2011/01/multi-platform-mobile-development_19.html
---

Here's the first installment on my series on multi-platform mobile development articles. A common practice on displaying information to a mobile device user is a list. A list is one of the best ways to display a group of information allowing the user to easily select which specific details he/she wishes to display.

A good example of decent list implementations is the Inbox on pretty much all mobile devices. Most Inbox list implementations display sender, title, date, size, and a preview of the message body. A good list is not only bound to textual information but also visual. Most Inbox implementation displays the information using bold fonts if the message is unread

In this article I would like to demonstrate how to implement customized list based UI's on the following platforms:
- Windows Phone 7
- Windows Mobile using .NET Compact Framework
- Android

Let's get started...

## Windows Phone 7

This is definitely the easiest platform to target, in fact this is by far the easiest platform I've ever worked with. Development times on this platform are a lot shorter than any other platform I've worked with. I've been working with Windows CE based phones for the last 7 or so and I definitely think that this is the best Windows CE based OS ever. There unfortunately a few down sides like lack of a native code API and limited platform integration, but considering the performance and development ease, it is for most cases worth it. The best part with designing UI's for Windows Phone 7 is that I don't have to care about the design very much, I just ask my designer / graphic artist to shine up my XAML file and I can concentrate on the code.

A Visual Studio 2010 project template is actually provided by default for creating a list based UI makes things easier. This project template is called a Windows Phone Databound Applicaton, the description goes "A project for creating Windows Phone applications using List and Navigation controls". This project creates 2 pages, one for displaying the list, and the other for displaying details of this list.

The code examples for Windows Phone 7 uses the Model-View-ViewModel. This pattern is heavily used and I guess one can say an accepted standard in developing Windows Phone 7 applications. I'm not gonna go deep into the pattern in this article, so I assume that you do a bit of home work on MVVM.

To display a list Windows Phone 7 we use the ListBox control in XAML. This will represent the View.

```xml
<ListBox ItemsSource="{Binding Items}">
  <ListBox.ItemTemplate>
    <DataTemplate>
      <StackPanel>
        <TextBlock Text="{Binding LineOne}" TextWrapping="Wrap"/>
        <TextBlock Text="{Binding LineTwo}" TextWrapping="Wrap"/>
      </StackPanel>
    </DataTemplate>
  </ListBox.ItemTemplate>
</ListBox>
```

Our ViewModel is implemented in code. A ViewModel class should implement the INotifyPropertyChanged interface for the View to be able to respond to changes in the ViewModel.

```csharp
public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
 
    protected void NotifyPropertyChanged(String propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (null != handler)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
 
public class ItemViewModel : ViewModelBase
{
    private string _lineOne;
    public string LineOne
    {
        get { return _lineOne; }
        set
        {
            if (value != _lineOne)
            {
                _lineOne = value;
                NotifyPropertyChanged("LineOne");
            }
        }
    }
 
    private string _lineTwo;
    public string LineTwo
    {
        get { return _lineTwo; }
        set
        {
            if (value != _lineTwo)
            {
                _lineTwo = value;
                NotifyPropertyChanged("LineTwo");
            }
        }
    }
}
 
public class MainViewModel : ViewModelBase
{
    private MainModel model;
 
    public MainViewModel()
    {
        model = new MainModel();
        Items = model.GetData();
    }
 
    public ObservableCollection<ItemViewModel> Items { get; private set; }
}
```

The ViewModel code above contains an instance of the Model. The Model in this naive example just returns a populated collection of ItemViewModel.

```csharp
public class MainModel
{
    public ObservableCollection<ItemViewModel> GetData()
    {
        return new ObservableCollection<ItemViewModel> 
        {
            new ItemViewModel() { LineOne = "runtime one", LineTwo = "Maecenas praesent accumsan bibendum" },
            new ItemViewModel() { LineOne = "runtime two", LineTwo = "Dictumst eleifend facilisi faucibus" },
            new ItemViewModel() { LineOne = "runtime three", LineTwo = "Habitant inceptos interdum lobortis" },
            new ItemViewModel() { LineOne = "runtime four", LineTwo = "Nascetur pharetra placerat pulvinar" }
        };
    }
}
```

Here's how the application looks like:

![](/assets/images/wp7-list.png)

## Windows Mobile

This is actually a pretty decent platform and offers a huge selection of low level API's for platform integration. The OS also offers full multi tasking and the ability to create applications that run behind scenes. The down side of course to all that fun stuff is that you have to do a lot of things the hard way. Implementing a decent list based UI in this platforms can be done in 2 ways: Using the Windows CE custom drawing service; Creating an Owner Drawn List Control. Both require writing a few hundred lines of code.

For this example we create an Owner Drawn List. For those who are not familiar what that means, we draw the entire control from scratch, manually. We create a class that inherits from System.Windows.Forms.Control (the base class of all UI components) and override the drawing, resizing, and input methods. It's a bit tedious, but most of the code in owner drawn controls can be re-used as base classes for other owner drawn controls.

Let's start off with creating an owner drawn list base class.

```csharp
abstract class OwnerDrawnListBase<T> : Control
{
    int selectedIndex;
    int visibleItemsPortrait;
    int visibleItemsLandscape;
    VScrollBar scrollBar;
 
    protected OwnerDrawnListBase()
        : this(7, 4)
    {
    }
 
    protected OwnerDrawnListBase(int visibleItemsPortrait, int visibleItemsLandscape)
    {
        this.visibleItemsPortrait = visibleItemsPortrait;
        this.visibleItemsLandscape = visibleItemsLandscape;
 
        Items = new List<T>();
 
        scrollBar = new VScrollBar { Parent = this, Visible = false, SmallChange = 1 };
        scrollBar.ValueChanged += (sender, e) => Invalidate();
    }
 
    public List<T> Items { get; private set; }
 
    public int SelectedIndex
    {
        get { return selectedIndex; }
        set
        {
            selectedIndex = value;
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, EventArgs.Empty);
            Invalidate();
        }
    }
 
    public event EventHandler SelectedIndexChanged;
 
    protected virtual void OnSelectedIndexChanged(EventArgs e)
    {
        if (SelectedIndexChanged != null)
            SelectedIndexChanged(this, e);
    }
 
    public T SelectedItem
    {
        get
        {
            if (selectedIndex >= 0 && selectedIndex < Items.Count)
                return Items[selectedIndex];
            else
                return null;
        }
    }
 
    protected Bitmap OffScreen { get; private set; }
 
    protected int VisibleItems
    {
        get
        {
            if (Screen.PrimaryScreen.Bounds.Height > Screen.PrimaryScreen.Bounds.Width)
                return visibleItemsPortrait;
            else
                return visibleItemsLandscape;
        }
    }
 
    protected int ItemHeight
    {
        get { return Height / VisibleItems; }
    }
 
    protected int ScrollPosition
    {
        get { return scrollBar.Value; }
    }
 
    protected bool ScrollBarVisible
    {
        get { return scrollBar.Visible; }
    }
 
    protected int ScrollBarWidth
    {
        get { return scrollBar.Width; }
    }
 
    protected int DrawCount
    {
        get
        {
            if (ScrollPosition + scrollBar.LargeChange > scrollBar.Maximum)
                return scrollBar.Maximum - ScrollPosition + 1;
            else
                return scrollBar.LargeChange;
        }
    }
 
    #region Overrides
 
    protected override void OnResize(EventArgs e)
    {
        scrollBar.Bounds = new Rectangle(
            ClientSize.Width - scrollBar.Width,
            0,
            scrollBar.Width,
            ClientSize.Height);
 
        Dispose(OffScreen);
 
        if (Items.Count > VisibleItems)
        {
            scrollBar.Visible = true;
            scrollBar.LargeChange = VisibleItems;
            OffScreen = new Bitmap(ClientSize.Width - scrollBar.Width, ClientSize.Height);
        }
        else
        {
            scrollBar.Visible = false;
            scrollBar.LargeChange = Items.Count;
            OffScreen = new Bitmap(ClientSize.Width, ClientSize.Height);
        }
        DrawBorder();
 
        scrollBar.Maximum = Items.Count - 1;
    }
 
    private void DrawBorder()
    {
        using (var gfx = Graphics.FromImage(OffScreen))
        using (var pen = new Pen(SystemColors.ControlText))
            gfx.DrawRectangle(pen, new Rectangle(0, 0, OffScreen.Width - 1, OffScreen.Height - 1));
    }
 
    protected override void OnMouseDown(MouseEventArgs e)
    {
        // Update the selected index based on where the user clicks
        SelectedIndex = scrollBar.Value + (e.Y / ItemHeight);
        if (SelectedIndex > Items.Count - 1)
            SelectedIndex = -1;
 
        if (!Focused)
            Focus();
 
        base.OnMouseUp(e);
    }
 
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // To avoid flickering, do all drawing in OnPaint
    }
 
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Dispose(OffScreen);
        base.Dispose(disposing);
    }
 
    #endregion
 
    protected static void Dispose(IDisposable obj)
    {
        if (obj != null)
        {
            obj.Dispose();
            obj = null;
        }
    }
}
```

The class above implements the basic functionality of an owner drawn list. It hands resizing the off screen bitmap that serves as a double buffer, handles the scroll bar visibility, and handles updating the selected index. One can implement responding to keyboard input or gestures from here as well.

Next we create a class where we define how the control is drawn. This class inherits from our owner drawn list base class.

```csharp
class CustomListViewItem
{
    public string LineOne { get; set; }
    public string LineTwo { get; set; }
}
 
class CustomListView : OwnerDrawnListBase<CustomListViewItem>
{
    const int topleft = 3;
 
    StringFormat noWrap;
    Pen pen;
    SolidBrush backgroundBrush;
    SolidBrush selectedBrush;
    SolidBrush selectedTextBrush;
    SolidBrush textBrush;
    Font headerFont;
 
    public override Font Font
    {
        get { return base.Font; }
        set
        {
            base.Font = value;
            Dispose(headerFont);
            headerFont = new Font(value.Name, value.Size, FontStyle.Bold);
        }
    }
 
    public CustomListView()
    {
        pen = new Pen(ForeColor);
        textBrush = new SolidBrush(ForeColor);
        backgroundBrush = new SolidBrush(BackColor);
        selectedTextBrush = new SolidBrush(SystemColors.HighlightText);
        selectedBrush = new SolidBrush(SystemColors.Highlight);
        noWrap = new StringFormat(StringFormatFlags.NoWrap);
        headerFont = new Font(base.Font.Name, base.Font.Size, FontStyle.Bold);
    }
 
    protected override void OnPaint(PaintEventArgs e)
    {
        using (var gfx = Graphics.FromImage(OffScreen))
        {
            gfx.FillRectangle(backgroundBrush, 1, 1, Width - 2, Height - 2);
 
            int top = 1;
            bool lastItem = false;
            bool itemSelected = false; ;
 
            for (var i = ScrollPosition; i < ScrollPosition + DrawCount; i++)
            {
                if (top > 1)
                    lastItem = Height - 1 < top;
 
                // Fill the rectangle if the item is selected
                itemSelected = i == SelectedIndex;
                if (itemSelected)
                {
                    if (!lastItem)
                    {
                        gfx.FillRectangle(
                            selectedBrush,
                            1,
                            (i == ScrollPosition) ? top : top + 1,
                            ClientSize.Width - (ScrollBarVisible ? ScrollBarWidth : 2),
                            (i == ScrollPosition) ? ItemHeight : ItemHeight - 1);
                    }
                    else
                    {
                        gfx.FillRectangle(
                            selectedBrush,
                            1,
                            top + 1,
                            ClientSize.Width - (ScrollBarVisible ? ScrollBarWidth : 1),
                            ItemHeight);
                    }
                }
 
                // Draw seperator lines after each item unless the item is the last item in the list
                if (!lastItem)
                {
                    gfx.DrawLine(
                        pen,
                        1,
                        top + ItemHeight,
                        ClientSize.Width - (ScrollBarVisible ? ScrollBarWidth : 2),
                        top + ItemHeight);
                }
 
                // Get the dimensions for creating the drawing areas
                var item = Items[i];
                var size = gfx.MeasureString(item.LineOne, Font);
                var rectheight = ItemHeight - (int)size.Height - 6;
                var rectwidth = ClientSize.Width - (ScrollBarVisible ? ScrollBarWidth : 5);
 
                // Draw line one with an offset of 3 pixels from the top of the rectangle 
                // using a bold font (no text wrapping)
                gfx.DrawString(
                    item.LineOne,
                    headerFont,
                    (i == SelectedIndex) ? selectedTextBrush : textBrush,
                    new RectangleF(topleft, top + 3, rectwidth, rectheight),
                    noWrap);
 
                // Draw line two with an offset of 3 pixels from the bottom of line one 
                // (no text wrapping)
                gfx.DrawString(
                    item.LineTwo,
                    Font,
                    (i == SelectedIndex) ? selectedTextBrush : textBrush,
                    new RectangleF(topleft, top + size.Height + 6, rectwidth, rectheight),
                    noWrap);
 
                // Set the top for the next item
                top += ItemHeight;
            }
 
            e.Graphics.DrawImage(OffScreen, 0, 0);
        }
    }
 
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Dispose(headerFont);
            Dispose(backgroundBrush);
            Dispose(textBrush);
            Dispose(selectedTextBrush);
            Dispose(selectedBrush);
            Dispose(pen);
        }
 
        base.Dispose(disposing);
    }
}
```

Once that is in place you can just drag it in from the toolbox or dynamically add it to the Form in runtime.

```csharp
var lv = new CustomListView();
lv.Dock = DockStyle.Fill;
Controls.Add(lv);
 
lv.Items.AddRange(new List<CustomListViewItem>
{
    new CustomListViewItem { LineOne = "runtime one", LineTwo = "Maecenas praesent accumsan bibendum" },
    new CustomListViewItem { LineOne = "runtime two", LineTwo = "Dictumst eleifend facilisi faucibus" },
    new CustomListViewItem { LineOne = "runtime three", LineTwo="Habitant inceptos interdum lobortis" },
    new CustomListViewItem { LineOne = "runtime four", LineTwo="Nascetur pharetra placerat pulvinar" },
    new CustomListViewItem { LineOne = "runtime five", LineTwo = "Maecenas praesent accumsan bibendum" },
    new CustomListViewItem { LineOne = "runtime six", LineTwo = "Dictumst eleifend facilisi faucibus" },
    new CustomListViewItem { LineOne = "runtime seven", LineTwo="Habitant inceptos interdum lobortis" },
    new CustomListViewItem { LineOne = "runtime eight", LineTwo="Nascetur pharetra placerat pulvinar" }
});
```

Here's how the custom list view looks like in a Windows Mobile 6.5.3 emulator

![](/assets/images/wm-list.jpg)

You can grab the source for Windows Mobile application above here.

## Android

Creating decent list based UI's is also pretty easy. The designer experience is unfortunately not as elegant as what Windows Phone 7 has to offer, but shares the same idea. The user interface layout of Android applications are described in XML files and are parsed during runtime. In some occasions it seems easier to create the UI layout in runtime through code instead of struggling with the UI designer. This is probably because of my lack of patience with the tool or because of my lack of experience using it. Either way, I think it could have been done in a much smarter way.

To create a list based UI in Android we can create a class that extends from the ListActivity class. The ListActivity base class contains a List control set to fill the parent, it comes in handy if you want a UI with nothing but a list control. In android development, you usually setup the UI and do other initialization methods in the onCreate() method, our example will do the same. We set the data source of our list control by calling setListAdapter().

To have a more flexible and customizable we use the ArrayAdapter for presenting our data source to the screen. To optimize performance, we use an object called convertView that ArrayAdapter exposes, we can store a single instance of a class containing UI text components and just update the text for that instance. This is done by overriding the ArrayAdapter getView method.

Here's the code for implementing the ListActivity and ArrayAdapter

```java
import java.util.ArrayList;
import java.util.List;

import android.app.Activity;
import android.app.ListActivity;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class MainActivity extends ListActivity {
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setListAdapter(new ListItemActivity(this, R.layout.list_item, createList()));
    }

    private List<ListItem> createList() {
        List<ListItem> list = new ArrayList<ListItem>();
        list.add(new ListItem("runtime one", "Maecenas praesent accumsan bibendum"));
        list.add(new ListItem("runtime two", "Dictumst eleifend facilisi faucibus"));
        list.add(new ListItem("runtime three", "Habitant inceptos interdum lobortis"));
        list.add(new ListItem("runtime four", "Nascetur pharetra placerat pulvinar"));
        return list;
    }

    class ListItem {
        public String lineOne;
        public String lineTwo;

        public ListItem(String lineOne, String lineTwo) {
            this.lineOne = lineOne;
            this.lineTwo = lineTwo;
        }
    }

    class ListItemActivity extends ArrayAdapter<ListItem> {
        private Activity context;
        private List<ListItem> items;

        public ListItemActivity(Activity context, int textViewResourceId, List<ListItem> items) {
            super(context, textViewResourceId, items);
            this.context = context;
            this.items = items;
        }

        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
            ViewHolder holder;
            if (convertView == null) {
                LayoutInflater inflater = context.getLayoutInflater();
                convertView = inflater.inflate(R.layout.list_item, parent, false);
                holder = new ViewHolder();
                holder.lineOne = (TextView) convertView.findViewById(R.id.lineOne);
                holder.lineTwo = (TextView) convertView.findViewById(R.id.lineTwo);
                convertView.setTag(holder);
            } else {
                holder = (ViewHolder) convertView.getTag();
            }

            ListItem item = items.get(position);
            holder.lineOne.setText(item.lineOne);
            holder.lineTwo.setText(item.lineTwo);
            return convertView;
        }
    }

    static class ViewHolder {
        TextView lineOne;
        TextView lineTwo;
    }
}
```

Here's how the XML layout file is for the list item (list_item.xml)

```xml
<?xml version="1.0" encoding="utf-8"?>
 
<LinearLayout
  xmlns:android="https://schemas.android.com/apk/res/android"
  android:layout_width="fill_parent"
  android:layout_height="fill_parent"
  android:orientation="vertical">
 
  <TextView
    android:id="@+id/lineOne"
    android:layout_width="fill_parent"
    android:layout_height="wrap_content"
    android:paddingTop="12dp"
    android:paddingLeft="12dp"
    android:textSize="18sp"
    android:textStyle="bold"
  />
 
  <TextView
    android:id="@+id/lineTwo"
    android:layout_width="fill_parent"
    android:layout_height="wrap_content"
    android:paddingLeft="12dp"
    android:paddingBottom="12dp"
  />
 
</LinearLayout>
 ```


Here's how the applications looks like on an Android 2.3 emulator

![](/assets/images/android-list.png)

You can grab the source for Android application above here.

So this basically all I have for now. I plan to go into detail by breaking down each part of the code samples I provided for all 3 platforms, or perhaps add another platform as well. I hope you found this useful.