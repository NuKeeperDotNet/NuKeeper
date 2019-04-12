---
title: "Inspect"
weight: 10
---

Use the inspect command to show updates that can be applied to a solution.

Inspect the current folder:

```bat
cd C:\code\MyApp
nukeeper inspect
```

This produces output e.g.

```txt
Found 2 package updates
Microsoft.Extensions.Configuration.Json to 2.1.0 from 2.0.2 in 1 place since 13 days ago.
Swashbuckle.AspNetCore to 2.5.0 from 2.4.0 in 1 place since 6 days ago.
```

You can inspect a different folder:

```bat
nukeeper inspect C:\code\MyApp
```
