---
layout: post
title: Accessing the Accelerometer from HTML5 and Javascript with Windows Phone 7
date: '2012-03-10T00:16:00.000+01:00'
author: Christian Resma Helle
tags:
- Windows Phone 7
- Javascript
- HTML5
modified_time: '2012-03-10T07:03:55.389+01:00'
thumbnail: https://1.bp.blogspot.com/-8M4jkELOzHs/T1qIaxexTGI/AAAAAAAAC2g/W1Z79calWJw/s72-c/AccelerometerEmulator.png
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-3692951769254666368
blogger_orig_url: https://christian-helle.blogspot.com/2012/03/accessing-accelerometer-from-html5-and.html
---

In my [previous post](/2012/03/integrating-html5-and-javascript-with.html) I discussed how to have Javascript code hosted in a WebBrowser control execute .NET code in the host application and vice versa. I also demonstrated how to retrieve and display device status information using HTML5 hosted in a WebBrowser control.  

For this sample I would like to demonstrate how to access the [Accelerometer](https://learn.microsoft.com/en-us/library/microsoft.devices.sensors.accelerometer.aspx?WT.mc_id=DT-MVP-5004822) sensor from HTML5 and Javascript. To make things more interesting, the Accelerometer reading data will be constantly updated every 100 milliseconds and .NET code will repeatedly call a Javascript method as Accelerometer reading data gets updated  

[![](/assets/images/accelerometer-emulator.png)

[![](/assets/images/accelerometer-tool.png)

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
        <h3>X:</h3>
        <input id="x" type="text" value="0" />
        <h3>Y:</h3>
        <input id="y" type="text" value="0" />
        <h3>Z:</h3>
        <input id="z" type="text" value="0" />
    </div>

    <script type="text/javascript">
        function onLoad() {
            window.external.notify("startAccelerometer");
        } 

        function accelerometerCallback(x, y, z) {
            document.getElementById("x").value = x;
            document.getElementById("y").value = y;
            document.getElementById("z").value = z;
        }
    </script>
</body>
</html>
```

**MainPage.xaml**  

The code below is the main page of the Silverlight application that will host the HTML content  


```xaml
<phone:PhoneApplicationPage x:Class="PhoneApp.MainPage"
                           xmlns="https://schemas.microsoft.com/winfx/2006/xaml/presentation"
                           xmlns:x="https://schemas.microsoft.com/winfx/2006/xaml"
                           xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone"
                           xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Microsoft.Phone"
                           xmlns:d="https://schemas.microsoft.com/expression/blend/2008"
                           xmlns:mc="https://schemas.openxmlformats.org/markup-compatibility/2006"
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
    private Microsoft.Devices.Sensors.Accelerometer accelerometer; 

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
        if (e.Value == "startAccelerometer")
        {
            if (accelerometer == null)
            {
                accelerometer = new Microsoft.Devices.Sensors.Accelerometer { TimeBetweenUpdates = TimeSpan.FromMilliseconds(100) };
                accelerometer.CurrentValueChanged += (o, args) => Dispatcher.BeginInvoke(() =>
                {
                    var x = args.SensorReading.Acceleration.X.ToString("0.000");
                    var y = args.SensorReading.Acceleration.Y.ToString("0.000");
                    var z = args.SensorReading.Acceleration.Z.ToString("0.000"); 

                    browser.InvokeScript("eval", string.Format("accelerometerCallback({0},{1},{2})", x, y, z));
                });

                accelerometer.Start();
            }
        }
    }
}
```

What happens in the code above is that a Javascript method is executed that notifies the host application telling it to start the Accelerometer when the HTML has loaded. We then add an event handler to the Accelerometers [CurrentValueChanged](https://learn.microsoft.com/en-us/library/hh239103.aspx?WT.mc_id=DT-MVP-5004822) event that invokes the accelerometerCallback Javascript method and passing in Accelerometer reading data as the arguments. Notice that I use **eval** as the Javascript method to invoke and passing the method call as an argument, this is because the accelerometer reading data is retrieved on a worker thread and for some reason an unknown system error occurs even when executing code on the UI thread through the Page Dispatcher.BeginInvoke() method. I figured out that using **eval** was the only way to execute Javascript code from a .NET worker thread.  

I hope you found this useful. You can grab the full source code for the example [here](/assets/samples/Accelerometer.zip)