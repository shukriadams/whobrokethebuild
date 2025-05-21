# CPP log parser

Parses build logs for CPP errors

## Setup

Register plugin, then add to a job under the `LogParserPlugins` node.

    Plugins:
    -   Key: MyCppParser
        Path: Wbtb.Extensions.LogParsing.Cpp
        Config:
        -   MaxLineLength: int (optional maximum space-unbroken line length allowed in log. If exceeded, the log or chunk will not be parsed)
        -   SectionDelimiter: string (optional string to break large logs up to into more performant chunks)

    Jobs:
    -   Key: MyJob
        LogParsers: 
        -   MyCppParser
