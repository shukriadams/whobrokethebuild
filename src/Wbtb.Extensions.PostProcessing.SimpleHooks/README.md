# SimpleHooks

Post-processor that assigns build failures based on hooks defined by other plugins. This plugin is meant to be run in tandem with plugins like parsers like `Wbtb.Extensions.LogParsing.SimpleRegex`, which can be user-configured to pick up specific strings in logs. Once a string is detected, simplehooks can be used to catch that and assign cause to a build failure

## Setup

Suppose you have a log parser with the key `Network_Smoke_Test` already registered. That log parser will catch and report a string in the log that indicated a smoketest failure. Register a simplehooks plugin, assign it to watch the key `Network_Smoke_Test`. Add to a job under the `PostProcessors` node. 

    Plugins:
    -   Key: Smoke_test_blamer
        Path: Wbtb.Extensions.PostProcessing.SimpleHooks
        Custom:
            KeyHook: Network_Smoke_Test                        # hook from parser to watch for
            Description: Smoke test error                       # optional short human-friendly text. if not set, hook is used
                
    Jobs:
    -   Key: MyJob
        PostProcessors: 
        -   Smoke_test_blamer

