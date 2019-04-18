---
title: "Output and Logging"
weight: 30
---


Text produced by NuKeeper falls into two categories: *logging* and *output*.
Both can be independently sent to `console`, or to `file`, or turned `off`. Both default to the console.

Options related to logging are `verbosity`, `logdestination` and `logfile`. Options related to output are `outputformat`, `outputdestination`, and `outputfile`

If the run of NuKeeper is intended to produce Pull Requests, then output and logging is less important. But if the run is intended to inspect local code or remote repositories, the output is the primary result of the run. Logging is important for debugging issues.

Logging has verbosity so that it can produce more lines or fewer lines. Verbosity is one of `Quiet`, `Minimal`, `Normal`, `Detailed`.

Output has different formats, intended for different consumers, be it human or machine-readable.

* `Text` is the default format, and is intended to be human readable.
* `None` turns it off entirely. No output is produced.
* `Csv` produces a report in Comma-Separated Value format, with one line for each package update found.
* `Metrics` produces several code metrics, one per line, each with a "name: number" format. [The `libyears` metric](https://libyear.com/) is included.
* `libyears` outputs only the [libyears](https://libyear.com/) number.

We may add to the output formats over time based on what people need.

For both logging and output, both destination and file options can be specified on the command-line or in the settings file. The usual fallback applies - values taken are (in order) from command line, from settings file, from the default value.

In both cases, destination is an enumeration, one of `Console`, `File` or `Off`. `Off` makes a null object that does not record the data at all. `File` appends to the named file.

In both cases, if the destination is `File` and no file name is given, then a default file name is used, `nukeeper.log` for logging, and `nukeeper.out` for output. So if you specify `--logdestination file` on the command-line and nothing else log-related, logging will go to the file `nukeeper.log`.

In both cases, the default destination is `Console`, unless a file name is given on the command-line, in which case the default is `File`. So if you specify `--outputfile somefile.out` on the command-line and nothing else output-related, output will go to the file `somefile.out`.
