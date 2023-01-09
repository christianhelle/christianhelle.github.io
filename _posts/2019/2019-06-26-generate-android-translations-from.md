---
layout: post
title: Generate Android Translations from Google Sheets
date: '2019-06-26T22:30:00.001+02:00'
author: Christian Helle
tags: 
- Localization 
- Xamarin 
- Android
modified_time: '2019-06-26T22:35:34.121+02:00'
thumbnail: https://1.bp.blogspot.com/-Xe0MspUwnQA/XRPOeMGmaMI/AAAAAAAAQWo/xmAfPa_eMgkS--gdCl-7mHkm3VHy3GNiwCLcBGAs/s72-c/sheets-android.png
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-1248680433354444596
blogger_orig_url: https://christian-helle.blogspot.com/2019/06/generate-android-translations-from.html
redirect_from:
    - /blog/2019/07/02/generate-android-translations-from-google-sheets/
    - /2019/06/generate-android-translations-from-google-sheets/
    - /2019/06/generate-android-translations-from-google-sheets
    - /2019/generate-android-translations-from-google-sheets/
    - /2019/generate-android-translations-from-google-sheets
    - /generate-android-translations-from-google-sheets/
    - /generate-android-translations-from-google-sheets
    - /2019/06/generate-android-translations-from/
    - /2019/06/generate-android-translations-from
    - /2019/generate-android-translations-from/
    - /2019/generate-android-translations-from
    - /generate-android-translations-from/
    - /generate-android-translations-from
---

In previous articles [Generating ResX translations from Google Sheets](/2019/06/generate-resx-translations-using-google.html) and [Generate iOS InfoPlist.strings Translations from Google Sheets](/2019/06/generate-ios-infopliststrings.html), I wrote about using Google Sheets as a translation tool by using the [GOOGLETRANSLATE](https://support.google.com/docs/answer/3093331?hl=en) built in function to generate translation files for a **Xamarin** based solution. For this post, I will demonstrate something very similar, but instead of ResX files or InfoPlist.strings, I'll generate **strings.xml** files for Android. For the sake of this article I created [this sample Google Sheets](https://docs.google.com/spreadsheets/d/1mrMkhItrIDsPwEKMlR8JJ3Pgj1K6zUv0AhmBT4jWRqs/edit#gid=0)  

For a quick recap, we will use a tool called [csvtrans](https://github.com/rickykaare/csvtrans) written by my colleague and good friend, [Ricky Kaare Engelharth](https://twitter.com/rickykaare). The tool is built with .NET Core and can be installed using this command  

    dotnet tool install -g csvtrans

Using the tool is also straight forward and it also comes with some quick start instructions  

    USAGE: csvtrans [--help] [--sheet <document id> <sheet name>]
                [--csv <url or path>] [--format <apple|android|resx>]
                [--outputdir <directory path>] [--name <string>]
                [--convert-placeholders <regex pattern>]

    OPTIONS:

        --sheet, -s <document id> <sheet name>
                            specify a Google Sheet as input.
        --csv, -c <url or path>
                            specify a online or local cvs file as input.
        --format, -f <apple|android|resx>
                            specify the output format.
        --outputdir, -o <directory path>
                            specify the output directory.
        --name, -n <string>   specify an optional name for the output.
        --convert-placeholders, -p <regex pattern>
                            convert placeholders to match the output format.
        --help                display this list of options.

Here's an example usage of tool  

    csvtrans --sheet 1mrMkhItrIDsPwEKMlR8JJ3Pgj1K6zUv0AhmBT4jWRqs Android --format android --outputdir .\Resources\

The first argument `**–-sheet**` is the Google Sheet document ID followed by the Sheet Name, the next argument `**–-format**` specifies the output file format, and the last argument `**–-outputdir**` specifies the output folder  

You can get the Document ID from the URL of the Google Sheet  

![](/assets/images/sheets-android.png)  

Here's an example output  

![](/assets/images/csvtrans-android.png)  

Now I can just bring these files into my project and use them directly. Well, almost! There's one little problem, and that is that by default the **Xamarin.Android** csproj tooling explicitly adds each **strings.xml** file as an **AndroidResource**. Oddly enough, the csproj format allows to specify wild card folders, so if we want to enable dynamic generation of **values/strings.xml** translations then we need to manually edit the csproj.  

This is actually very easy to do. We just need to replace the lines like  

![](/assets/images/android-csproj-before.png)  

with  

![](/assets/images/android-csproj-after.png)  

This opens up for dynamic translations at build time using your CI/CD build tools of choice