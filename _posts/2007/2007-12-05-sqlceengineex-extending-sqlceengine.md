---
layout: post
title: SqlCeEngineEx - Extending the SqlCeEngine class
date: "2007-12-05T20:33:00.000+01:00"
author: Christian Resma Helle
tags:
  - SQL Server Compact Edition
  - ".NET Compact Framework"
modified_time: "2008-01-07T09:37:10.267+01:00"
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-8343075279084074763
blogger_orig_url: https://christian-helle.blogspot.com/2007/12/sqlceengineex-extending-sqlceengine.html
---

I use `System.Data.SqlServer.SqlCeEngine()` quite a lot in all my projects. I normally create the database on the fly when the application is launched for the first time and then I populate the initial data via a web service.

I often check if database objects exist before I create them. You can do this by querying the `INFORMATION_SCHEMA` views. I created a helper class called SqlCeEngineEx that contains the following methods for querying the `INFORMATION_SCHEMA`:

1. `bool DoesTableExist(string table)` - Checks if a table exists in the database
2. `string[] GetTables()` - Returns a string array of all the tables in the database
3. `string[] GetTableConstraints(string table)` - Returns a string array of all the constraints for a table
4. `string[] GetTableConstraints()` - Returns a string array of all the constraints in the database

And here is the full code:

```csharp
public class SqlCeEngineEx : IDisposable
{
    private SqlCeEngine engine;

    public SqlCeEngineEx()
    {
        engine = new SqlCeEngine();
    }

    public SqlCeEngineEx(string connectionString)
    {
        engine = new SqlCeEngine(connectionString);
    }

    public bool DoesTableExist(string tablename)
    {
        bool result = false;

        using (SqlCeConnection conn = new SqlCeConnection(LocalConnectionString))
        {
            conn.Open();
            using (SqlCeCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT COUNT(TABLE_NAME)
                      FROM INFORMATION_SCHEMA.TABLES
                      WHERE TABLE_NAME=@Name";
                cmd.Parameters.AddWithValue("@Name", tablename);
                result = Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }

        return result;
    }

    private string[] PopulateStringList(SqlCeCommand cmd)
    {
        List<string> list = new List<string>();

        using (SqlCeDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }
        }

        return list.ToArray();
    }

    public string[] GetTables()
    {
        string[] tables;

        using (SqlCeConnection conn = new SqlCeConnection(LocalConnectionString))
        {
            conn.Open();
            using (SqlCeCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES";
                tables = PopulateStringList(cmd);
            }
        }

        return tables;
    }

    public string[] GetTableConstraints()
    {
        string[] constraints;

        using (SqlCeConnection conn = new SqlCeConnection(LocalConnectionString))
        {
            conn.Open();
            using (SqlCeCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS";
                constraints = PopulateStringList(cmd);
            }
        }

        return constraints;
    }

    public string[] GetTableConstraints(string tablename)
    {
        string[] constraints;

        using (SqlCeConnection conn = new SqlCeConnection(LocalConnectionString))
        {
            conn.Open();
            using (SqlCeCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT CONSTRAINT_NAME
                      FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                      WHERE TABLE_NAME=@Name";
                cmd.Parameters.AddWithValue("@Name", tablename);
                constraints = PopulateStringList(cmd);
            }
        }

        return constraints;
    }

    public string LocalConnectionString
    {
        get { return engine.LocalConnectionString; }
        set { engine.LocalConnectionString = value; }
    }

    public void Compact()
    {
        engine.Compact(null);
    }

    public void Compact(string connectionString)
    {
        engine.Compact(connectionString);
    }

    public void CreateDatabase()
    {
        engine.CreateDatabase();
    }

    public void Repair(string connectionString, RepairOption options)
    {
        engine.Repair(connectionString, options);
    }

    public void Shrink()
    {
        engine.Shrink();
    }

    public bool Verify()
    {
        return engine.Verify();
    }

    public void Dispose()
    {
        engine.Dispose();
        engine = null;
    }
}
```
