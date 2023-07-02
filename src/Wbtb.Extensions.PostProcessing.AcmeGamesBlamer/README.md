# AcmeGamesBlamer

Blames people who break Unreal projects in Perforce.

## Setup

Register plugin, then add to a job under the `LogParserPlugins` node.

    Plugins:
    -   Key: AcmeGamesBlamer
        Path: Wbtb.Extensions.PostProcessing.AcmeGamesBlamer

    Jobs:
    -   Key: MyJob
        PostProcessing: 
        -   MyCppParser