# JustRegexPlease

Log parser using only user-defined regexes. A regex can be provided to identify some conditions. Additional regexes can be provided to describe the error.

## Setup

Register plugin, then add to a job under the `LogParsers` node.

    Plugins:
    -   Key: MyRegexPlease
        Path: Wbtb.Extensions.LogParsing.JustRegexPlease
        Custom:
            Name: My Network Smoke Test
            SectionDelimiter : " ---- my log delimiter -----"  # optional
            Regex: "^Network smoketest error occurred:"
            Describes:
            -   Name : My Ping Errors
                Regex : "ing_Errors"
            -   Name : My Timout Errors
                Regex : "Net_timeouts"
                
    Jobs:
    -   Key: MyJob
        LogParsers: 
        -   MyRegexPlease

