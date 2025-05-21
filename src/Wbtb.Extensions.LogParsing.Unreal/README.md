# Unreal log errors

Parses build logs from Unreal engine.

## Setup

Register plugin, then add to a job under the `LogParsers` node.

    Plugins:
    -   Key: MyUnrealParser
        Path: Wbtb.Extensions.LogParsing.Unreal
        Config:
        -   MaxLineLength: int (optional maximum space-unbroken line length allowed in log. If exceeded, the log or chunk will not be parsed)
        -   SectionDelimiter: string (optional string to break large logs up to into more performant chunks)


    Jobs:
    -   Key: MyJob
        LogParsers: 
        -   MyUnrealParser