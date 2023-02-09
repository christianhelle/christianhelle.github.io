---
layout: post
title: Atc.Cosmos - Azure Cosmos DB with A Touch of Class
date: '2023-02-09'
author: Christian Helle
tags: 
- CosmosDb
redirect_from:
- /2023/02/atc-cosmos
- /2023/02/atc-cosmos
- /2023/atc-cosmos/
- /2023/atc-cosmos
- /atc-cosmos/
- /atc-cosmos
---

A couple of years ago, me and a group of colleagues and friends, decided that we should open source the ideas, concepts, design patterns, and libraries that we have been carrying around from project to project. From this idea, [ATC.NET](https://github.com/atc-net) was born. We had to come up with a name for this project, which in turn became a GitHub Organization by this time of writing has 40 [members](https://github.com/orgs/atc-net/people), 28 active [repositories](https://github.com/orgs/atc-net/repositories), 1.8 million [total package downloads from NuGet](https://www.nuget.org/profiles/atc-net), and 70k monthly [downloads from PyPi](https://pypistats.org/packages/atc-dataplatform)

For the past 6 years, I have been using [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/introduction?WT.mc_id=DT-MVP-5004822) (formerly known as [Document DB](https://azure.microsoft.com/en-us/blog/dear-documentdb-customers-welcome-to-azure-cosmos-db?WT.mc_id=DT-MVP-5004822)) as my go-to data store. Document databases make so much more sense for the things that I have been building over the past 6 years. The library [Atc.Cosmos](https://github.com/atc-net/atc-cosmos) is the result of years of collective experience solving problems using the same patterns. Atc.Cosmos is a library for configuring containers in Azure Cosmos DB and provides easy, efficient, and convenient ways to read and write document resources.

## Getting Started
The library is installed by adding the NuGet package Atc.Cosmos to your project. Once the library is added to your project, you will have access to the following interfaces, used for reading and writing Cosmos document resources:

- `ICosmosReader<T>`
- `ICosmosWriter<T>`
- `ICosmosBulkReader<T>`
- `ICosmosBulkWriter<T>`

Where `T` is a document resource represented by a class deriving from the `CosmosResource` base-class, or by implementing the underlying `ICosmosResource` interface directly.
