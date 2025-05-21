# Unreal log errors

Parses build logs from Unreal engine.

## Setup

Register plugin, then add to a job under the `LogParsers` node.

    Plugins:
    -   Key: MyUnrealParser
        Path: Wbtb.Extensions.LogParsing.Unreal
        Config:
        -   MaxLogSize: int (maximum log character length to parse. Logs longer than this will return "Log too long, did not parse message.")
        -   SectionDelimiter: string (optional string to break large logs up to into more performant chunks)


    Jobs:
    -   Key: MyJob
        LogParsers: 
        -   MyUnrealParser