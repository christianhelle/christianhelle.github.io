---
layout: post
title: Generate iOS InfoPlist.strings Translations from Google Sheets
date: '2019-06-18T23:21:00.001+02:00'
author: Christian Helle
tags: iOS Localization Xamarin
modified_time: '2019-06-20T16:55:49.393+02:00'
thumbnail: https://1.bp.blogspot.com/-b3CPRXAItU4/XQlNzCtdiaI/AAAAAAAAQVw/QGee8D4WHkMt4QxvvJGqAKRG0BPUxgktQCLcBGAs/s72-c/infoplist-google-sheets.png
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6206453854170211664
blogger_orig_url: https://christian-helle.blogspot.com/2019/06/generate-ios-infopliststrings.html
---

In my previous article [Generating ResX translations from Google Sheets](/2019/06/generate-resx-translations-using-google.html), I wrote about using Google Sheets as a translation tool by using the [GOOGLETRANSLATE](https://support.google.com/docs/answer/3093331?hl=en) built in function to generate translation files for a **Xamarin.Forms** solution. For this post, I will demonstrate something very similar, but instead of ResX files I'll generate **InfoPlist.strings** files in iOS for localizing the permission request prompts for accessing things like Camera, Location, Photo Gallery, etc. For the sake of this article I created [this sample Google Sheets](https://docs.google.com/spreadsheets/d/125id155PUq-6Odwg8Nf9fmkgBsKahTGbJYaYBD2rpSg)  

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

Here’s an example usage of the tool  

    csvtrans --sheet 125id155PUq-6Odwg8Nf9fmkgBsKahTGbJYaYBD2rpSg iOS --format apple --outputdir .\Resources --name InfoPlist

The first argument `**–-sheet**` is the Google Sheet document ID followed by the Sheet Name, the next argument `**–-format**` specifies the output file format, the argument `**–-outputdir**` specifies the output folder, and the last argument `**--name**` specifies the output filename.  

You can get the Document ID from the URL of the Google Sheet  

![](/assets/images/infoplist-google-sheets.png)  

Here's an example output  

![](/assets/images/ios-infoplist-console.png)  

Now I can just bring these files into my project and use them directly. Well, almost! There's one little problem, and that is that by default the **Xamarin.iOS** csproj tooling explicitly adds each **InfoPlist.strings** file as a **BundleResource**. Oddly enough, the csproj format allows to specify wild card folders, so if we want to enable dynamic generation of InfoPlist.strings translations then we need to manually edit the csproj.  

This is actually very easy to do. We just need to replace the lines like  

![](/assets/images/ios-csproj-before.png)

with  

![](/assets/images/ios-csproj-after.png)

This opens up for dynamic translations at build time using your CI/CD build tools of choice