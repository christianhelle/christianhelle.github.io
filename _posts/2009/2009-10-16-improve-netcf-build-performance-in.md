---
layout: post
title: Improve .NETCF Build Performance in Visual Studio
date: '2009-10-16T09:27:00.015+02:00'
author: Christian Resma Helle
tags:
- Visual Studio
- ".NET Compact Framework"
modified_time: '2009-10-16T14:54:02.265+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-5013021704875596972
blogger_orig_url: https://christian-helle.blogspot.com/2009/10/improve-netcf-build-performance-in.html
---

A lot of .NETCF developers are surprisingly not aware of the [Platform Verification Task](http://blogs.msdn.com/vsdteam/archive/2006/09/15/756400.aspx) in Visual Studio. Disabling this in the build process will speed up the build of .NETCF projects. To make things quick and short, here's what you need to do:  
  
1) Open the file C:\\WINDOWS\\Microsoft.NET\\Framework\\v3.5\\Microsoft.CompactFramework.Common.targets for editing.  
  
2) Change the following in Line 99

```xml
<Target
  Name="PlatformVerificationTask">
  <PlatformVerificationTask
    PlatformFamilyName="$(PlatformFamilyName)"
    PlatformID="$(PlatformID)"
    SourceAssembly="@(IntermediateAssembly)"
    ReferencePath="@(ReferencePath)"
    TreatWarningsAsErrors="$(TreatWarningsAsErrors)"
    PlatformVersion="$(TargetFrameworkVersion)"/>
</Target>
```

to

```xml
<Target
  Name="PlatformVerificationTask">
  <PlatformVerificationTask
    Condition="'$(DoPlatformVerificationTask)'=='true'"
    PlatformFamilyName="$(PlatformFamilyName)"
    PlatformID="$(PlatformID)"
    SourceAssembly="@(IntermediateAssembly)"
    ReferencePath="@(ReferencePath)"
    TreatWarningsAsErrors="$(TreatWarningsAsErrors)"
    PlatformVersion="$(TargetFrameworkVersion)"/>
</Target>
```

The following configuration above was an excert from an article called [Platform Verification Task leading to slow builds on compact framework projects](http://www.developer-corner.com/blog/2009/07/28/slow-build-on-compact-framework-projects/)