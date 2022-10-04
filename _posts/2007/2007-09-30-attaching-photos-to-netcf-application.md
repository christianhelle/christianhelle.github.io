---
layout: post
title: Attaching Photos to a .NETCF Application
date: '2007-09-30T14:01:00.000+02:00'
author: Christian Resma Helle
tags:
- ".NET Compact Framework"
modified_time: '2007-10-01T09:15:58.471+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-3879352136016765784
blogger_orig_url: https://christian-helle.blogspot.com/2007/09/attaching-photos-to-netcf-application.html
---

Sorry for not posting any articles lately. I've been quite busy and didn't have time to follow up on blogs.

Anyway, lately I've been working on a Mobile Inspection Log application for a customer. The application is rather simple and works like a small check list. The back-end server provides the mobile client with a list of Observation types where the User can set each observation to "OK", "Questionable", "Fail", and "N/A". If an Observation is questionable or failed then the user can attach a comment and a photo. The photo is then saved to a local database which can be synchronized with an access database on the users desktop computer or be sent to a web service.

The user has 2 options for attaching photos to their observation: 1) Opening an existing photo on the file system; 2) taking a photo themselves using the built-in camera. Since the one of the requirements for the application is Windows Mobile 6.0 Professional, my job got a lot easier with the Microsoft.WindowsMobile.Forms.CameraCaptureDialog and the Microsoft.WindowsMobile.Forms.SelectPictreDialog forms

Here's a small sample of how to use the CameraCaptureDialog:

```csharp
string image_filename = null;
using (CameraCaptureDialog camera = new CameraCaptureDialog()) 
{
    camera.Owner = this;
    camera.Title = base.Text;
    camera.Mode = CameraCaptureMode.Still;
    camera.StillQuality = CameraCaptureStillQuality.High;

    if (camera.ShowDialog() == DialogResult.OK)
    {
        image_filename = camera.FileName;
    }
}
```

And for the SelectPictureDialog:

```csharp
string image_filename = null;
using (SelectPictureDialog open = new SelectPictureDialog()) 
{
    open.Filter = "Pictures (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
    if (open.ShowDialog() == DialogResult.OK) 
    {
        image_filename = open.FileName;
    }
}
```

For saving to the local database I simply convert an Image to byte[]. I do this by:

```csharp
public static byte[] ImageToByteArray(Image image, ImageFormat format)
{
    MemoryStream stream = new MemoryStream();
    image.Save(stream, format);

    byte[] buffer = stream.GetBuffer();

    stream.Close();
    stream = null;

    return buffer;
}
```

or

```csharp
public static byte[] ImageToByteArray(string image_file)
{
    FileStream stream = new FileStream(image_file, FileMode.Open);
    int size = (int)stream.Length;
    byte[] buffer = new byte[size];
    stream.Read(buffer, 0, size);

    return buffer;
}
```

For loading image from the database to the application I simply convert the byte[] to an Image. I do that by:

```csharp
public static Image ByteArrayToImage(byte[] raw_data) 
{
    MemoryStream stream = new MemoryStream(raw_data.Length);
    stream.Write(raw_data, 0, raw_data.Length - 1);

    Bitmap image = new Bitmap(stream);

    stream.Close();
    stream = null;

    return image;
}
```

Since each inspection report will contain around 40-50 observation, it can also take around 40-50 images. For synchronizing the application via web services, I had to split up the inspection report in parts, 1 part will contain all the textual information, and all the other parts will be for the photos. The Mobile Client Software Factory made life easier for having this architecture run on an occasionally connected environment.

Other things that are keeping me busy is integrating mobile applications with popular out-of-the-box ERP systems, such as Navision, Axapta, and Visma. I'll post more info and how-to's once I get more familiar with these systems.