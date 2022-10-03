---
layout: post
title: Generic Singleton Implementation
date: '2009-10-13T10:56:00.012+02:00'
author: Christian Resma Helle
tags:
- Design Patterns
modified_time: '2009-10-14T00:13:45.921+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-9134577889776386894
blogger_orig_url: https://christian-helle.blogspot.com/2009/10/generic-singleton-implementation.html
---

Here's a helpful class that I've been using for implementing singleton objects through the years. This way I can use any class (as long as it has a default constructor) as a singleton object. Originally, I called this class the YetAnotherSingleton< T > but that was simply too unprofessional and I ended up renaming it.

```csharp
public static class Singleton<T> where T : class, new()
{
    private static readonly object staticLock = new object();
    private static T instance;
 
    public static T GetInstance()
    {
        lock (staticLock)
        {
            if (instance == null)
                instance = new T();
            return instance;
        }
    }
 
    public static void Dispose()
    {
        if (instance == null)
            return;
        var disposable = instance as IDisposable;
        if (disposable != null)
            disposable.Dispose();
        instance = null;
    }
}
```

And here's an example of how to use the class above. To improve performance of web service calls its a good idea to use a singleton instance of the web service proxy class.

```csharp
public static class ServiceClientFactory
{
    public static Service GetService()
    {
        var ws = Singleton<Service>.GetInstance();
        ws.Url = ConfigurationManager.AppSettings["ServiceUrl"];
        ws.Credentials = GetCredentials();
        return ws;
    }
 
    private static ICredentials GetCredentials()
    {
        var username = ConfigurationManager.AppSettings["ServiceUsername"];
        var password = ConfigurationManager.AppSettings["ServicePassword"];
        var domain = ConfigurationManager.AppSettings["ServiceDomain"];
        return new NetworkCredential(username, password, domain);
    }
}
```

In my next few articles I'll be sharing code from my design pattern framework that I've been using through the years.