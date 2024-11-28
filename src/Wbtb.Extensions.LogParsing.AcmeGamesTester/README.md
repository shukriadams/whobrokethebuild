# Acme Games Tester

Does log linker lookups, specific to the Acme Games Corporation

## Setup

Register plugin, then add to a job under the `LogParsers` node.

    Plugins:
    -   Key: MyAcmeGamesTester
        Path: Wbtb.Extensions.LogParsing.AcmeGamesTester
        Config:
        -   MaxLogSize: int (maximum log character length to parse. Logs longer than this will return "Log too long, did not parse message.")

    Jobs:
    -   Key: MyJob
        LogParsers: 
        -   MyAcmeGamesTester