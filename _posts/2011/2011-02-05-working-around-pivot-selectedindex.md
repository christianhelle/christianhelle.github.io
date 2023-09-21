---
layout: post
title: Working around Pivot SelectedIndex limitations in Windows Phone 7
date: '2011-02-05T00:30:00.001+01:00'
author: Christian Resma Helle
tags:
- Windows Phone 7
modified_time: '2011-02-05T00:30:00.358+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-566654096796432301
blogger_orig_url: https://christian-helle.blogspot.com/2011/02/working-around-pivot-selectedindex.html
---

I've been working on an application with 2 pages, a main page and a content page. The content page contains a Pivot control with a few pivot items. The main page does nothing but navigate to the content page and suggest which pivot item to display. The only reason the main page exists is to display the information in the pivot item headers in a more graphical and elegant way.  
  
For some reason I can't set the displayed pivot index to be the third item. I wanted to do this on the OnNavigatedTo event of the content page but whenever I attempt doing so an exception is thrown. Every other pivot item works fine, which I think is really weird.  
  
To load the content page, I navigate to the page by passing some information of the pivot index I wish to be displayed. Something like this:  

```csharp
NavigationService.Navigate(new Uri("/ContentPage.xaml?index=" + index, UriKind.Relative));
```  

If the value of index in the code above is set to 2 then I get an exception, any other valid value works fine. A value out of range (less than 0 or greater than 5) throws an out of range exception which is the behavior anyone would expect.  
  
Here's the XAML definition of the content page  
  
```xml
<phone:PhoneApplicationPage
    x:Class="WindowsPhonePivotApplication.ContentPage"
    xmlns="https://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="https://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone"
    xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Microsoft.Phone"
    xmlns:controls="clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls"
    xmlns:d="https://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="https://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d" d:DesignWidth="480" d:DesignHeight="768"
    FontFamily="{StaticResource PhoneFontFamilyNormal}"
    FontSize="{StaticResource PhoneFontSizeNormal}"
    Foreground="{StaticResource PhoneForegroundBrush}"
    SupportedOrientations="Portrait"  Orientation="Portrait"
    shell:SystemTray.IsVisible="True">
 
  <Grid x:Name="LayoutRoot" Background="Transparent">
    <controls:Pivot Name="pivot" Title="CONTENT PAGE">
      <controls:PivotItem Header="first" />
      <controls:PivotItem Header="second" />
      <controls:PivotItem Header="third" />
      <controls:PivotItem Header="fourth" />
      <controls:PivotItem Header="fifth" />
      <controls:PivotItem Header="sixth" />
    </controls:Pivot>
  </Grid> 
</phone:PhoneApplicationPage>
```  
  
To work around this limitation, you can handle the Loaded event of the page and update the pivot selected index from there. Here's an example how to do it:  
  
```csharp
public partial class ContentPage : PhoneApplicationPage
{
    private int pivotIndex;
 
    public ContentPage()
    {
        InitializeComponent();
 
        Loaded += delegate { pivot.SelectedIndex = pivotIndex; };
    }
 
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        string value;
        if (NavigationContext.QueryString.TryGetValue("index", out value))
        {
            pivotIndex = 0;
            int.TryParse(value, out pivotIndex);
        }
    }
}
```  
  
I'm not sure if this limitation is by design or it's a bug in the control. Either way I managed to get it to work the way I wanted it to. Hopefully I'm not the only one who ran across this and that you found this information useful.