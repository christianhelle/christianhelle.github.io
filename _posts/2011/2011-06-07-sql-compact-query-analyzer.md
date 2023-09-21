---
layout: post
title: SQL Compact Query Analyzer
date: '2011-06-07T08:35:00.004+02:00'
author: Christian Resma Helle
tags:
- SQL Server Compact Edition
modified_time: '2012-02-21T11:20:29.650+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-8029180074364801730
blogger_orig_url: https://christian-helle.blogspot.com/2011/06/sql-compact-query-analyzer.html
redirect_from:
- /blog/2011/06/07/sql-compact-query-analyzer/
---

I’ve been working extensively on enterprise mobility projects lately. These applications integrate into large SAP based systems and when testing the system it can get very tedious to set up some temporary data from the backend. I’m also working with some not-so-technical testers that get intimidated by the Visual Studio or the SQL Server Management Studio. This led me to writing an open source project called [SQL Compact Query Analyzer](https://github.com/christianhelle/sqlcequery)  

Here’s some details I pulled directly off the ~~CodePlex~~ [Github](https://github.com/christianhelle/sqlcequery) page

## Project Description
A SQL Server Compact Edition Database Query Analyzer

#### Download latest release [here](https://github.com/christianhelle/sqlcequery/releases/latest)


*Features:*

- Create new database
- Automatically refresh database upon executing create/alter/drop table queries
- Displays database information (database version, filename, size, creation date)
- Displays schema summary (number of tables, columns, primary keys, identity fields, nullable fields)
- Displays the information schema views
- Displays column information (database type, clr type, max length, allows null, etc)
- Displays index information (column name, is unique, is clustered)
- Execute SQL Queries against a SQL Server Compact Edition database
- Execute multiple SQL queries (delimited by a semi colon ;)
- Easily edit the contents of the database
- Display query result as XML
- Shrink and Compact Databases
- SDF file association with SQL Compact Query Analyzer for launching directly by opening the SDF in Windows Explorer
- Displays thumbnails for IMAGE fields
- Generates Schema and Data Scripts
- Supports password protected databases
- Supports SQLCE 3.0, 3.1, 3.5 and 4.0

*Coming at some point:*
- Purge database content
- Create, edit, and drop tables UI
- Create, edit, and delete table references and indexes UI
- Support for SQL Server Compact Edition 2.0


*Screenshots*

![alt text](https://github.com/christianhelle/sqlcequery/raw/master/Screenshots/QueryResultMessages.png)
- Displays database and schema information and executes multiple SQL queries directly

![alt text](https://github.com/christianhelle/sqlcequery/raw/master/Screenshots/EditTable.png)
- Edit the table data directly

![alt text](https://github.com/christianhelle/sqlcequery/raw/master/Screenshots/ContentWithImages.png)
- Display the contents of IMAGE fields

![alt text](https://github.com/christianhelle/sqlcequery/raw/master/Screenshots/QueryResultMessages.png)
- Performance numbers for queries

![alt text](https://github.com/christianhelle/sqlcequery/raw/master/Screenshots/QueryResultErrors.png)
- Query errors

![alt text](https://github.com/christianhelle/sqlcequery/raw/master/Screenshots/ResultsAsXml.png)
- Output result set as XML

![alt text](https://github.com/christianhelle/sqlcequery/raw/master/Screenshots/CreateDatabase.png)
- Create new database

![alt text](https://github.com/christianhelle/sqlcequery/raw/master/Screenshots/Shrink.png)
- Shrink, compact, script database

*Prerequisites:*
- .NET Framework 4.0

Check it out! You might find it useful!