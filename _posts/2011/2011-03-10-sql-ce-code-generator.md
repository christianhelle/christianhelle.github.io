---
layout: post
title: SQL Compact Code Generator
date: '2011-03-10T23:17:00.004+01:00'
author: Christian Resma Helle
tags:
- CodePlex
- SQL Server Compact Edition
modified_time: '2012-01-19T14:45:35.026+01:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-3941131121333901945
blogger_orig_url: https://christian-helle.blogspot.com/2011/03/sql-ce-code-generator.html
redirect_from:
- /blog/2010/01/24/sqlce-code-generator/
---

More than a year ago, I published a project on CodePlex called [SQL Compact Code Generator](http://github.com/christianhelle/sqlcecodegen). Unfortunately, I never managed to find the time to do some work on it and the project was set on a very long hold. A year after I suddenly really needed such a tool and decided that I should put in some hours on the project.  

I'm currently working on a large enterprise project where changes to the database schema is done rather frequently, to avoid the pain of updating my data layer after every change I decided to use my code generator.  

Here's some details I pulled directly off the ~~CodePlex~~ [Github](http://github.com/christianhelle/sqlcecodegen) page.

### Project Description
Contains a stand alone GUI application and a Visual Studio Custom Tool for automatically generating a .NET data access layer code for objects in a SQL Server Compact Edition database.  

**Features:**

- Visual Studio 2008 and 2010 Custom Tool Support
- Creates entity classes for each table in the database
- Generates data access code that implements the Repository Pattern
- Generates methods for Create, Read, Update and Delete operations
- Generates SelectBy and DeleteBy methods for every column in every table
- Generates a Purge method for every table to delete all records
- Generates Count() method for retrieving the number of records in each table
- Generates CreateDatabase() method for re-creating the database
- Generates xml-doc code comments for entities and data access methods
- Generates Entity Unit Tests
- Generates Data Access Unit Tests
- Generates .NET Compact and Full Framework compatible code
- Support for SQL Compact Edition version 4.0
- Multiple test framework code generation (MSTest, NUnit, xUnit)
- Transaction support per DataRepository instance (Begin, Commit, Rollback)
- Code generation options to enable/disable unit test code generation
- Windows Phone 7 "Mango" support for generating a LINQ to SQL DataContext

**Data Access Custom Tools:**
- SQLCECodeGenerator - Generates entity and data access components using a single file.
- SQLCEMangoCodeGenerator - Generates Windows Phone 7 "Mango" SQLCE entity and data access components using a single file
- SQLCECodeGeneratorV2 - Generates entity and data access components using several files. One file per class or interface
- SQLCEMangoCodeGeneratorV2 - Generates Windows Phone 7 "Mango" SQLCE entity and data access components using several files. One file per class or interface

**Data Access Unit Test Custom Tools:**
- SQLCEMSTestCodeGenerator - Generates entity and data access unit test code that targets MSTest using a single file
- SQLCENUnitCodeGenerator - Generates entity and data access unit test code that targets NUnit using a single file
- SQLCEXUnitCodeGenerator - Generates entity and data access unit test code that targets xUnit.net using a single file
- SQLCEMSTestCodeGeneratorV2 - Generates entity and data access unit test code that targets MSTest using several files. One file per class or interface
- SQLCENUnitCodeGeneratorV2 - Generates entity and data access unit test code that targets NUnit using several files. One file per class or interface
- SQLCEXUnitCodeGeneratorV2 - Generates entity and data access unit test code that targets xUnit.net using several files. One file per class or interface

**Screenshots**

![](/assets/images/sqlcecodegen-entities.png)

![](/assets/images/sqlcecodegen-dal.png)

![](/assets/images/sqlcecodegen-mock.png)

![](/assets/images/sqlcecodegen-export-files.png)

![](/assets/images/sqlcecodegen-export-output.png)