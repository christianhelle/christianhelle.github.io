---
layout: post
title: Querying Overridden Check-in Policies
date: '2007-06-04T17:54:00.000+02:00'
author: Christian Resma Helle
tags:
- TFS
modified_time: '2007-06-05T15:48:36.967+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-3366694715429493200
blogger_orig_url: https://christian-helle.blogspot.com/2007/06/querying-overridden-check-in-policies.html
---

In the Team Foundation Server, you can enable certain policies for checking in files. The default install will contain policies that can verify that the check-in is associated with a work item, or that unit tests were created for the changes made, etc etc. But even though certain rules were made for checking in, the user is still given the possibility to override these policies, if the user decides to override the policy then the user is prompted with a dialog, where the user can input their "reason" for ignoring such policies. This action is logged to the TFS databases.

In this article I will show you how to query the database `TfsVersionControl` and `TfsWarehouse` to get more information for the overridden check-in policy.

let's start off with openning a query to the database server that TFS uses then type in the following query:

```sql
SELECT
    'Changeset ID'=p.ChangeSetId,
    'Creation Date'=cs.CreationDate,
    'Check-in Comment'=cs.Comment,
    'Override Reason'=p.Comment,
    'Owner'=pr.Person,
    'Email'=pr.Email
FROM
    TfsVersionControl..tbl_PolicyOverride p
INNER JOIN
    TfsVersionControl..tbl_ChangeSet cs ON p.ChangeSetId=cs.ChangeSetId
INNER JOIN
    TfsVersionControl..tbl_Identity i ON cs.OwnerId=i.IdentityId
INNER JOIN
    TfsWarehouse..Person pr ON i.DisplayName=(pr.Domain+'\'+pr.Alias)
ORDER BY
    cs.ChangeSetId DESC
```

This query will provide you with the change set number, date, check-in comment, override reason, change set owner and email address for every overridden check-in policy.