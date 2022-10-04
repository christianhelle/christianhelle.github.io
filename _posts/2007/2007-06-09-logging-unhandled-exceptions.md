---
layout: post
title: Logging Unhandled Exceptions
date: '2007-06-09T16:06:00.001+02:00'
author: Christian Resma Helle
tags:
- Windows Mobile
- ".NET Compact Framework"
modified_time: '2007-06-11T09:51:09.749+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-1410492378440252064
blogger_orig_url: https://christian-helle.blogspot.com/2007/06/logging-unhandled-exceptions.html
---

Bugs are sometimes unavoidable. They're best caught during the development or testing phase. There might be some cases where the developer forgot to handle possible exceptions in a function. It could be possible that this exception isn't handled anywhere at all. But even so, it is still possible to catch this exception. To do this we handle the UnhandledException event of the current AppDomain. We should do this in our `static void Main()` before calling `Application.Run([Main Form])`

Here's a small snippet to accomplish this task.

[C# CODE]

```csharp
static void Main()
{
    AppDomain.CurrentDomain.UnhandledException +=
        new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
    Application.Run(new MainForm());
}

static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Exception exception = (Exception)e.ExceptionObject;
    if (exception != null) {
        Error.Append(exception.Message, exception.StackTrace);
    }
}
```

Now we need a mechanism for saving to a Error log file. Let's create a simple class called Error() and add a function called Append(string message, string stacktrace)

[C# CODE]

```csharp
public class Error
{
    internal static void Append(string message, string stacktrace)
    {
        string file = string.Format("{0}\\Errors.txt",
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase));
        if (File.Exists(file)) {
            FileInfo fi = new FileInfo(file);
            if (fi.Length > 100 * 1024) {
                fi.Delete();
            }
        }
            StreamWriter sw = new StreamWriter(file, true, Encoding.UTF8);
            sw.WriteLine(
                string.Format(
                    "-=-=-=-=-=--=-=-\n{0}\nMESSAGE:\n{1}\nSTACK TRACE:\n{3}\n", 
                    DateTime.Now, 
                    message, 
                    stacktrace));
            sw.Close();
        }
    }
}
```

The `Error()` class should be put in a namespace that is accessible throughout the application. This will be very helpful tool for finding those nasty almost impossible to reproduce bugs.