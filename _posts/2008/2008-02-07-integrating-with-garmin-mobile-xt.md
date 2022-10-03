---
layout: post
title: Integrating with Garmin Mobile XT
date: '2008-02-07T22:15:00.119+01:00'
author: Christian Resma Helle
tags:
- GPS
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2008-02-17T15:10:18.324+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-4809292998115058207
blogger_orig_url: https://christian-helle.blogspot.com/2008/02/integrating-with-garmin-mobile-xt.html
---

Half a year ago, I wrote an article about [Integrating with TomTom Navigator](/2007/06/integrating-with-tomtom-navigator.html). This time I'm gonna discuss how you can integrate a .NET Compact Framework application with the Garmin Mobile XT navigation software. The process is a bit similar to integrating with TomTom because the Garmin SDK only provides a native API.  
  
Before we get started, we need to have the **Garmin Mobile XT for the Windows Mobile platform**. Unlike TomTom's SDK, Garmin's SDK is available free of charge for download.  
  
Before we can get more into detail, we will need the following:  
1) Visual Studio 2005 or 2008  
2) Windows Mobile 5.0 SDK for Pocket PC  
3) A windows mobile device a GPS receiver and the Garmin Mobile XT (and Maps)  
4) Garmin Mobile XT SDK for the Windows Mobile platform  
  
We will be making the same projects we made for [Integrating with TomTom Navigator](/2007/06/integrating-with-tomtom-navigator.html):  
1) Native wrapper for the Garmin XT SDK  
2) Managed Garmin XT SDK wrapper  
3) Device application that will call the Garmin XT SDK wrapper methods  
  
  
Let's get started...  
  
  
### Native wrapper for the Garmin XT SDK
  
The Garmin SDK ships with C++ header files and a static library that a native application can link to. For that reason we need to create a native DLL that exposes the methods that we need as C type funtions. Let's call this **Garmin.Native.dll**.  
  
In this article, we will implement a managed call to the Garmin Mobile XT to allow us to launch the Garmin Mobile XT, Navigate to a specific address or GPS coordinate, and to Show an address on the Map. These tasks will be performed on a native wrapper and which will be called from managed code.  
  
We will be using the following methods from the Garmin Mobile XT SDK:  
- QueLaunchApp  
- QueAPIOpen  
- QueAPIClose  
- QueCreatePointFromAddress  
- QueCreatePoint  
- QueRouteToPoint  
- QueViewPointOnMap  
  
These methods return specific error codes describing whether the command executed successfully or not. This error information is translated to a .NET Framework enum which we will see later. 

```c
#include "QueAPI.h"

#define EXPORTC extern "C" __declspec(dllexport)

long DecimalDegreesToSemicircles(double degrees);

BOOL APIENTRY DllMain(
  HANDLE hModule,
  DWORD ul_reason_for_call,
  LPVOID lpReserved)
{
  switch (ul_reason_for_call)
  {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
    break;
  }
  return TRUE;
}

static void QueCallback(QueNotificationT8 aNotification)
{
  // Used for debugging purposes
}

EXPORTC int CloseAPI()
{
  QueErrT16 err = QueAPIClose(QueCallback);
  return err;
}

EXPORTC int OpenNavigator()
{
  QueErrT16 err = QueLaunchApp(queAppMap);
  return err;
}

EXPORTC int NavigateToAddress(
  const wchar_t *streetAddress,
  const wchar_t *city,
  const wchar_t *postalCode,
  const wchar_t *state,
  const wchar_t *country)
{
  QueErrT16 err = QueAPIOpen(QueCallback);
  if (err != gpsErrNone) {
    return err;
  }

  QueSelectAddressType address;
  QuePointHandle point = queInvalidPointHandle;

  memset(&address, 0, sizeof(QueSelectAddressType));
  address.streetAddress = streetAddress;
  address.city = city;
  address.postalCode = postalCode;
  address.state = state;
  address.country = country;

  err = QueCreatePointFromAddress(&address, &point);
  if (err == queErrNone && point != queInvalidPointHandle) {
    err = QueRouteToPoint(point);
  }

  QueAPIClose(QueCallback);
  return err;
}

long DecimalDegreesToSemicircles(double degrees)
{
  return degrees * (0x80000000 / 180);
}

EXPORTC int NavigateToCoordinates(double latitude, double longitude)
{
  QueErrT16 err = QueAPIOpen(QueCallback);
  if (err != gpsErrNone) {
    return err;
  }

  QuePointType point;
  QuePositionDataType position;

  memset(&position, 0, sizeof(QuePositionDataType));
  position.lat = DecimalDegreesToSemicircles(latitude);
  position.lon = DecimalDegreesToSemicircles(longitude);

  memset(&point, 0, sizeof(QuePointType));
  point.posn = position;

  QuePointHandle hPoint;
  memset(&hPoint, 0, sizeof(QuePointHandle));

  err = QueCreatePoint(&point, &hPoint);
  if (err == queErrNone && hPoint != queInvalidPointHandle) {
    err = QueRouteToPoint(hPoint);
  }

  QueAPIClose(QueCallback);
  return err;
}

EXPORTC int ShowAddressOnMap(
  const wchar_t *streetAddress,
  const wchar_t *city,
  const wchar_t *postalCode,
  const wchar_t *state,
  const wchar_t *country)
{
  QueErrT16 err = QueAPIOpen(QueCallback);
  if (err != gpsErrNone) {
    return err;
  }

  QueSelectAddressType address;
  QuePointHandle point = queInvalidPointHandle;

  memset(&address, 0, sizeof(QueSelectAddressType));
  address.streetAddress = streetAddress;
  address.city = city;
  address.postalCode = postalCode;
  address.state = state;
  address.country = country;

  err = QueCreatePointFromAddress(&address, &point);
  if (err == queErrNone && point != queInvalidPointHandle) {
    err = QueViewPointOnMap(point);
  }

  QueAPIClose(QueCallback);
  return err;
}
```

The Garmin Mobile XT SDK works in a straight forward way. Before making any calls to the API, you first need to Open it. Once open, you can start executing a series of methods and then once you're done you must Close the API. The Garmin Mobile XT has to be running before you can execute commands to it, otherwise you will get a communication error.  
  
You might notice in the code above an empty static method called QueCallback(QueNotificationT8 aNotification). This is a callback method that receives the information about the application state. You can use this for making callbacks from native to managed code. You can pass a delegate method from managed code to the native methods that expect QueNotificationCallback as a parameter. We will only use it for debugging purposes in this example. We will not dig more into that in this article.  
  
Normally when reverse geocoding an address to a GPS coordinates using some free service, you will get the coordinates in decimal degrees (WGS84 decimal format). Navigating to a coordinate using the Garmin Mobile XT SDK requires the coordinates to be in semicircles (2^31 semicircles equals 180 degrees).  
  
To convert decimal degrees to semicircles we use the following formula:
```
semi-circles = decimal degrees * (2^31 / 180)
```

### Managed wrapper
  
In my article [Integrating with TomTom Navigator](/2007/06/integrating-with-tomtom-navigator.html), I created a Generic Navigator wrapper that uses the INavigator interface for defining methods to be used by the managed wrapper. The purpose of the Generic Navigator was to allow the application to integrate with several navigation solutions without changing any of the existing code. As I already discussed this in the past, I will skip this part and only focus on how to integrate with Garmin Mobile XT.  
  
We first need to create an enumeration containing error codes we receive from the native wrapper.  

```csharp
public enum GarminErrorCodes : int
{
  None = 0,
  NotOpen = 1,
  InvalidParameter,
  OutOfMemory,
  NoData,
  AlreadyOpen,
  InvalidVersion,
  CommunicationError,
  CmndUnavailable,
  LibraryStillOpen,
  GeneralFailure,
  Cancelled,
  RelaunchNeeded
}
```

We of course need to create our P/Invoke declarations. This time let's put them in an internal class called NativeMethods()

```csharp
internal class NativeMethods
{
  [DllImport("Garmin.Native.dll")]
  internal static extern int CloseAPI();

  [DllImport("Garmin.Native.dll")]
  internal static extern int OpenNavigator();

  [DllImport("Garmin.Native.dll")]
  internal static extern int NavigateToAddress(
    string address,
    string city,
    string postalcode,
    string state,
    string country);

  [DllImport("Garmin.Native.dll")]
  internal static extern int NavigateToCoordinates(
    double latitude,
    double longitude);

  [DllImport("Garmin.Native.dll")]
  internal static extern int ShowAddressOnMap(
    string address,
    string city,
    string postalcode,
    string state,
    string country);
}
```

Let's create a .NET exception that we can throw which contains native error details when a native method call fails. Let's call it GarminNativeException()

```csharp
[Serializable]
public class GarminNativeException : Exception
{
  public GarminNativeException() { }

  public GarminNativeException(GarminErrorCodes native_error) { }

  public GarminNativeException(
    string message,
    GarminErrorCodes native_error) : base(message) { }
}
```

Now we need a class that we can use for calling the wrapped managed methods to the Garmin mobile XT. Let's call it GarminXT()

```csharp
public class GarminXT : IDisposable
{
  public void Dispose()
  {
    NativeMethods.CloseAPI();
  }

  public void OpenNavigator()
  {
    GarminErrorCodes err = (GarminErrorCodes)NativeMethods.OpenNavigator();

    if (err != GarminErrorCodes.None) {
      ThrowGarminException(err);
    }
  }

  public void NavigateToAddress(
    string address,
    string city,
    string postalcode,
    string state,
    string country)
  {
    GarminErrorCodes err = (GarminErrorCodes)NativeMethods.NavigateToAddress(
      address,
      city,
      postalcode,
      state,
      country);

    if (err != GarminErrorCodes.None) {
      ThrowGarminException(err);
    }
  }

  public void NavigateToCoordinates(double latitude, double longitude)
  {
    GarminErrorCodes err = (GarminErrorCodes)NativeMethods.NavigateToCoordinates(
      latitude,
      longitude);

    if (err != GarminErrorCodes.None) {
      ThrowGarminException(err);
    }
  }

  public void ShowAddressOnMap(
    string address,
    string city,
    string postalcode,
    string state,
    string country)
  {
    GarminErrorCodes err = (GarminErrorCodes)NativeMethods.ShowAddressOnMap(
      address,
      city,
      postalcode,
      state,
      country);

    if (err != GarminErrorCodes.None) {
      ThrowGarminException(err);
    }
  }

  private GarminNativeException ThrowGarminException(GarminErrorCodes err)
  {
    string message = string.Empty;

    switch (err) {
      case GarminErrorCodes.NotOpen:
        message = "Close() called without having Open() first";
        break;
      case GarminErrorCodes.InvalidParameter:
        message = "Invalid parameter was passed to the function";
        break;
      case GarminErrorCodes.OutOfMemory:
        message = "Out of Memory";
        break;
      case GarminErrorCodes.NoData:
        message = "No Data Available";
        break;
      case GarminErrorCodes.AlreadyOpen:
        message = "The API is already open";
        break;
      case GarminErrorCodes.InvalidVersion:
        message = "The API is an incompatible version";
        break;
      case GarminErrorCodes.CommunicationError:
        message = "There was an error communicating with the API";
        break;
      case GarminErrorCodes.CmndUnavailable:
        message = "Command is unavailable";
        break;
      case GarminErrorCodes.LibraryStillOpen:
        message = "API is still open";
        break;
      case GarminErrorCodes.GeneralFailure:
        message = "General Failure";
        break;
      case GarminErrorCodes.Cancelled:
        message = "Action was cancelled by the user";
        break;
      case GarminErrorCodes.RelaunchNeeded:
        message = "Relaunch needed to load the libraries";
        break;
      default:
        break;
    }

    throw new GarminNativeException(message, err);
  }
}
```

The managed wrapper GarminXT() implements IDisposible for ensuring that the API will be closed when the GarminXT object gets disposed. I check the return code of every method to verify if the native method call succeeded or failed. If the native method call failed then I throw a GarminNativeException containing a text description of the error and the GarminErrorCode returned by the native method call.

### Using the Managed Wrapper

Now that we have a managed wrapper for the Garmin Mobile XT SDK we can start testing it with a simple smart device application. Let's say that we created a simple application that accepts street address, city, postal code, country, latitude, longitude. We also have some buttons or menu items for: Navigating to an address, Navigating to coordinates, Showing an address on the map, and for launching Garmin Mobile XT.

Since the managed wrapper implements IDisposable, we surround our calls to it with the using statement:

```csharp
using (GarminXT xt = new GarminXT()) {
  xt.OpenNavigator();
}
```

As I mentioned before, it is important that Garmin Mobile XT is running in the background for executing certain commands. Otherwise the managed Garmin XT wrapper will throw a GarminNativeException saying that there was an error communicating with the API. I would suggest handling the GarminNativeException everytime calls to the managed wrapper are made.

For launching Garmin Mobile XT:

```csharp
try {
  using (GarminXT xt = new GarminXT()) {
    xt.OpenNavigator();
  }
}
catch (GarminNativeException ex) {
  Debug.Assert(false, ex.Message, ex.StackTrace);
}
```

For navigating to an address:

```csharp
try {
  using (GarminXT xt = new GarminXT()) {
    xt.NavigateToAddress(
      "Hørkær 24",
      "Herlev",
      "2730",
      null,
      "Denmark");
  }
}
catch (GarminNativeException ex) {
  Debug.Assert(false, ex.Message, ex.StackTrace);
}
```

For navigating to coordinates:

```csharp
try {
  using (GarminXT xt = new GarminXT()) {
    xt.NavigateToCoordinates(
      55.43019,
      12.26075);
  }
}
catch (GarminNativeException ex) {
  Debug.Assert(false, ex.Message, ex.StackTrace);
}
```

For showing an address on the map:

```csharp
try {
  using (GarminXT xt = new GarminXT()) {
    xt.ShowAddressOnMap(
      "Hørkær 24",
      "Herlev",
      "2730",
      null,
      "Denmark");
  }
}
catch (GarminNativeException ex) {
  Debug.Assert(false, ex.Message, ex.StackTrace);
}
```

That wasn't that hard was it?

But there is one thing that I don't quite understand. Why do we have to wrap SDK's like this ourselves? Why don't they just provide managed SDK's? Hopefully this will change in the near future. Until then, I guess I can just write a few more articles about it.

If you're interested in the full source code then you can grab it [here](/assets/samples/GarminXT.zip).