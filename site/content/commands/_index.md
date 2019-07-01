---
title: Commands
weight: 20
pre: "<b>2. </b>"
chapter: true
---

### NuKeeper

# Commands


| Name      |  Type    | Intention |
|-----------|----------|---------------------------|
| `inspect` | local | inspect outdated packages |
| `update` | local| updates outdated packages |
| `repo` | remote| find outdated packages and updates it with a PR to your platform |
| `organisation` | remote| find outdated packages and updates it with a PR to your platform for the whole organisation. |
| `global` | remote| find outdated packages and updates it with a PR to your platform for all your repositories.|


{{% notice info %}}
The organisation and the global command are only available for *github*, because the other platforms don't allow such a powerful token.
{{% /notice %}}
