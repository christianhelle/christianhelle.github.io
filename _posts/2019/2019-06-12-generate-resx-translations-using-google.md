---
layout: post
title: Generate Resx Translations from Google Sheets
date: '2019-06-12T22:30:00.002+02:00'
author: Christian Resma Helle
tags: 
- Localization 
- Xamarin
modified_time: '2019-06-13T09:28:50.488+02:00'
thumbnail: https://1.bp.blogspot.com/-lldRsH8dOVk/XQFZ1BrC4aI/AAAAAAAAWL4/awcO8fBQ_5UuoCVeLl9ELkta7R051iCpwCEwYBhgL/s72-c/GoogleSheets.png
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-4089591479727741453
blogger_orig_url: https://christian-helle.blogspot.com/2019/06/generate-resx-translations-using-google.html
---

In my career, I have tried multiple translation tools for handling localization. This usually ends up in a spreadsheet sent back and forth that gets imported/exported with the actual translation tool. I have also tried giving my translators and customers direct access to the translation tool but that never really worked as they tend to blindly translate everything they see and usually miss out on the fact that some strings contain important placeholders that executable code expects. Anyway, at the end of the sending a spreadsheet back and forth seems to always work.  

In a recent project, I built an Android and iOS app with Xamarin.Forms that used Resx files for handling cross platform translations, and InfoPlist.strings files in iOS for localizing OS requirement prompts for using things like Camera, Localization, Photos, etc. For this project we thought about playing around with Google Sheets as a translation tool. Google Sheets has built in Google Translate support so you can do something like **=GOOGLETRANSLATE($B2,$B$1,C$1)** where **$B2** describes the text to translate, **$B1** describes the source language, in this case English is the default, and **$C1** describes the language to translate to. With this approach, I can very easily, blindly, add new translations to my app, like in [this sample](http://docs.google.com/spreadsheets/d/1icJ0a48MIIRkbHSIbPyLNXsbTZcPKI_U80QwdX5pWf8) Google Sheets document, where I added Danish, German, Filipino, Simplified Chinese, Japanese, and Koreanusing the Google Translate tool. Of course, this needs to be proof-read by a translation professional who mastered the language, but this approach is very convenient for checking out how the app looks like in different languages.  

Now here’s the awesome part. My colleague and good friend, [Ricky Kaare Engelharth](https://twitter.com/rickykaare), created a translation tool called [csvtrans](https://github.com/rickykaare/csvtrans) that can produce Resx, iOS, and Android translation files from a publicly available Google Sheets document. The tool is written in .NET Core and is publicly available from [nuget.org](https://www.nuget.org/packages/csvtrans) as a tool.  

The tool can be installed using this command  

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

    csvtrans --sheet 1icJ0a48MIIRkbHSIbPyLNXsbTZcPKI_U80QwdX5pWf8 Resx --format resx --outputdir .\Resources

The first argument `**–-sheet**` is the Google Sheet document ID followed by the Sheet Name, the next argument `**–-format**` specifies the output file format, and the last argument `**–-outputdir**` specifies the output folder.  

You can get the Document ID from the URL of the Google Sheet  

![](/assets/images/resx-google-sheets.png)

Here's an example output  

![](/assets/images/resx-csvtrans-output.png)

Now I can just bring these files into my project and use them directly. With the modern csproj format I don't even need to do any changes to include these translation files, as long as the resx files are in the project folder they will be automagically included into the output. This opens up for dynamic translations at build time using your CI/CD build tools of choice