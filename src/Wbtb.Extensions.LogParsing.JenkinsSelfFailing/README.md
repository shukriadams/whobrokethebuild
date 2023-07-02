# Jenkins self failing

Parses build logs for CPP errors

## Setup

Register plugin, then add to a job under the `LogParsers` node.

    Plugins:
    -   Key: MyCppParser
        Path: Wbtb.Extensions.LogParsing.Cpp

    Jobs:
    -   Key: MyJob
        LogParsers: 
        -   MyCppParser