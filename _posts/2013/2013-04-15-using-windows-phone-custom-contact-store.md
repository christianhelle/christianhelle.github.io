---
layout: post
title: Using the Windows Phone Custom Contact Store
date: '2013-04-15T14:41:00.001+02:00'
author: Christian Resma Helle
tags: 
- Windows Phone 8
modified_time: '2019-06-11T18:56:03.181+02:00'
thumbnail: http://lh5.ggpht.com/-d_5My4CT_Gg/UWv1X_a6jpI/AAAAAAAADEY/8DdL5NTtKe0/s72-c/id_cap_contacts_thumb%25255B2%25255D.png?imgmax=800
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-975202751219361332
blogger_orig_url: https://christian-helle.blogspot.com/2013/04/using-windows-phone-custom-contact-store.html
---

In previous versions of Windows Phone, you could always query the contact store to retrieve contact or calendar items. During that time I always wondered why I couldn’t just create my own contacts that can be shared with other applications and accessed through the People Hub. I guess more people had this problem and in Windows Phone 8 this has been addressed. Windows Phone 8 introduced the custom contact store in which apps can create contacts that are accessible from the People Hub and from other apps. Items in the custom contact store may only be modified by app that created them

**How to create contacts in Windows Phone 8**

In this section I would like to demonstrate how to use the custom contact store API. In order to do so we’ll create a UI that accepts the display name, email address, and mobile phone number. To make it a bit more fancy, we’ll add a feature that accepts a photo which can be loaded either from the camera or the media library

So here’s the code…

```xaml
<phone:PhoneApplicationPage x:Class="CustomContactStore.MainPage"
                           xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                           xmlns:phone="clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone"
                           xmlns:shell="clr-namespace:Microsoft.Phone.Shell;assembly=Microsoft.Phone"
                           xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                           xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                           mc:Ignorable="d"
                           FontFamily="{StaticResource PhoneFontFamilyNormal}"
                           FontSize="{StaticResource PhoneFontSizeNormal}"
                           Foreground="{StaticResource PhoneForegroundBrush}"
                           SupportedOrientations="Portrait"
                           Orientation="Portrait"
                           shell:SystemTray.IsVisible="True">

    <Grid x:Name="LayoutRoot" Background="Transparent">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <StackPanel x:Name="TitlePanel" Grid.Row="0" Margin="12,17,0,28">
            <TextBlock Text="CUSTOM CONTACT STORE" Style="{StaticResource PhoneTextNormalStyle}" Margin="12,0" />
            <TextBlock Text="Sample" Margin="9,-7,0,0" Style="{StaticResource PhoneTextTitle1Style}" />
        </StackPanel>

        <Grid x:Name="ContentPanel" Grid.Row="1" Margin="12,0,12,0">
            <StackPanel>
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>
                    <Grid.RowDefinitions>
                        <RowDefinition />
                        <RowDefinition />
                        <RowDefinition />
                        <RowDefinition />
                    </Grid.RowDefinitions>
                    <TextBlock Text="Display Name" VerticalAlignment="Center" />
                    <TextBox Grid.Column="1" Name="displayName" />
                    <TextBlock Grid.Row="1" Text="Email" VerticalAlignment="Center" />
                    <TextBox Grid.Row="1" Grid.Column="1" Name="email" />
                    <TextBlock Grid.Row="2" VerticalAlignment="Center" Text="Mobile" />
                    <TextBox Grid.Row="2" Grid.Column="1" Name="mobile" />
                </Grid>
                <Button Content="Attach New Photo" Click="AttachNewPhotoClicked" />
                <Button Content="Attach Existing Photo" Click="AttachExistingPhotoClicked" />
                <Button Content="Save Contact" Click="AddClicked" />
            </StackPanel>
        </Grid>
    </Grid>

</phone:PhoneApplicationPage>
```

Code Behind (C#)

```csharp
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Tasks;
using Windows.Phone.PersonalInformation;

namespace CustomContactStore
{
    public partial class MainPage
    {
        private Stream photo;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void AddClicked(object sender, RoutedEventArgs e)
        {
            var store = await ContactStore.CreateOrOpenAsync();
            var contact = new StoredContact(store)
                              {
                                  DisplayName = displayName.Text
                              };

            var props = await contact.GetPropertiesAsync();
            props.Add(KnownContactProperties.Email, email.Text);
            props.Add(KnownContactProperties.MobileTelephone, mobile.Text);

            if (photo != null)
                await contact.SetDisplayPictureAsync(photo.AsInputStream());

            await contact.SaveAsync();

            if (photo != null)
                photo.Dispose();
        }

        private void AttachNewPhotoClicked(object sender, RoutedEventArgs e)
        {
            var task = new CameraCaptureTask();
            task.Completed += OnTaskOnCompleted;
            task.Show();
        }

        private void OnTaskOnCompleted(object o, PhotoResult result)
        {
            photo = result.ChosenPhoto;
        }

        private void AttachExistingPhotoClicked(object sender, RoutedEventArgs e)
        {
            var task = new PhotoChooserTask();
            task.Completed += OnTaskOnCompleted;
            task.Show();
        }
    }
}
```

To create a custom contact we need to use the [ContactStore](http://learn.microsoft.com/en-us/library/windowsphone/develop/jj207529(v=vs.105).aspx?WT.mc_id=DT-MVP-5004822) API, we create an instance of this using the helper method [CreateOrOpenAsync()](http://learn.microsoft.com/en-us/library/windowsphone/develop/jj207576(v=vs.105).aspx?WT.mc_id=DT-MVP-5004822). Now that we have an instance of the contact store, we create an instance of a [StoredContact](http://learn.microsoft.com/en-us/library/windowsphone/develop/jj207727(v=vs.105).aspx?WT.mc_id=DT-MVP-5004822) and set the DisplayName property to the value of the display name entered in the UI. The StoredContact object is very limited but we can add [KnownContactProperties](http://learn.microsoft.com/en-US/library/windowsphone/develop/windows.phone.personalinformation.knowncontactproperties(v=vs.105).aspx?WT.mc_id=DT-MVP-5004822) such as Email and MobileTelephone. This is done by using the [GetPropertiesAsync()](http://learn.microsoft.com/en-us/library/windowsphone/develop/windows.phone.personalinformation.storedcontact.getpropertiesasync(v=vs.105).aspx?WT.mc_id=DT-MVP-5004822) method of the StoredContact instance. The photos can be attached using the [CameraCaptureTask](http://learn.microsoft.com/en-us/library/windowsphone/develop/hh394006(v=vs.105).aspx?WT.mc_id=DT-MVP-5004822) or the [PhotoChooserTask](http://learn.microsoft.com/en-us/library/windowsphone/develop/hh394019(v=vs.105).aspx?WT.mc_id=DT-MVP-5004822). We attach the photos by calling the [SetDisplayPictureAsync()](http://learn.microsoft.com/en-us/library/windowsphone/develop/windows.phone.personalinformation.storedcontact.getdisplaypictureasync(v=vs.105).aspx?WT.mc_id=DT-MVP-5004822) method of the StoredContact instance. The API’s for the custom contact store are pretty straight forward and easy to use.

**Manifest**

The custom contact store requires the ID_CAP_CONTACTS capability, we should enable that in the WMAppManifest.xml file. In order to that, in the Visual Studio Solution Explorer, expand the project properties folder and double click the WMAppManifest.xml file. This will open the new UI editor for the manifest file. Go to the Capabilities tab and enable the ID_CAP_CONTACTS

[![id_cap_contacts](/assets/images/id_cap_contacts.png)]

Once the manifest file has been updated the app should be able to launch.

The user interface looks like this:

[![Custom Contact Store](/assets/images/custom-contact-store.png)

Once the contact is created it will be available in the People Hub

[![People Hub](/assets/images/people-hub.png)

When the contact is viewed from the People Hub the owner of the contact will be displayed on top

[![Custom Contact](/assets/images/custom-contact.png)

I hope you found this useful. You can check out the source code using the link below

<iframe height="120" src="https://skydrive.live.com/embed?cid=CA531E7FB4762C70&amp;resid=CA531E7FB4762C70%2136984&amp;authkey=AE6ctkxSOjg-Xgo" frameborder="0" width="98" scrolling="no"></iframe>