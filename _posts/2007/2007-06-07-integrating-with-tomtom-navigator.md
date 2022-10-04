---
layout: post
title: Integrating with TomTom Navigator
date: '2007-06-07T19:53:00.000+02:00'
author: Christian Resma Helle
tags:
- GPS
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2008-02-07T23:15:11.886+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6470922594105492551
blogger_orig_url: https://christian-helle.blogspot.com/2007/06/integrating-with-tomtom-navigator.html
---

PDA's are used for pretty much everything these days. From the bunch of devices the I work (play) with, I took a great liking to devices that have built in GPS receivers. These devices are usually bundled with really cool navigation software from various vendors. Some of these navigation software have SDK's that you can buy separately. By using these SDK's, you can fully integrate navigation features to your mobile solutions.  
  
In this article, I would like to discuss how to integrate a .NET Compact Framework application with TomTom Navigator. I will also demonstrate an example of making a generic navigator wrapper so your application is not just bound to one kind of navigation software.  
  
Before we get started, we need to have the TomTom Navigator SDK. Unfortunately this is not free, but can be easily purchased from the TomTom Pro website
  
Before we dig into more detail, let's go through our software requirements. We need the following:  
  
    1. Visual Studio 2005  
    2. Windows Mobile 5.0 SDK for Pocket PC  
    3. A device running Windows Mobile 5.0 with TomTom Navigator 5 installed  
    4. The TomTom Navigator SDK  
    5. ActiveSync 4.2 or higher (for Vista, the Mobile Device Center)  
  
  
Now, Lets get started...  
  
  
Here is what we need to make:  
  
    1. native wrapper for the TomTom SDK (native dll)  
    2. generic navigator wrapper in .NET CF  
    3. managed TomTom wrapper  
    4. device application that will call TomTom SDK wrapper methods  
  
Sounds pretty simple doesn't it?  
  
  
**1. Native Wrapper for the TomTom SDK**
  
We will first need a little help from native code to access the TomTom SDK. We cannot access the TomTom SDK directly from .NET due to the architecture of the SDK. We have to wrap around the TomTom SDK C++ classes and methods and expose them as C type functions.  
  
In your native wrapper, lets say we want to wrap the following TomTom SDK functions:  
  - GetApplicationVersion(TError* err, TVersion* ver)  
  - FlashMessage(TError* err, char* msg, int ms)  
  - NavigateToAddress(TError* aError, char* aCity, char* aStreet, char* aHouseNr, char* aPostcode)  
  
  
[C CODE]  

```c
#include "sdkconstants.h"  
#include "TomTomAPI.h"  
#include "TomTomGoFileLayer.h"  
  
#define CLIENT_NAME "client"  
  
CTomTomAPI::TError err;  
int res = 0;  
  
BOOL APIENTRY DllMain(  
  HANDLE hModule, DWORD ul_reason_for_call, LPVOID lpReserved )  
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
  
extern "C" __declspec(dllexport) int TTN_GetApplicationVersion(  
  int* iError, LPTSTR szVersion, int *iBuildNumber )  
{  
  MTomTomCommunicationLayerInterface *comms =  
    DEFAULT_TRANSPORTATION_LAYER(CLIENT_NAME,2005,TOMTOM_TCPIP_PORT);  
  
  CTomTomAPI api(*comms);  
  CTomTomAPI::TVersion version;  
  res = api.GetApplicationVersion(&err, &version);  
  *iError = err.iError;  
  
  TCHAR str[16];  
  _stprintf(str, TEXT("%S"), version.iVersion);  
  lstrcpy( szVersion, (LPTSTR)str );  
  *iBuildNumber = version.iBuildNumber;  
  
  delete comms;  
  return res;  
}  
  
extern "C" __declspec(dllexport) int TTN_FlashMessage(  
   int* iError, char* aMessage, int aMilliSeconds )  
{  
  char message[256];  
  sprintf(message, "%S", aMessage);  
  
  MTomTomCommunicationLayerInterface *comms =  
    DEFAULT_TRANSPORTATION_LAYER(CLIENT_NAME,2005,TOMTOM_TCPIP_PORT);  
  
  CTomTomAPI api(*comms);  
  res = api.FlashMessage(&err, message, aMilliSeconds);  
  *iError = err.iError;  
  
  delete comms;  
  return res;  
}  
  
extern "C" __declspec(dllexport) int TTN_NavigateToAddress(  
   int* iError, char* aCity, char* aStreet, char* aHouseNr, char* aPostcode )  
{  
  char city[256];  
  char street[256];  
  char houseNr[16];  
  char postcode[32];  
  
  sprintf(city, "%S", aCity);  
  sprintf(street, "%S", aStreet);  
  sprintf(houseNr, "%S", aHouseNr);  
  sprintf(postcode, "%S", aPostcode);  
  
  MTomTomCommunicationLayerInterface *comms =  
    DEFAULT_TRANSPORTATION_LAYER(CLIENT_NAME,2005,TOMTOM_TCPIP_PORT);  
  
  CTomTomAPI api(*comms);  
  res = api.NavigateToAddress(&err, city, street, houseNr, postcode);  
  *iError = err.iError;  
  
  delete comms;  
  return res;  
}  
```

Let's set the output of the project to be called **TTSDK.dll**  
  
**2. Generic Navigator Wrapper in .NET CF**  
  
Once we've gotten our native wrapper up and running, we create a generic navigator wrapper. We start off by creating a smart device class library project. Once the project is created, add the following classes: INavigator.cs, Navigator.cs, and Common.cs  
  
Lets go and define the common objects we want to use in Common.cs  
  
[C# CODE]  

```csharp
public struct NVersion
{  
  string Version;  
  int BuildNumber;  
}  
```

INavigator.cs will be an interface defining the how the wrapper will look like. Lets add methods for the 3 TomTom SDK methods we want to use.  
  
[C# CODE]  

```csharp  
public interface INavigator  
{  
  NVersion GetApplicationVersion();  
  void FlashMessage(string text, int duration);  
  void NavigateToAddress(string city, string street, string houseno, string zipcode);  
}  
```

Navigator.cs will be the class your application will call. This will load the managed TomTom wrapper as an instance of INavigator. Navigator itself will implement INavigator and will return calls from the TomTom wrapper.  
  
[C# CODE]  

```csharp  
public class Navigator : INavigator  
{  
  private INavigator instance;  
  
  public Navigator(string typeName)  
  {  
    Type type = Type.GetType(typeName);  
    if (type == null) {  
      throw new TypeLoadException();  
    } else {  
      instance = (INavigator)Activator.CreateInstance(type);  
      if (instance == null) {  
        throw new TypeLoadException();  
      }  
    }  
  }  
  
  public TVersion GetApplicationVersion()  
  {  
    return instance.GetApplicationVersion();  
  }  
  
  public void FlashMessage(string text, int duration)  
  {  
    instance.FlashMessage(text, duration);  
  }  
  
  public void NavigateToAddress(string city, string street, string houseno, string zipcode)  
  {  
    instance.NavigateToAddress(city, street, houseno, zipcode);  
  }  
}  
```

The default constructor for Navigator accepts a type name. The format for type name is `[Namespace].[ClassName], [AssemblyName]`
  
**3. Managed TomTom Wrapper**  
  
Here we create a new smart device class library. Once the project is created, add a reference to the generic navigator wrapper since we will implement the INavigator interface and add a class called TomTom.cs  
  
Lets implement TomTom.cs as INavigator  
  
[C# CODE]  

```csharp
[DllImport("TTSDK.dll", EntryPoint="TTN_GetApplicationVersion")]  
internal static extern int TTN_GetApplicationVersion(  
  ref int iError, StringBuilder szVersion, ref int iBuildNumber);  
  
[DllImport("TTSDK.dll", EntryPoint="TTN_FlashMessage")]  
internal static extern int TTN_FlashMessage(  
  ref int iError, string aMessage, int aMilliseconds);  
  
[DllImport("TTSDK.dll", EntryPoint="TTN_NavigateToAddress")]  
internal static extern int TTN_NavigateToAddress(  
  ref int iError, string aCity, string aStreet, string aHouseNo, string aPostcode);  
  
public void FlashMessage(string aMessage, int aMilliseconds)  
{  
  if (0 != TTN_FlashMessage(ref iError, aMessage, aMilliseconds)) {  
    throw new InvalidOperationException();  
  }  
}  
  
public NVersion GetApplicationVersion()  
{  
  NVersion version = new TVersion();  
  StringBuilder szVersion = new StringBuilder();  
  int iBuildNumber = 0;  
  int iError = 0;  

  if (0 != TTN_GetApplicationVersion(ref iError, szVersion, ref iBuildNumber)) {  
    throw new InvalidOperationException();  
  } else {  
    version.iVersion = szVersion.ToString();  
    version.iBuildNumber = iBuildNumber;  
  }  
}  
  
public void NavigateToAddress(string sCity, string sStreet, string sHouseNo, string sPostcode)  
{  
  int iError = 0;  
  
  if (0 != TTN_NavigateToAddress(ref iError, sCity, sStreet, sHouseNo, sPostcode)) {  
    throw new InvalidOperationException();  
  }  
}  
```

Now our TomTom wrapper is pretty much ready  
  
**4. Device Application**  
  
In our application, we want to integrate with TomTom Navigator for navigating to a specific address. The address can could be retrieved from a web service, or stored in your pocket outlook. For this article, we're going to retrieve address information of a customer from Pocket Outlook.  
  
In order to do this, we will need the Windows Mobile 5.0 SDK for Pocket PC to be installed. Let's start off by creating a Windows Mobile 5.0 device application project. Once the project is created, add a reference to the Navigator wrapper and the TomTom wrapper. Next we have to build the Native wrapper project, and add the output file TTSDK.dll to our project. Set TTSDK.dll to be "Copied if Newer". To retrieve address information from contacts, we must add a reference to Microsoft.WindowsMobile.PocketOutlook.dll.  
  
Once the references and files are in place, we can start adding some code to Form1.cs. No need to change the name of the main form since this is only a small demo. We need to have a control that can contain the contacts, lets use the ComboBox control for now. Add a ComboBox control to the form and call it _cbContacts_. Lets add a "Navigate to" button to the form as well and call it _btnNavigate_.  
  
To retrieve a list of contacts we need to create a global instance of `Microsoft.WindowsMobile.PocketOutlook.OutlookSession` and `Microsoft.WindowsMobile.PocketOutlook.ContactsCollection`, once we instantiate our `OutlookSession`, we can then retrieve a list of Contacts through `OutlookSession.Contacts.Items`
  
To communicate with TomTom, we create an instance of `Navigator`. The default constructor for Navigator will need a typeName for loading the TomTom wrapper as INavigator. It would be a smart idea to store the typeName in a seperate file, text or xml would be perfect. Once again, we do this so that if we want our application to integrate with different navigation software, we don't have to re-write everything. In this demo, the typeName will just be a hard coded string constant.  
  
[C# CODE]  

```csharp
private Microsoft.WindowsMobile.PocketOutlook.OutlookSession session;  
private Microsoft.WindowsMobile.PocketOutlook.ContactsCollection contacts;  
  
private const string TYPENAME="[The namespace].[The class name], [The assembly name]";  
private Navigator navigator;  
  
public Form1()  
{  
  InitializeComponent();  
  
  btnNavigate.Click += new EventHandler(btnNavigate_Click);  
  Closing += new CancelEventHandler(Form1_Closing);  
  
  navigator = new Navigator(TYPENAME);  
  
  string restriction = "[BusinessAddressStreet] <> " " OR [HomeAddressStreet] <> " "";  
  session = new OutlookSession();  
  contacts = session.Contacts.Items.Restrict(restriction);  
  cbContacts.DataSource = contacts;  
}  
  
private void Form1_Closing(object sender, CancelEventArgs e)  
{  
  contacts.Dispose();  
  contacts = null;  
  
  session.Dispose();  
  session = null;  
}  
  
private void btnNavigate_Click(object sender, EventArgs e)  
{  
  Contact contact = cbContacts.SelectedItem as Contact;  
  navigator.FlashMessage("Navigating...", 1500);  
  navigator.NavigateToAddress(contact.BusinessAddressCity,  
    contact.BusinessAddressStreet,  
    "PARSE THE HOUSE NUMBER...",  
    contact.BusinessAddressPostalCode);  
}  
```

When the application launches, your pocket outlook contacts that have a valid address will be loaded into our ComboBox control. Let's select an item in the ComboBox. Once you click on the Navigate button it will launch TomTom Navigator and display the message "Navigating" for 1.5 seconds, after that it will start calculating the route from your current location to your destination (in this case, the selected contact).  
  
That wasn't too hard was it?