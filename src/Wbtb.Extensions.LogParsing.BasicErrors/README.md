# LogParsing.BasicErrors

A simple log parser that matches lines with word "error" in it. When other more specific log parsers fail to match, this can be a useful fallback for guessing a summary of errors in a log.

## Setup

Register plugin, then add to a job under the `LogParserPlugins` node.

    Plugins:
    -   Key: MyBasicErrorsParser
        Path: Wbtb.Extensions.LogParsing.BasicErrors
        Config:
        -   MaxLogSize: int (maximum log character length to parse. Logs longer than this will return "Log too long, did not parse message.")

    Jobs:
    -   Key: MyJob
        LogParserPlugins: 
        -   MyBasicErrorsParser