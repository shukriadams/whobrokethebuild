# LogParsing.BasicErrors

Very simple log parser, finds all lines with word "error" in it. When other more specific log parsers fail to match, this can be a useful way to get a summary of error state in a build.

## Setup

Register plugin, then add to a job under the `LogParserPlugins` node.

    Plugins:
    -   Key: MyBasicErrorsParser
        Path: Wbtb.Extensions.LogParsing.BasicErrors

    Jobs:
    -   Key: MyJob
        LogParserPlugins: 
        -   MyBasicErrorsParser