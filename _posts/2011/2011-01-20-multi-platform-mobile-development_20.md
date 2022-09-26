---
layout: post
title: Multi-platform Mobile Development - Sending SMS
date: '2011-01-20T16:28:00.000+01:00'
author: Christian Resma Helle
tags:
- Multi-platform Mobile Development
- Windows Phone 7
- Android
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2011-01-20T16:28:14.655+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-2178191598738103131
blogger_orig_url: https://christian-helle.blogspot.com/2011/01/multi-platform-mobile-development_20.html
---

This is a task that pretty much every mobile device can do, or at least any mobile phone can do. In this short post on multi-platform mobile development I would like to demonstrate how to send an SMS or launch the SMS compose window in a mobile application.

I'm going to demonstrate how to use the messaging API's of the following platforms:
- Android
- Windows Phone 7
- Windows Mobile 5.0 (and higher) using .NET Compact Framework
- Windows Mobile using the Platform SDK (Native code)

## Android

There are 2 ways of sending SMS from an Android application: Launching the Compose SMS window; Through the SmsManager API. I figured that since this article is supposed to demonstrate as many ways as possible for sending SMS that I create a helper class containing methods that I think would be useful or at least convenient to have.

Here's a SMS helper class for Android that I hope you would find useful.

```java
package com.christianhelle.android.samples;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.Uri;
import android.telephony.SmsManager;
import android.widget.Toast;

/**
* Helper class for sending SMS messages
*
* @author Christian Resma Helle
*/
public class Sms {
    private final static String SENT_ACTION = "SENT";
    private final static String DELIVERED_ACTION = "DELIVERED";
    private Context context;
    private PendingIntent sentIntent;
    private PendingIntent deliveredIntent;

    /**
     * Creates an instance of the SMS class
     *
     * @param context     Context that owns displayed notifications
     */
    public Sms(Context context) {
        this.context = context;
        registerForNotification();
    }

    private void registerForNotification() {
        sentIntent = PendingIntent.getBroadcast(context, 0, new Intent(SENT_ACTION), 0);
        deliveredIntent = PendingIntent.getBroadcast(context, 0, new Intent(DELIVERED_ACTION), 0);

        context.registerReceiver(messageSentReceiver, new IntentFilter(SENT_ACTION));
        context.registerReceiver(messageDeliveredReceiver, new IntentFilter(DELIVERED_ACTION));
    }
    
    protected void finalize() throws Throwable {
        context.unregisterReceiver(messageSentReceiver);
        context.unregisterReceiver(messageDeliveredReceiver);
    }

    /**
     * Opens the Compose SMS application with the recipient phone number displayed
     *
     * @param phoneNumber     recipient phone number of the SMS
     */
    public void composeMessage(String phoneNumber) {
        Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse("sms:" + phoneNumber));
        context.startActivity(intent);
    }

    /**
     * Opens the Compose SMS application with the recipient phone number and message displayed
     *
     * @param phoneNumber     recipient phone number of the SMS
     * @param text             message body
     */
    public void composeMessage(String phoneNumber, String text) {
        Intent intent = new Intent(Intent.ACTION_VIEW);
        intent.putExtra("sms_body", text);
        intent.putExtra("address", phoneNumber);
        intent.setType("vnd.android-dir/mms-sms");
        context.startActivity(intent);
    }

    /**
     * Opens the Compose SMS application with the multiple recipient phone numbers and the message displayed
     *
     * @param phoneNumber     recipient phone numbers of the SMS
     * @param text             message body
     */
    public void composeMessage(String[] phoneNumbers, String text) {
        StringBuilder sb = new StringBuilder();
        for (String string : phoneNumbers) {
            sb.append(string);
            sb.append(";");
        }
        composeMessage(sb.toString(), text);
    }

    /**
     * Send an SMS to the specified number
     *
     * @param phoneNumber     recipient phone number of the SMS
     * @param text             message body
     */
    public void sendMessage(String phoneNumber, String text) {
        sendMessage(phoneNumber, text, false);
    }

    /**
     * Send an SMS to the specified number and display a notification on the message status
     * if the notifyStatus parameter is set to <b>true</b>
     *
     * @param phoneNumber     recipient phone number of the SMS
     * @param text             message body
     * @param notifyStatus     set to <b>true</b> to display a notification on the screen
     *                         if the message was sent and delivered properly, otherwise <b>false</b>
     */
    public void sendMessage(String phoneNumber, String text, boolean notifyStatus) {
        SmsManager sms = SmsManager.getDefault();
        if (notifyStatus) {
            sms.sendTextMessage(phoneNumber, null, text, sentIntent, deliveredIntent);   
        } else {
            sms.sendTextMessage(phoneNumber, null, text, null, null);
        }   
    }

    /**
     * Send an SMS to multiple recipients and display
     *
     * @param phoneNumber     recipient phone number of the SMS
     * @param text             message body
     */
    public void sendMessage(String[] phoneNumbers, String text) {
        sendMessage(phoneNumbers, text, false);
    }

    /**
     * Send an SMS to multiple recipients and display a notification
     * on the message status if notifyStatus is set to <b>true</b>
     *
     * @param phoneNumber     recipient phone number of the SMS
     * @param text             message body
     * @param notifyStatus     set to <b>true</b> to display a notification on the screen
     *                         if the message was sent and delivered properly, otherwise <b>false</b>
     */
    public void sendMessage(String[] phoneNumbers, String text, boolean notifyStatus) {
        StringBuilder sb = new StringBuilder();
        for (String string : phoneNumbers) {
            sb.append(string);
            sb.append(";");
        }
        sendMessage(sb.toString(), text, notifyStatus);
    }

    private BroadcastReceiver messageSentReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            switch (getResultCode()) {
            case Activity.RESULT_OK:
                Toast.makeText(context, "SMS sent", Toast.LENGTH_SHORT).show();
                break;
            case SmsManager.RESULT_ERROR_GENERIC_FAILURE:
                Toast.makeText(context, "Generic failure", Toast.LENGTH_SHORT).show();
                break;
            case SmsManager.RESULT_ERROR_NO_SERVICE:
                Toast.makeText(context, "No service", Toast.LENGTH_SHORT).show();
                break;
            case SmsManager.RESULT_ERROR_NULL_PDU:
                Toast.makeText(context, "Null PDU", Toast.LENGTH_SHORT).show();
                break;
            case SmsManager.RESULT_ERROR_RADIO_OFF:
                Toast.makeText(context, "Radio off", Toast.LENGTH_SHORT).show();
                break;
            }
        }
    };

    private BroadcastReceiver messageDeliveredReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            switch (getResultCode()) {
            case Activity.RESULT_OK:
                Toast.makeText(context, "Message delivered", Toast.LENGTH_SHORT).show();
                break;
            case Activity.RESULT_CANCELED:
                Toast.makeText(context, "Message not delivered", Toast.LENGTH_SHORT).show();
                break;
            }
        }
    };
}
```

Before your Android application can send SMS it needs the right permissions for it. Add the SEND_SMS permission your AndroidManifest.xml

```xml
<uses-permission android:name="android.permission.SEND_SMS"></uses-permission>
```

Here are some examples on how to use the SMS helper class defined above from within an Activity class:

```java
Sms sms = new Sms(getApplicationContext());

// Send an SMS to the specified number
sms.sendMessage("+4512345678", "Multi-platform Mobile Development");

// Send an SMS to the specified number and display a notification on the message status
sms.sendMessage("+4512345678", "Multi-platform Mobile Development", true);

// Send an SMS to multiple recipients
sms.sendMessage(new String[] { "+4512345678", "+4598765432" }, "Multi-platform Mobile Development");       

// Send an SMS to multiple recipients and display a notification on the message status
sms.sendMessage(new String[] { "+4512345678", "+4598765432" }, "Multi-platform Mobile Development", true);

// Opens the Compose SMS application with the recipient phone number displayed
sms.composeMessage("+4512345678");

// Opens the Compose SMS application with the recipient phone number and message displayed
sms.composeMessage("+4512345678", "Multi-platform Mobile Development");

// Opens the Compose SMS application with the multiple recipient phone numbers and the message displayed
sms.composeMessage(new String[] { "+4512345678", "+4598765432" }, "Multi-platform Mobile Development");
```

## Windows Phone 7

This platform unfortunately doesn't provide as vast a API collection compared to Android and Windows Mobile. To send an SMS in Windows Phone 7, you will have to use the SMS Compose page in the built-in messaging application. To launch this we call the Show() method in SmsComposeTask.

Here's how to use SmsComposeTask

```csharp
SmsComposeTask launcher = new SmsComposeTask();
launcher.To = "+45 12 34 56 78";
launcher.Body = "Multi-platform Mobile Development";
launcher.Show();
```

## Windows Mobile 5.0 (and higher) using .NET Compact Framework

Sending an SMS in this platform is just as easy as doing so in Windows Phone 7. Windows Mobile provides native API's for the Short Messaging System, these methods are exposed as C type methods in a DLL called sms.dll. Aside from the SMS API, the platform also offers another API called the Messaging API (CE MAPI) for sending SMS, MMS, and Emails. Microsoft has provided managed wrappers for these and many other API's to make the life of the managed code developer a lot easier.

To send an SMS in Windows Mobile 5.0 (and higher) we use the SmsMessage object. There are 2 ways of accomplishing this: Using the Send() method of the SmsMessage class; Sending the SMS using the Compose SMS application

Here's a snippet on how to send SMS using the `Send()` method

```csharp
SmsMessage sms = new SmsMessage("+45 12 34 56 78", "Multi-platform Mobile Development");
sms.Send();
```

Here's a snippet on how to send SMS using the Compose SMS application

```csharp
SmsMessage sms = new SmsMessage("+45 12 34 56 78", "Multi-platform Mobile Development");
MessagingApplication.DisplayComposeForm(sms);
```

The code above depends on 2 assemblies that must be referenced to the project:
- Microsoft.WindowsMobile.dll
- Microsoft.WindowsMobile.PocketOutlook.dll

It is also possible to P/Invoke the SMS API through sms.dll, but this requires a slightly more complicated solution. In the next section, I will demonstrate how use the SMS API in native code. This should give you an idea on how to use the SMS API if you would like to go try the P/Invoke approach.

## Windows Mobile using the Platform SDK (Native code)

Probably not very relevant for most modern day managed code developers but just to demonstrate as many ways to send SMS in as many platforms as possible I'd like to show how to send SMS in native code using the Windows CE Short Message Service (SMS) API.

Here's a sample C++ helper class for sending SMS using the Platform SDK

```c++
#include "stdafx.h"
#include "sms.h"
#include <string>
 
class SmsMessage 
{
private:
    std::wstring recipient;
    std::wstring message;
 
public:
    SmsMessage(const wchar_t* phoneNumber, const wchar_t* text) 
    {
        recipient = phoneNumber;
        message = text;
    }
 
    void Send() 
    {
        SMS_HANDLE smshHandle;
        HRESULT hr = SmsOpen(SMS_MSGTYPE_TEXT, SMS_MODE_SEND, &smshHandle, NULL);
        if (hr != S_OK)
            return;
 
        SMS_ADDRESS smsaDestination;
        memset (&smsaDestination, 0, sizeof (smsaDestination));
        smsaDestination.smsatAddressType = SMSAT_INTERNATIONAL;
        lstrcpy(smsaDestination.ptsAddress, recipient.c_str());
 
        TEXT_PROVIDER_SPECIFIC_DATA tpsd;
        tpsd.dwMessageOptions = PS_MESSAGE_OPTION_NONE;
        tpsd.psMessageClass = PS_MESSAGE_CLASS1;
        tpsd.psReplaceOption = PSRO_NONE;
 
        SMS_MESSAGE_ID smsmidMessageID = 0;
        hr = SmsSendMessage(smshHandle, 
                            NULL, 
                            &smsaDestination, 
                            NULL,
                            (PBYTE) message.c_str(), 
                            (message.length() + 1) * sizeof(wchar_t), 
                            (PBYTE) &tpsd, 
                            sizeof(TEXT_PROVIDER_SPECIFIC_DATA), 
                            SMSDE_OPTIMAL, 
                            SMS_OPTION_DELIVERY_NONE, 
                            &smsmidMessageID);
 
        SmsClose (smshHandle);
    }
};
```

The code above requires the that the project is linked with sms.lib, otherwise you won't be able to build.

Here's a snippet of how to use the SMS helper class defined above:

```c++
SmsMessage *sms = new SmsMessage(L"+14250010001", L"Multi-platform Mobile Development");
sms->Send();
delete sms;
```

For those who don't know what +14250010001 is, this is the phone number of the Windows Mobile emulator. For testing SMS functionality on the emulator, you can use this phone number.

That's it for now. I hope you found this article interesting.