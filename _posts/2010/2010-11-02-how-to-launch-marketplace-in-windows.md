---
layout: post
title: How to launch the Marketplace in the Windows Phone 7 Emulator
date: '2010-11-02T13:10:00.000+01:00'
author: Christian Resma Helle
tags:
- Windows Phone 7
modified_time: '2010-11-02T13:10:38.101+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-5050046703164963045
blogger_orig_url: https://christian-helle.blogspot.com/2010/11/how-to-launch-marketplace-in-windows.html
---

You probably noticed by now that the default Windows Phone 7 emulator only contains Internet Explorer and the Settings. I stumbled upon a tip from **Daniel Vaughan** about Launchers and Choosers today and I learned about a task that can start up the Marketplace Hub. Unfortunately you will have to create an application that calls into the [Launcher API](https://learn.microsoft.com/en-us/previous-versions/windows/apps/ff769550(v=vs.105)?WT.mc_id=DT-MVP-5004822) to accomplish this.  
  
Here's what you need to do:  
  
1) Create a new Silverlight Windows Phone Application  
2) In your MainPage.cs and add a using directive to the [Microsoft.Phone.Tasks](https://learn.microsoft.com/en-us/previous-versions/ff428753(v=vs.110)?WT.mc_id=DT-MVP-5004822) namespace.  
3) Add the following in the constructor of MainPage.cs after calling the `InitializeComponent()` method:  
  
```csharp
var task = new MarketplaceHubTask();
task.ContentType = MarketplaceContentType.Applications;
task.Show();
```
  
4) Launch the application and you should be good to go.  
  
Here's how the Marketplace looks like on the emulator:

<iframe width="560" height="315" src="https://www.youtube.com/embed/XQ45Yy8b_Dc" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
  
You can read more about Launchers and Choosers [here](https://learn.microsoft.com/en-us/previous-versions/windows/apps/ff769542(v=vs.105)?WT.mc_id=DT-MVP-5004822).