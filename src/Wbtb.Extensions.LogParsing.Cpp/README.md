# CPP log parser

Parses build logs for CPP errors

## Setup

Register plugin, then add to a job under the `LogParserPlugins` node.

    Plugins:
    -   Key: MyCppParser
        Path: Wbtb.Extensions.LogParsing.Cpp
        Config:
        -   MaxLogSize: int (optional maximum log character length to parse. Logs longer than this will return "Log too long, did not parse message.")
        -   SectionDelimiter: string (optional string to break large logs up to into more performant chunks)

    Jobs:
    -   Key: MyJob
        LogParsers: 
        -   MyCppParser
