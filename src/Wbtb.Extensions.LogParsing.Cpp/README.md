# CPP log parser

Parses build logs for CPP errors

## Setup

Register plugin, then add to a job under the `LogParserPlugins` node.

    Plugins:
    -   Key: MyCppParser
        Path: Wbtb.Extensions.LogParsing.Cpp

    Jobs:
    -   Key: MyJob
        LogParserPlugins: 
        -   MyCppParser