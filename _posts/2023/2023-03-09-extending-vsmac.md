---
layout: post
title: Extending Visual Studio for Mac 2022
date: '2023-03-08'
author: Christian Helle
tags: 
- Visual Studio for Mac
redirect_from:
- /2023/03/extending-vsmac
- /2023/03/extending-vsmac
- /2023/extending-vsmac/
- /2023/extending-vsmac
- /extending-vsmac/
- /extending-vsmac
---

The extensibility story for [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) was almost non-existent for a while, and the [documentation for getting started](https://learn.microsoft.com/en-us/previous-versions/visualstudio/mac/extending-visual-studio-mac-walkthrough?WT.mc_id=DT-MVP-5004822) was really outdated. [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) was originally a re-branding of Xamarin Studio, which was built over MonoDevelop. The extensibility SDK's we used for the longest time was still all from the old MonoDevelop Addin libraries.

For [Visual Studio for Mac 2022](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) this has changed and now we can create Visual Studio for Mac extensions using the [Microsoft.VisualStudioMac.Sdk](https://www.nuget.org/packages/Microsoft.VisualStudioMac.Sdk) library that we can install from [nuget.org]([Microsoft.VisualStudioMac.Sdk](https://www.nuget.org/packages/Microsoft.VisualStudioMac.Sdk)). To make things even better, we can now break free of our old .NET Framework 4.x shackles and start targetting .NET 7.0. and use the SDK style csproj format

As of the time I'm writing this, there is still no File->New->Extension Project, but it's not hard to get started either. 

Here's how a csproj file for an empty Visual Studio for Mac extension project looks like:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.VisualStudioMac.Sdk" Version="17.0.0" />
  </ItemGroup>
</Project>
```

With the new SDK's you can build the project from the command line simply by using `dotnet build`. You may have noticed that I explicitly set `<CopyLocalLockFileAssemblies>` to `true`. This is because if I don't do this then the DLL's won't be copied over to the build output folder and won't be included in the final .mpack package either, which will in the end make your extension not work

Without explicitly setting `<CopyLocalLockFileAssemblies>` to `true` the build output folder will look a bit empty

```bash
$ ls -l bin/Debug/net7.0

total 168
-rw-r--r--  1 christian  staff  55858 Mar  8 23:32 MyExtension.VSMac.deps.json
-rw-r--r--  1 christian  staff   5120 Mar  8 23:32 MyExtension.VSMac.dll
-rw-r--r--  1 christian  staff  18344 Mar  8 23:32 MyExtension.VSMac.pdb
drwxr-xr-x  4 christian  staff    128 Mar  8 23:31 runtimes
```

and with `<CopyLocalLockFileAssemblies>` set to `true`

```bash
$ ls -l bin/Debug/net7.0

total 6024
-rwxr--r--  1 christian  staff   212584 Oct 21  2021 Microsoft.Build.Framework.dll
-rwxr--r--  1 christian  staff  1028208 Oct 21  2021 Microsoft.Build.Tasks.Core.dll
-rwxr--r--  1 christian  staff   300648 Oct 21  2021 Microsoft.Build.Utilities.Core.dll
-rwxr--r--  1 christian  staff    28560 May 13  2021 Microsoft.NET.StringTools.dll
-rwxr--r--  1 christian  staff    26224 Oct 23  2021 Microsoft.Win32.SystemEvents.dll
-rw-r--r--  1 christian  staff    55858 Mar  8 23:36 MyExtension.VSMac.deps.json
-rw-r--r--  1 christian  staff     5120 Mar  8 23:36 MyExtension.VSMac.dll
-rw-r--r--  1 christian  staff    18344 Mar  8 23:36 MyExtension.VSMac.pdb
-rwxr--r--  1 christian  staff   184944 Oct 23  2021 System.CodeDom.dll
-rwxr--r--  1 christian  staff   395376 Oct 23  2021 System.Configuration.ConfigurationManager.dll
-rwxr--r--  1 christian  staff   175216 Oct 23  2021 System.Drawing.Common.dll
-rwxr--r--  1 christian  staff    54136 Sep 13  2019 System.Resources.Extensions.dll
-rwxr--r--  1 christian  staff   246912 Oct 23  2021 System.Security.Cryptography.Pkcs.dll
-rwxr--r--  1 christian  staff    20592 Oct 23  2021 System.Security.Cryptography.ProtectedData.dll
-rwxr--r--  1 christian  staff   160632 Nov 15  2019 System.Security.Cryptography.Xml.dll
-rwxr--r--  1 christian  staff   104048 Oct 23  2021 System.Security.Permissions.dll
-rwxr--r--  1 christian  staff    25712 Oct 23  2021 System.Windows.Extensions.dll
drwxr-xr-x  3 christian  staff       96 Mar  8 23:36 ref
drwxr-xr-x  4 christian  staff      128 Mar  8 23:31 runtimes
```
