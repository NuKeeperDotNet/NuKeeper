---
title: "Update"
weight: 20
---

Use the update command to apply an update to code.

Apply a update chosen by NuKeeper to a solution:

```sh
cd C:\code\MyApp
nukeeper update
```

or

```sh
nukeeper update C:\code\MyApp
```

Apply up to ten package updates in a run:

```sh
nukeeper update C:\code\MyApp --maxpackageupdates 10
```

Apply an update to a particular package:

```sh
nukeeper update C:\code\MyApp --include SomePackageThatIWant
```

Exclude updates to a particular package:

```sh
nukeeper update C:\code\MyApp --exclude SomePackageThatIDoNotWant
```

Apply updates to all packages of [the AWS SDK](https://aws.amazon.com/sdk-for-net/), a set of closely-related packages with names that all start with `AWSSDK.`:

```sh
nukeeper update C:\code\MyApp --include ^AWSSDK. --maxpackageupdates 100
```

### Include and exclude

Include and exclude package names with `--include` and `--exclude`.
e.g. `--include nlog` or `--exclude aspnet`

Exclude specifies that packages that match the pattern will _not_ be applied. Include specifies that _only_ packages that match the pattern can be applied.

The patterns are regular expressions. This is not a regular expression tutorial, but it is worth knowing a few common ways to use it:

* By default, it is substring match. e.g. `--include NLog` will include all package names that contain the text `NLog`.
* You can [anchor](https://docs.microsoft.com/en-us/dotnet/standard/base-types/anchors-in-regular-expressions) the match with `^` at the start or `$` at the end to ensure that the package name starts and/or ends with the specified text. Using both start and end anchors means that it must be an exact match for the whole package name. e.g. `i=^NLog$` will include only the package `NLog`.
* You can use `|` as an "or" to match one of several things. However since on the command line the `|` has special meaning to pipe command output, you have to escape it.
  * On the command line on windows you escape the `|`  as `^|`. e.g.  to exclude updates to framework packages: `--exclude (Microsoft^|System^|AspNet)`.
  * In the bash shell on linux, you instead escape by wrapping the whole argument in single quotes, e.g.   `--exclude '(Microsoft|System|AspNet)'`.

### Restricting updates

#### Restrict by version change

When I do not want major version changes, only minor or patch version changes.

```sh
nukeeper update --change minor
```

e.g. An update from version `1.2.3` to version `1.5.0` will be selected even if version `2.0.0` is also available. [Semantic versioning](https://semver.org/ "Semantic Versioning documentation") calls these three numbers `major`, `minor` and `patch` versions. If semantic versioning is being well-used, then breaking changes only appear in major versions.

The default value is `major`.

#### Restrict by package age

When I am cautious, only want updates that have been available for 3 weeks or more.

```sh
nukeeper update --age 3w
```

You can specify minimum update age in weeks (`w`), days (`d`) or hours (`h`), e.g. `--age 6w`, or `--age 12h`. The default is 7 days. You can use zero (`0`) on it's own.

This is a precaution against being on the "bleeding edge" by taking new packages immediately. Sometimes issues are discovered just after package release, and the package is removed or replaced by a `x.0.1` fix release.

When I am living on the edge, I will take updates as soon as they are available.

```sh
nukeeper update --age 0
```