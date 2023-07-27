# AcmeGamesBlamer

Blames people who break Unreal projects in Perforce. 

## Prerequisites

Your build log should contain the clientspec at the time of building, wrapped thusly 
    
    <p4-cient-state>
        ...your clientspec here...
    <p4-cient-state>

To achieve this, you can add something like the following your build script

    echo "<p4-cient-state>"
    p4 client -o
    echo "<p4-cient-state>"

Client spec is used to map paths of files in the build agent's workspace back to server paths, which can then be correlated with developer paths.

## Setup

Register plugin, then add to a job under the `LogParserPlugins` node.

    Plugins:
    -   Key: AcmeGamesBlamer
        Path: Wbtb.Extensions.PostProcessing.AcmeGamesBlamer

    Jobs:
    -   Key: MyJob
        PostProcessors: 
        -   MyCppParser