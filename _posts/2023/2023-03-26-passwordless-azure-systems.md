---
layout: post
title: Building Passwordless Systems on Azure
date: '2023-03-26'
author: Christian Helle
tags: 
- Azure
---

In today's world of cloud computing, security is of utmost importance. With the vast amounts of data being stored in the cloud, it is essential to ensure that it is safe and secure. One of the challenges that developers face is securing the connection between their applications and the cloud. Traditionally, this has been done using connection strings, access keys, passwords, and other secrets. Not only do these credentials need to be stored securely, but they also need to be rotated regularly to prevent unauthorized access.

Thankfully, Azure provides a solution for this: [**Role-Based Access Control**](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview?WT.mc_id=DT-MVP-5004822), or [**RBAC**](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview?WT.mc_id=DT-MVP-5004822) for short. RBAC is a feature that allows you to define roles for different applications, users, or groups within your organization. Each role has a set of permissions that define what actions can be taken by the user or group. With RBAC, you can grant access to specific resources without the need for secrets, making it a more secure method of accessing resources in Azure.

If you want a deeper understanding of RBAC then I suggest that you read [**What is Azure role-based access control (Azure RBAC)?**](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview?WT.mc_id=DT-MVP-5004822) from [**Microsoft Learn**](https://learn.microsoft.com/?WT.mc_id=DT-MVP-5004822)

In this article, we will explore how to avoid using connection strings, access keys, passwords, and secrets and instead use role-based access control (RBAC) in Azure. I will demonstrate how to connect to Azure resources using the applications [**Managed Service Identity**](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview?WT.mc_id=DT-MVP-5004822), how to assign roles access from the Azure Portal and more importantly how to provision your Azure Resources with role assignments from your CI/CD pipeline using [**Bicep**](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview?WT.mc_id=DT-MVP-5004822)

To make this article more interesting, let's create an application that has a REST API and some Azure Functions. The REST API reads and writes to a CosmosDb container, publishes messages to a ServiceBus topic and an EventHub. The Azure Functions are triggered by the ServiceBus topic messages.

![](/assets/images/passwordless-azure-resources.png)

This overly simplified setup is a common theme in most of the Azure systems that I have been involved in. I put everything in a single resource group for simplicity but I hope that the core idea is clear. This example uses EventHub to illustrate a scenario where an application publishes event data to a Data Platform. An important part to notice in this setup is that we do not need to use a [**Azure Key Vault**](https://learn.microsoft.com/en-us/azure/key-vault/general/basic-concepts?WT.mc_id=DT-MVP-5004822) resoruce because there will be **no secrets whatsoever**

## Azure Key Vault

If we were to build a similar system **without** using [**RBAC**](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview?WT.mc_id=DT-MVP-5004822) then we would probably include an [**Azure Key Vault**](https://learn.microsoft.com/en-us/azure/key-vault/general/basic-concepts?WT.mc_id=DT-MVP-5004822) to securely store our secrets, and grant only our App Services LIST and GET access to those secrets. These secrets will most likely be provisioned from some CI/CD pipeline which will also be responsible for rotating secrets upon every deployment. The problem with that setup is that anyone who can access the Keyvault can also access the secrets. Another problem with that is that you need to build some infrastructure yourself for rotating secrets to avoid down time, this could be something in the lines of creating a new secret (connection string, access key, password, etc) and storing that to the same Keyvault Secret entry, then ensuring that the App has loaded the new secret, then deleting the old secret (connection string, access key, password, etc) from whatever system owns it. There are couple of ways to automate rotating secrets, all of which I find to be rather troublesome.

## Managed Service Identity

Before we can do anything, our application, the REST API and Azure Functions, running on App Services need an identity. We enable a system assigned [**Managed Service Identity**](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview?WT.mc_id=DT-MVP-5004822)

![](/assets/images/passwordless-azure-appservice-msi.png)

![](/assets/images/passwordless-azure-appservice-msi-created.png)

which will create a application in Azure Active Directory to which we will assign roles to

![](/assets/images/passwordless-azure-appservice-msi-ad.png)

We need to do this for both the REST API and the Azure Functions.