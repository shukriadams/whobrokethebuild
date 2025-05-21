# Acme Games Tester

Does log linker lookups, specific to the Acme Games Corporation

## Setup

Register plugin, then add to a job under the `LogParsers` node.

    Plugins:
    -   Key: MyAcmeGamesTester
        Path: Wbtb.Extensions.LogParsing.AcmeGamesTester
        Config:
        -   MaxLineLength: int (optional maximum space-unbroken line length allowed in log. If exceeded, the log or chunk will not be parsed)
        -   SectionDelimiter: string (optional string to break large logs up to into more performant chunks)

    Jobs:
    -   Key: MyJob
        LogParsers: 
        -   MyAcmeGamesTester