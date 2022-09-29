---
layout: post
title: Microsoft Advertising SDK for Windows Phone 7
date: '2010-10-11T15:17:00.027+02:00'
author: Christian Resma Helle
tags:
- Windows Phone 7
modified_time: '2010-10-13T11:39:40.550+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-1127109241581408403
blogger_orig_url: https://christian-helle.blogspot.com/2010/10/microsoft-advertising-sdk-for-windows.html
---

I recently stumbled upon the Microsoft Advertising SDK for Windows Phone 7. At the client side, it provides a control called AdControl that retrieves banner information based on the specified ApplicationId and AdUnitId. In order to use the Microsoft Advertising’s ad delivery system, you first need to create a Microsoft pubCenter account.

Here's some information extracted from the help file included in the SDK:

    The Microsoft Advertising SDK for Windows Phone 7 provides an AdControl that you can use to publish advertisements in Windows Phone 7 applications. The Microsoft Advertising AdControl communicates with Microsoft servers that deliver ads. When these servers return ads to the AdControl, the AdControl will render the ads within your Windows Phone 7 application.

    In order to use the Microsoft Advertising’s ad delivery system, you first need to create a Microsoft pubCenter account. Microsoft pubCenter is an advertising publisher management system that enables you to create ad placements and collect advertising revenue. Once you create an account, you will register your mobile applications by using Microsoft pubCenter. When a Windows Phone 7 application is registered with Microsoft pubCenter it will receive a unique mobile application identifier (ApplicationId).

    When a mobile application displays a window, the application can define a space within the window for the presentation of advertisements. Each ad placement that is presented is called an ad unit. You will define and create mobile ad units by using Microsoft pubCenter, and each mobile ad unit will be assigned a unique ad unit identifier (AdUnitId).

    The ApplicationId and the AdUnitId are required for your Windows Phone 7 application to request ads from Microsoft Advertising’s ad delivery system. The ApplicationId and the AdUnitId identify the ad unit that is delivered to a Windows Phone 7 application and the publisher that will receive credit for displaying the ad unit.

    It takes only a few easy steps to register with Microsoft pubCenter, create ad units, manage revenues, and integrate the Microsoft Advertising Mobile AdControl for Windows Phone 7 into your application.

Sounds fairly simple but not everything can be perfect in a first release. The first problem I found was that the pubCenter Registration and Payment will only be available for publishers from the United States. So to be eligable for payments one must for example have a valid US address and a valid Tax Information Number (TIN) if the publisher is a business entity. As I'm not based in the US, I haven't tried this yet. But based on what I've read in the documentation, using the AdControl in a Windows Phone 7 application seems fairly easy.


Here are some C# and XAML snippets:


The following C# code instantiates a new AdControl and sets mandatory targeting parameters.

```csharp
AdControl ctrl = new AdControl();
ctrl.ApplicationId = "testapplication";
ctrl.AdUnitId = "testadunit";
this.ContentGrid.Children.Add(ctrl);
```

The following C# code instantiates a new AdControl with manual ad rotation.

```csharp
string applicationId = "testapplication";
string adUnitId = "testadunit";
bool isAutoRotation = false;
AdControl ctrl = new AdControl(applicationId, adUnitId, AdModel.Contextual, isAutoRotation);
 
ctrl.Width = 480;
ctrl.Height = 80;
this.ContentGrid.Children.Add(ctrl);
```

The following XAML instantiates a new AdControl with mandatory targeting parameters.

```xml
<Grid xmlns:adctl="clr-namespace:Microsoft.Advertising.Mobile.UI;assembly=Microsoft.Advertising.Mobile.UI" Grid.Row="1">
   <adctl:AdControl Height="80" Width="480" AdUnitId="Test" ApplicationId="Test" AdModel="Contextual" />
</Grid>
```

I'm looking forward for these tools to be available in Europe so I can try them out.