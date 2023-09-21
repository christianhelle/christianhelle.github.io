---
layout: post
title: How to retrieve a list of installed applications using .NETCF
date: '2010-10-06T14:29:00.016+02:00'
author: Christian Resma Helle
tags:
- How to
- ".NET Compact Framework"
modified_time: '2010-10-06T14:47:34.933+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-3823128256528423396
blogger_orig_url: https://christian-helle.blogspot.com/2010/10/how-to-retrieve-list-of-installed.html
---

In this short post I'd like to demonstrate how to retrieve a list of installed applications on a Windows Mobile device using OMA Client Provisioning. First you need to add a reference to the [Microsoft.WindowsMobile.Configuration](http://learn.microsoft.com/en-us/library/microsoft.windowsmobile.configuration.aspx?WT.mc_id=DT-MVP-5004822) assembly. To retrieve the list we need to process a specific configuration ([UnInstall Configuration Service Provider](http://learn.microsoft.com/en-us/library/aa455977.aspx?WT.mc_id=DT-MVP-5004822)) using the [ConfigurationManager.ProcessConfiguration](http://learn.microsoft.com/en-us/library/microsoft.windowsmobile.configuration.configurationmanager.processconfiguration.aspx?WT.mc_id=DT-MVP-5004822) method. The configuration is described in an XML and the response to this will also be described in XML  
  
To query the device we process this configuration:  
  
```xml
<wap-provisioningdoc>
  <characteristic-query type="UnInstall"/>
</wap-provisioningdoc>
```  
  
The query above will only return installed application that can be **uninstalled**. The device would then respond with something like this:  
  
```xml
<wap-provisioningdoc>
  <characteristic type="UnInstall">
    <characteristic type="Microsoft Application#2">
      <parm name="uninstall" value="0"/>
    </characteristic>
    <characteristic type="Microsoft Application#1">
      <parm name="uninstall" value="0"/>
    </characteristic>
    <characteristic type="Demo Home Screen">
      <parm name="uninstall" value="0"/>
    </characteristic>
  </characteristic>
</wap-provisioningdoc>
```  
  
And here's how to accomplish this task using .NETCF and C#  
  
```csharp
var doc = new XmlDocument();
doc.LoadXml(@"<wap-provisioningdoc><characteristic-query type=""UnInstall""/></wap-provisioningdoc>");
doc = ConfigurationManager.ProcessConfiguration(doc, true);
 
var nodes = doc.SelectNodes("wap-provisioningdoc/characteristic[@type='UnInstall']/characteristic/@type");
foreach (var node in nodes.Cast<XmlNode>())
{
    Trace.WriteLine(node.InnerText);
}
```  

I hope you found this useful.