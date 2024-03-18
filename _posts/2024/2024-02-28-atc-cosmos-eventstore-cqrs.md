---
layout: post
title: Cosmos DB, Event Sourcing, and CQRS with A Touch of Class
date: '2024-02-28'
author: Christian Helle
tags:
- Azure
- Cosmos DB
redirect_from:
- /2024/02/atc-cosmos-eventstore-cqrs/
- /2024/02/atc-cosmos-eventstore-cqrs
- /2024/atc-cosmos-eventstore-cqrs/
- /2024/atc-cosmos-eventstore-cqrs
- /atc-cosmos-eventstore-cqrs/
- /atc-cosmos-eventstore-cqrs
---

For the past 6 or 7 years, I have been using [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/introduction?WT.mc_id=DT-MVP-5004822) as my go-to data store. Document databases make so much more sense for the things that I have been building over these past years. In an [old post](/2023/02/atc-cosmos.html), I wrote about a library called [Atc.Cosmos](https://github.com/atc-net/atc-cosmos) that I was part of building that I use for configuring containers in Azure Cosmos DB to provides easy, efficient, and convenient ways to read and write document resources.

One of the things I use Azure Cosmos DB is for implementing [CQRS](https://www.eventstore.com/cqrs-pattern), a pattern I first heard about from [Mark Seemann](https://blog.ploeh.dk/), an old colleague from a decade and a half ago. I first started really working with Event Sourcing and CQRS 6 or 7 years ago, when I started working with a colleague named [Lars Skovslund](https://www.linkedin.com/in/larsskovslund) 

I must begin this post by stating that I am in no way an expert in the subject and this article is about implementing the pattern with Azure Cosmos DB using a library called [Atc.Cosmos.EventStore](https://github.com/atc-net/atc-cosmos-eventstore)
