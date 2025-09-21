# SimpleRegex

Log parser using only user-defined regexes. A regex can be provided to identify some conditions. Additional regexes can be provided to describe the error.

## Setup

Register plugin, then add to a job under the `LogParsers` node.

    Plugins:
    -   Key: MySmokeTest                                            # required string key, can be used by post-processors as identification hook.
        Path: Wbtb.Extensions.LogParsing.SimpleRegex
        Custom:
            Key: myNetworkSmokeTest                                 
            SectionDelimiter : " ---- my log delimiter -----"       # optional log delimiter to split log into sections before parsing. Helps with performance.
            Regex: "^Network smoketest error occurred:"             # required. regex to match what you're looking to find
            Describes:                                              # optional extra info to describe the condition. Presented as "name : value" lines
            -   Name : My Ping Errors                               # required string label
                Regex : ping_Errors:(.*)                            # required single-group regex to find value for this item 
            -   Name : My Timout Errors
                Regex : Net_timeouts:(\d*)
                
    Jobs:
    -   Key: MyJob
        LogParsers: 
        -   MyRegexPlease

