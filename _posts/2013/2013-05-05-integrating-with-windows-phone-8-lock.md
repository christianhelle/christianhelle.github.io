---
layout: post
title: Integrating with the Windows Phone 8 Lock Screen
date: '2013-05-05T23:31:00.000+02:00'
author: Christian Resma Helle
tags: Windows Phone 8
modified_time: '2013-05-06T17:02:52.269+02:00'
thumbnail: http://lh6.ggpht.com/--tghTed1JPs/UXWn7qFyc7I/AAAAAAAADFg/ih6cMqbzzKk/s72-c/settings_thumb%25255B1%25255D.png?imgmax=800
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-5193764592110998860
blogger_orig_url: https://christian-helle.blogspot.com/2013/05/integrating-with-windows-phone-8-lock.html
---

A nice new feature in Windows Phone 8 is integration with the lock screen. As a developer, you can now display notification icons the same way the Outlook app displays New Mail notifications. You can also change the background image of the lock screen from your app. The information used for displaying notification is the exact same information that an app uses to display notifications on the app’s primary tile regardless if the app is pinned to the start screen or not  

**Notification Icon**  

To display an icon on the lock screen you must follow a small set of strict rules on how your image file should be. The image has to be a **38 x 38 PNG image that contains only white pixels and some levels of transparency**. Yes, I know it’s a bit strict but it makes sense as the icon notifications are designed to be very discreet.  
**App Manifest**  

You need to define a few extensions to let the operating system allow your app to integrate with the lock screen. So here’s what we need to do, open the WMAppManifest.xml file using an XML editor, to do this right click on the WMAppManifest.xml file and select **Open With**, and use the **XML (Text) Editor**. First, we need to define the **<DeviceLockImageURI>** element in the primary token. The DeviceLockImageURI describes which image file to display in the lock screen as an icon. To define DeviceLockImageURI, insert the following element in the **<PrimaryToken>**  

```xml
<DeviceLockImageURI IsRelative="true" IsResource="false">Assets\YourLockImage.png</DeviceLockImageURI>
```

Then, insert the following lines after the between the **<Tokens>** and **<ScreenResolutions>** definitions  

```xml
<Extensions>
  <Extension ExtensionName="LockScreen_Notification_IconCount"
             ConsumerID="{111DFF24-AA15-4A96-8006-2BFF8122084F}"
             TaskID="_default" />
  <Extension ExtensionName="LockScreen_Notification_TextField"
             ConsumerID="{111DFF24-AA15-4A96-8006-2BFF8122084F}"
             TaskID="_default" />
  <Extension ExtensionName="LockScreen_Background"
             ConsumerID="{111DFF24-AA15-4A96-8006-2BFF8122084F}"
             TaskID="_default" />
</Extensions>
```

**Lock screen settings**  

Before our app can display notifications we need to configure our Lock Screen settings to allow the type of notification that we are interested in displaying. Currently an app can display 3 types of notifications: background image; detailed status; and quick status. Detailed status is similar to the textual calendar notifications that are displayed using the Outlook Calendar app. Quick status is similar to the way the Outlook Mail app displays notifications, an icon and a count indicator.  

To configure the device to display notifications from your app, go to the settings app and select Lock screen  

[![settings](http://lh6.ggpht.com/--tghTed1JPs/UXWn7qFyc7I/AAAAAAAADFg/ih6cMqbzzKk/settings_thumb%25255B1%25255D.png?imgmax=800 "settings")](http://lh5.ggpht.com/-lfeyk0bPGvM/UXWn0xJI-iI/AAAAAAAADFc/NO9mBQR8FA8/s1600-h/settings%25255B3%25255D.png)  

In the lock screen settings, choose the notification type that will display notifications from our app  

[![image](http://lh3.ggpht.com/-C_RetikC3Ok/UXWn9Fzh1pI/AAAAAAAADFw/a7arSl9p5qI/image_thumb%25255B6%25255D.png?imgmax=800 "image")](http://lh3.ggpht.com/-it0vr-dA13A/UXWn8Jmp51I/AAAAAAAADFo/UEflSF2kKLY/s1600-h/image%25255B12%25255D.png)  

For this example we’ll show a quick status notification  

[![quick status on](http://lh3.ggpht.com/-Blwu72A0ags/UXWn-1FadzI/AAAAAAAADGA/CKrpwD6H_y0/quick%252520status%252520on_thumb%25255B3%25255D.png?imgmax=800 "quick status on")](http://lh3.ggpht.com/-LIB-eOQqh7k/UXWn9s-rG9I/AAAAAAAADF4/IgzfDyNrgSA/s1600-h/quick%252520status%252520on%25255B5%25255D.png)  

**Code**  

To update the background image of the lock screen we need use the [LockScreen](http://msdn.microsoft.com/en-us/library/windowsphone/develop/windows.phone.system.userprofile.lockscreen(v=vs.105).aspx) class of the [UserProfile](http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj207562(v=vs.105).aspx) API. First we check if the user configured the app to be able to set the background of the lock screen, we can do this through [LockScreenManager](http://msdn.microsoft.com/en-us/library/windowsphone/develop/windows.phone.system.userprofile.lockscreenmanager(v=vs.105).aspx) class. If the app isn’t allowed to change the lock screen background then we can open the lock screen settings page.  

**Lock screen background (C#)**

```csharp
if (await LockScreenManager.RequestAccessAsync() == LockScreenRequestResult.Granted)
{
    var uri = new Uri("ms-appx:///Assets/LockScreenImage.png", UriKind.Absolute);
    LockScreen.SetImageUri(uri);
}
else
{
    // Open the Settings -> Lock Screen settings
    await Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
}
```

To display an icon notification just update the primary application tile with a notification. We can do this by using the [ShellTile](http://msdn.microsoft.com/en-US/library/windowsphone/develop/microsoft.phone.shell.shelltile(v=vs.105).aspx) API

****Display Notification (C#)**

```csharp
var tile = ShellTile.ActiveTiles.First();
var data = new FlipTileData
                {
                    Count = 1,
                    Title = "Lock Screen Demo"
                };
tile.Update(data);
```

To clear the notification just update the primary application tile to its original state  

**Clear Notification (C#)**

```csharp
var tile = ShellTile.ActiveTiles.First();
var data = new FlipTileData
                {
                    Count = 0,
                    Title = "Lock Screen Demo"
                };
tile.Update(data);
```

Pretty simple isn’t it?  

**Testing on the Emulator**  

To test on the Windows Phone emulator you can use the [Simulation Dashboard](http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj206953(v=vs.105).aspx) which integrates directly into Visual Studio. To launch this, go to Tools –> Simulation Dashboard. You can use this tool to Lock and Unlock the emulator to test your apps lock screen integration  

I hope you found this interesting. You can grab the source code [here](https://skydrive.live.com/embed?cid=CA531E7FB4762C70&amp;resid=CA531E7FB4762C70%2137080&amp;authkey=AOXEBkTA6wCceKI)