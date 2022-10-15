---
layout: post
title: Scrollable UISegmentedControl for Xamarin.iOS
date: '2019-06-09T14:52:00.005+02:00'
author: Christian Resma Helle
tags: 
- iOS
- Xamarin
modified_time: '2019-06-11T18:54:11.769+02:00'
thumbnail: https://1.bp.blogspot.com/-gC-VaqZBJrI/XPz-aZ02FWI/AAAAAAAAWBs/orVSUyZjkvU7TtX0wxZ2lcqCoLPF5X7OQCLcBGAs/s72-c/scrollablesegmentedcontrol-nuget.png
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-1168052297225765485
blogger_orig_url: https://christian-helle.blogspot.com/2019/06/scrollable-uisegmentedcontrol-for.html
---

A few years ago, I had a full time job as a device developer in the Music Streaming industry. The applications we produced at the time targeted consumers and had a huge focus on UX and UI. One of the requirements our designers had was to have scrollable tabs. This was 5 years ago and before [Xamarin.Forms](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/?WT.mc_id=DT-MVP-5004822) existed so we built the [iOS app](http://itunes.apple.com/dk/app/yousee-musik/id1108892163) and [Android app](http://play.google.com/store/apps/details?id=dk.yousee.musik) separately. We used [MvvmCross](https://www.mvvmcross.com/) and the shared a lot of core code but the UI components were done per OS. When we started, the company did a recent switch to go full on with .NET on everything, so not only did we share code between apps, but we shared code across all systems within the entire organization.  

Scrollable tabs come for free in Android using the built-in control [TabLayout](https://developer.android.com/reference/android/support/design/widget/TabLayout) but on iOS we needed to re-create the [UISegmentedControl](https://developer.apple.com/documentation/uikit/uisegmentedcontrol) and add scroll/pan/swipe functionality to it. On native code, you had a few options to choose from, so the first task was to find the best native implementation of it and port it to C#. A few Google searches later I found the [HMSegmentedControl](https://github.com/HeshamMegid/HMSegmentedControl) written by [Hesham Abd-Elmegid](https://hesh.am/). The component at the time was written in a single file and was a drop in replacement for the UISegmentedControl. It was functional, elegent, and directly portable to C#. It was perfect!  

A few hours of focused coding later I managed to port the entire thing to C# and created a [Github project](https://github.com/christianhelle/ScrollableSegmentedControl) for it. I originally called it HMSegmentedControl as a tribute to the author (I also sent him a thank you email at the time) but later changed it to ScrollableSegmentedControl as it was a better and more descriptive name that states exactly what it does. Recently, I re-visited this project to clean up, modernize the code, and structure of the repository. I added a [README](https://github.com/christianhelle/ScrollableSegmentedControl/blob/master/README.md) file with a useful description, screenshots, and code examples. I also published a [NuGet package](https://www.nuget.org/packages/scrollablesegmentedcontrol) called [ScrollableSegmentedControl](https://www.nuget.org/packages/scrollablesegmentedcontrol) to make it easier for others to use while keeping the responsibility of maintaining it.  

So using the component is quite trivial. Here's all you need to do:  

Add the ScrollableSegmentedControl [NuGet package](https://www.nuget.org/packages/scrollablesegmentedcontrol)  

![](/assets/images/scrollablesegmentedcontrol-nuget.png)

then you create an instance of **ScrollableSegmentedControl** and you add it to a View  

```csharp
using System;  
using ChristianHelle.Controls.iOS;  
using CoreGraphics;  
using UIKit;  

namespace ScrollableSegmentedControlSample  
{  
    public partial class ViewController : UIViewController  
    {  
        public ViewController(IntPtr handle) : base(handle)  
        {  
        }  

        public override void ViewDidLoad()  
        {  
            base.ViewDidLoad();  
            CreateScrollableSegmentedControl();  
        }  

        private void CreateScrollableSegmentedControl()  
        {  
            var sectionTitles = new[] { "One", "Two", "Three", "Four", "Five", "Six" };  
            View.AddSubview(new ScrollableSegmentedControl(sectionTitles)  
            {  
                Font = UIFont.FromName("STHeitiSC-Light", 18.0f),  
                Frame = new CGRect(0, 60, View.Frame.Width, 40),  
                SegmentEdgeInset = new UIEdgeInsets(0, 10, 0, 10),  
                SelectionStyle = ScrollableSegmentedControlSelectionStyle.FullWidthStripe,  
                SelectionIndicatorLocation = ScrollableSegmentedControlIndicatorLocation.Down  
            });  
        }  
    }  
}  
```

This would result in a segmented control that looks like this:  

![](/assets/images/scrollablesegmentedcontrol.png)  

and can look like one of these examples depending on the **SelectionStyle** and **SelectionIndicatorLocation**  

![](/assets/images/scrollablesegmentedcontrol-dark.png)  

This post is probably 5 years too late but since I just only recently made it publicly available as a NuGet package I thought I should write a short article about it. I hope you find this useful