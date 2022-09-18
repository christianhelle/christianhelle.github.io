---
layout: post
title: AppCenter Extensions for ASP.NET Core and Application Insights
date: '2020-03-01T21:31:00.001+01:00'
author: Christian Helle
tags: Azure Xamarin AppCenter
modified_time: '2020-03-01T21:32:22.348+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-6120354957540588816
blogger_orig_url: https://christian-helle.blogspot.com/2020/03/appcenter-extensions-for-aspnet-core.html
redirect_from:
    - /2020/03/appcenter-extensions-for-asp-net-core-and-application-insights/
    - /2020/03/appcenter-extensions-for-asp-net-core-and-application-insights
    - /2020/03/appcenter-extensions-for-aspnet-core/
    - /2020/03/appcenter-extensions-for-aspnet-core
    - /2020/appcenter-extensions-for-aspnet-core/
    - /2020/appcenter-extensions-for-aspnet-core
    - /appcenter-extensions-for-aspnet-core/
    - /appcenter-extensions-for-aspnet-core
---

In my [previous post](/2020/02/appcenter-extensions-for-xamarinforms.html), I wrote about an open source project called [AppCenterExtensions](https://github.com/christianhelle/appcenterextensions) available at Github and nuget.org. I recently updated this project and added a few components for ASP.NET Core that enables including AppCenter diagnostic information in Application Insights.  
  
The NuGet package is called [AppCenterExtensions.AppInsights](https://www.nuget.org/packages/AppCenterExtensions.AppInsights) and contains extension methods and [ITelemetryInitializer](https://docs.microsoft.com/en-us/dotnet/api/microsoft.applicationinsights.extensibility.itelemetryinitializer) implementations to be used in a ASP.NET Core web app for including AppCenter diagnostic information when logging to Application Insights  
  
Enabling this is easy. Assuming that the project is already configured to use Application Insights, just add the [AppCenterExtensions.AppInsights](https://www.nuget.org/packages/AppCenterExtensions.AppInsights) NuGet package mentioned above to your ASP.NET Core and call **services.AddAppCenterTelemetry()** in the **ConfigureServices** method of the **Startup** class  
  
Here's an example:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configure and register services to the IoC

        services.AddApplicationInsightsTelemetry();
        services.AddAppCenterTelemetry();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure app
    }
}
```

Once this is setup, AppCenter diagnostic information should now be searchable and visible in Application Insights.  
  
Here's a screenshot of search results for the **x-supportkey** header  
  
![](https://github.com/christianhelle/appcenterextensions/blob/master/images/appinsights-search-result.png?raw=true)  
  
and here's a screenshot of the details of a single request containing AppCenter diagnostic information logged in Application Insights  
  
![](https://github.com/christianhelle/appcenterextensions/blob/master/images/appinsights-search-result-details.png?raw=true)  
  
With this flow you can now correlate Crash Reports and Analytics data from AppCenter with the HTTP requests for your backend systems in Application Insights. In the systems that I have been involved with building we include the AppCenter diagnostic information from our [API Gateway](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/direct-client-to-microservice-communication-versus-the-api-gateway-pattern) to all calls to our internal Microservices