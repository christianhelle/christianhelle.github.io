---
layout: post
title: Integrating HTML5 and Javascript with Windows Phone 7
date: '2012-03-08T22:57:00.001+01:00'
author: Christian Resma Helle
tags:
- Windows Phone 7
- Javascript
- HTML5
modified_time: '2012-03-09T23:29:57.513+01:00'
thumbnail: http://4.bp.blogspot.com/-Q2YUMg7r0PI/T1Zqdn7__UI/AAAAAAAAC2E/7PqcN7PY25Y/s72-c/JavascriptCSharpInterop.jpg
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-2972343304265197073
blogger_orig_url: https://christian-helle.blogspot.com/2012/03/integrating-html5-and-javascript-with.html
---

Everyone, everywhere is talking about HTML5 these days. I myself did a recent talk at the [Danish Developer Conference 2012 on Windows Phone and HTML5](/2012/03/html5-and-windows-phone-7.html). I get the point and I see the power and beauty of HTML5. But, HTML5 is as far as I can see not entirely ready yet and as for mobile applications, I would always choose writing a native application that takes full advantage of the platform and not just an application that runs in a browser, even if the the browser component is hosted in a native application. I think developers should really learn to appreciate the platform more.  

In this article I would like to explain how to integrate HTML5 + Javascript in a Windows Phone application and the same demonstrate how to call a .NET method from Javascript and how to call a Javascript method from .NET  

So here's what we need to do to get started:  

1.  Create a Windows Phone Silverlight application
2.  Add a WebBrowser component on the main page
3.  Set the **IsScriptEnabled** property of the WebBrowser component to **true**
4.  Add an event handler to the **ScriptNotify** event of the WebBrowser component
5.  Create a folder on the project called HTML and add the HTML, Javascript, and Stylesheet assets to this folder
6.  Write code to copy the HTML related assets to IsolatedStorage
7.  Set the source of the WebBrowser component to the main HTML page

Simple isn't it?  

**How it works**  

The steps above really do seem to be quite simple, and yes it really is. For Javascript to call into the host of the WebBrowser control we can use the **window.external.notify()** method. This is the same approach for having Javascript code execute code in the host application in other platforms. The **window.external.notify()** method takes a string which can be used to contain meta data that describes what you want the host to do. And for .NET code to execute Javascript code we use the **InvokeScript()** method of the WebBrowser control. The **InvokeScript()** method takes a string parameter that describes the Javascript method to execute, and a collection of strings that describe the arguments to be passed to the Javascript method to execute. If the method that will invoke a javascript function from the host is running on a non-UI thread (worker thread) then the best approach to using this method is by calling **InvokeScript("eval", "methodName(args1,args2,args3)")** instead of passing the name of the method to be invoked as the first method argument.  

Here's a diagram I used in DDC 2012 that illustrates the process mentioned above:  

![](/assets/images/javascript-csharp-interop.jpg)

For this example, we will have an application that hosts a HTML5 page that displays memory information of the device (as shown in the screenshot below)  

![](/assets/images/html-wp7.png)

And here's the code...  

**Default.html (HTML5 + Javascript)**  

The code below is going to be used as a local html file that is to be copied to isolated storage. Let's put this in a folder called HTML  

```html
<!DOCTYPE html>
<html>
<head>
    <meta name="viewport" content="width=480, height=800, user-scalable=no" />
    <meta name="MobileOptimized" content="width" />
    <meta name="HandheldFriendly" content="true" />
    <title>HTML5 and Windows Phone 7</title>
    <style>
        body
        {
            color: White;
            background-color: Black;
            font-family: 'Segoe WP Semibold';
            text-align: left;
        }
        h3
        {
            font-size: 20pt;
        }
        input
        {
            color: #ffffff;
            background-color: #000000;
            border: 2px solid white;
            vertical-align: baseline;
            font-size: 17pt;
            min-width: 40px;
            min-height: 40px;
            margin: 5;
        }
    </style>
</head>
<body onload="onLoad()">
    <div>
        <h3>
            Current memory usage:</h3>
        <input id="memoryUsage" type="text" value="0" />
        <h3>
            Memory usage limit:</h3>
        <input id="memoryUsageLimit" type="text" value="0" />
        <h3>
            Peak memory usage:</h3>
        <input id="peakMemoryUsage" type="text" value="0" />
        <h3>
            Total memory:</h3>
        <input id="totalMemory" type="text" value="0" />
    </div>
    <script type="text/javascript">
        function onLoad() {
            window.external.notify("getMemoryUsage");
        }

        function getMemoryUsageCallback(memoryUsage, memoryUsageLimit, peakMemoryUsage, totalMemory) {
            document.getElementById("memoryUsage").value = memoryUsage;
            document.getElementById("memoryUsageLimit").value = memoryUsageLimit;
            document.getElementById("peakMemoryUsage").value = peakMemoryUsage;
            document.getElementById("totalMemory").value = totalMemory;
        }
    </script>
</body>
</html>
```

**MainPage.xaml**  

The code below is the main page of the Silverlight application that will host the HTML content  

```xaml
<phone:PhoneApplicationPage x:Class="PhoneApp.MainPage"
                           xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                           xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone"
                           xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Microsoft.Phone"
                           xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                           xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                           mc:Ignorable="d"
                           d:DesignWidth="480"
                           d:DesignHeight="768"
                           FontFamily="{StaticResource PhoneFontFamilyNormal}"
                           FontSize="{StaticResource PhoneFontSizeNormal}"
                           Foreground="{StaticResource PhoneForegroundBrush}"
                           SupportedOrientations="Portrait"
                           Orientation="Portrait"
                           shell:SystemTray.IsVisible="True"
                           Loaded="PhoneApplicationPage_Loaded">

    <Grid x:Name="LayoutRoot"
         Background="Transparent">
        <phone:WebBrowser Name="browser"
                         IsScriptEnabled="True"
                         Source="HTML/Default.html"
                         ScriptNotify="browser_ScriptNotify" />
    </Grid>

</phone:PhoneApplicationPage>
```

**MainPage.xaml.cs**  

And here's the code behind the xaml file  

```csharp
public partial class MainPage : PhoneApplicationPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
    {
        using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
        {
            if (!store.DirectoryExists("HTML")) store.CreateDirectory("HTML");
            CopyToIsolatedStorage("HTML\\Default.html", store);
        }
    }

    private static void CopyToIsolatedStorage(string file, IsolatedStorageFile store, bool overwrite = true)
    {
        if (store.FileExists(file) && !overwrite)
            return;

        using (Stream resourceStream = Application.GetResourceStream(new Uri(file, UriKind.Relative)).Stream)
        using (IsolatedStorageFileStream fileStream = store.OpenFile(file, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            int bytesRead;
            var buffer = new byte[resourceStream.Length];
            while ((bytesRead = resourceStream.Read(buffer, 0, buffer.Length)) > 0)
                fileStream.Write(buffer, 0, bytesRead);
        }
    }

    private void browser_ScriptNotify(object sender, NotifyEventArgs e)
    {
        var response = new object[]
                       {
                           DeviceStatus.ApplicationCurrentMemoryUsage,
                           DeviceStatus.ApplicationMemoryUsageLimit,
                           DeviceStatus.ApplicationPeakMemoryUsage,
                           DeviceStatus.DeviceTotalMemory
                       };
        browser.InvokeScript("getMemoryUsageCallback", response.Select(c => c.ToString()).ToArray());
    }
}
```

What happens in the code above is that when the main page has loaded, the html assets are copied to isolated storage and loaded into the web browser component as a local file. When **ScriptNotify** is triggered, the Silverlight application retrieves memory information using the **DeviceStatus** class and passes this information back to the WebBrowser component by invoking the getMemoryUsageCallback() method using the **InvokeScript()** method of the WebBrowser component  

The sample above is a very basic and naive but it demonstrates something that can provide endless platform interop possibilities. I hope you found this useful.

You can grab the full source code the sample above [here](https://1drv.ms/u/s!AnAsdrR_HlPKhk70qLoVRRSQ9M1S?e=bikf6e)