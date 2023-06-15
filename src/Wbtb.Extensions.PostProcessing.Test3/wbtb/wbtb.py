# Single include required to create a WBTB Python plugin. Has no dependencies. Can be included directly 
# into your plugin script.
#
# How to use (basic)
#
# # 1) Import this module
# wbtb = ... <import this module>
#
# # 2) Get the message id passed to your plugin, here we're assuming you're using argparse
# messageid = args.wbtb_message
#
# # 3) Your plugin must declare some class that implements the inteface methods defined in WBTB.Common for that plugin type
# def MyPlugin:
#     def SomeMethod(arg1, arg2):   
#         pass # your method logic here
#
# # 4) Call process() with an instacnc of your PluginType and the messageid
# wbtb.process(MyPlugin(), messageid)


import json
from urllib.request import urlopen
import ast
import sys
import re as regex
import urllib.request

# todo : move to better config
port = 5001

def encodeOutput(payload):
    from datetime import datetime

    return f"<WBTB-output ts=\"{datetime.now()}\" pluginType=\"pyplugin\" >{json.dumps(payload)}</WBTB-output>",

def logIncoming():
    import sys
    import os 
    
    dir_path = os.path.dirname(os.path.realpath(__file__))
    dir_path = os.path.join(dir_path, 'log.txt')

    with open(dir_path, 'a') as f:
        f.write(f'{" ".join(sys.argv)}\n')

def downloadJson(url):
    from urllib.request import urlopen
    output = urlopen(url).read()
    output = output.decode('utf-8')
    message = json.loads(output)
    return message

def getConfig():
    # get config from server
    return downloadJson(f'http://localhost:{port}/api/v1/messagequeueconfig')

def process(processor, messageid):

    # get message 
    message = downloadJson(f'http://localhost:{port}/api/v1/messagequeue/{messageid}')


    functionName = message["FunctionName"]
    func = getattr(processor, functionName)
    result = None

    if message["Arguments"] == None or len(message["Arguments"]) == 0:
        result = func()
    else:
        args = []
        for argument in message["Arguments"]:
            args.append(ast.literal_eval(f'{argument["Value"]}'))

        result = func(*args)

    # convert object back to JSON, then to a base64 string
    result = encodeOutput(result)

    # return result via stdout
    print(result)
    sys.exit(0)

def invokeMethod(self, source, pluginKey, argumentsData):
    
    argumentsData["pluginKey"] = pluginKey
    argumentsJson = json.dumps(argumentsData)
    port = 5000 # todo : move to better config
    url = f'http://localhost:{port}/api/v1/invoke'

    req = urllib.request.Request(url)
    req.add_header('Content-Type', 'application/json; charset=utf-8')
    jsondata = json.dumps(argumentsData)
    jsondataasbytes = jsondata.encode('utf-8')
    req.add_header('Content-Length', len(jsondataasbytes))
    res = urllib.request.urlopen(req, jsondataasbytes)
    content = res.read().decode('utf-8')
    status = res.getcode()

    if status != 200:
        raise f"error posting to message broker {r}"

    replyJsonLookup = regex.search('<WBTB-output(.*)>([\S\s]*?)<\/WBTB-output>', content)

    if replyJsonLookup is None:
        raise ValueError(f"Could not parse result from message broker. received : {content}")

    replyJson = replyJsonLookup.group(2)
    replyData = json.loads(replyJson)
    return replyData




class DataLayer:

    def __init__(self, config):
        self.config = config

        # find plugin that satisifies IDataLayerPlugin requirement
        for plugin in config["Plugins"]:

            if plugin["Manifest"] != None and plugin["Manifest"]["Interface"] == "Wbtb.Core.Common.IDataLayerPlugin":
                self.pluginKey = plugin["Key"]
        
        if self.pluginKey == None:
            raise ValueError("Could not locate plugin that implements Wbtb.Core.Common.IDataLayerPlugin")

    def Verify():
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "Verify"
        })

    def VerInitializeDatastoreify():
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "InitializeDatastore"
        })

    def SaveBuildServer(self, buildServer):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveBuildServer",
            "Arguments" : [
                { 
                    "Name" : "buildServer", 
                    "Value" : buildServer 
                }
            ]
        })

    def GetBuildServerById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildServerById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetBuildServerByKey(self, key: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildServerByKey",
            "Arguments" : [
                { 
                    "Name" : "key", 
                    "Value" : key 
                }
            ]
        })

    def GetBuildServers(self):
        return InvokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildServers"
        })

    def DeleteBuildServer(self, record):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteBuildServer",
            "Arguments" : [
                { 
                    "Name" : "record", 
                    "Value" : record 
                }
            ]
        })

    def SaveSourceServer(self, sourceServer):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveSourceServer",
            "Arguments" : [
                { 
                    "Name" : "sourceServer", 
                    "Value" : sourceServer 
                }
            ]
        })

    def GetSourceServerById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetSourceServerById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetSourceServerByKey(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetSourceServerByKey",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetSourceServers(self):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetSourceServers"
        })

    def DeleteSourceServer(self, record):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteSourceServer",
            "Arguments" : [
                { 
                    "Name" : "record", 
                    "Value" : record 
                }
            ]
        })

    def SaveJob(self, job):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveJob",
            "Arguments" : [
                { 
                    "Name" : "job", 
                    "Value" : job 
                }
            ]
        })

    def GetJobById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetJobById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetJobByKey(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetJobByKey",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetJobs(self):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetJobByKey"
        })

    def GetJobsByBuildServerId(self, buildServerId: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetJobsByBuildServerId",
            "Arguments" : [
                { 
                    "Name" : "buildServerId", 
                    "Value" : buildServerId 
                }
            ]
        })

    def DeleteJob(self, job):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteJob",
            "Arguments" : [
                { 
                    "Name" : "job", 
                    "Value" : job 
                }
            ]
        })

    def GetIncidentIdsForJob(self, job):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetIncidentIdsForJob",
            "Arguments" : [
                { 
                    "Name" : "job", 
                    "Value" : job 
                }
            ]
        })

    def GetJobStats(self, job):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetJobStats",
            "Arguments" : [
                { 
                    "Name" : "job", 
                    "Value" : job 
                }
            ]
        })

    def ResetJob(self, jobId, hard: bool):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetJobStats",
            "Arguments" : [
                { 
                    "Name" : "jobId", 
                    "Value" : jobId 
                },
                { 
                    "Name" : "hard", 
                    "Value" : hard 
                }
            ]
        })

    def GetUserById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetUserById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetUserByKey(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetUserByKey",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def SaveUser(self, user):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveUser",
            "Arguments" : [
                { 
                    "Name" : "user", 
                    "Value" : user 
                }
            ]
        })

    def GetUsers(self):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetUsers"
        })

    def PageUsers(self, index: int, pageSize: int):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "PageUsers",
            "Arguments" : [
                { 
                    "Name" : "index", 
                    "Value" : index 
                },
                { 
                    "Name" : "pageSize", 
                    "Value" : pageSize 
                }                
            ]
        })

    def DeleteUser(self, record):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteUser",
            "Arguments" : [
                { 
                    "Name" : "record", 
                    "Value" : record 
                }
            ]
        })

    def SaveBuild(self, build):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveBuild",
            "Arguments" : [
                { 
                    "Name" : "build", 
                    "Value" : build 
                }
            ]
        })

    def GetBuildById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetBuildByKey(self, jobId: str, key: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildByKey",
            "Arguments" : [
                { 
                    "Name" : "jobId", 
                    "Value" : jobId 
                },
                { 
                    "Name" : "key", 
                    "Value" : key 
                }                
            ]
        })

    def PageBuildsByJob(self, jobId: str, index: int, pageSize: int):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "PageBuildsByJob",
            "Arguments" : [
                { 
                    "Name" : "jobId", 
                    "Value" : jobId 
                },
                { 
                    "Name" : "index", 
                    "Value" : index 
                },
                { 
                    "Name" : "pageSize", 
                    "Value" : pageSize 
                }                               
            ]
        })

    def PageIncidentsByJob(self, jobId: str, index: int, pageSize: int):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "PageIncidentsByJob",
            "Arguments" : [
                { 
                    "Name" : "jobId", 
                    "Value" : jobId 
                },
                { 
                    "Name" : "index", 
                    "Value" : index 
                },
                { 
                    "Name" : "pageSize", 
                    "Value" : pageSize 
                }                               
            ]
        })

    def PageBuildsByBuildAgent(self, hostname: str, index: int, pageSize: int):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "PageBuildsByBuildAgent",
            "Arguments" : [
                { 
                    "Name" : "hostname", 
                    "Value" : hostname 
                },
                { 
                    "Name" : "index", 
                    "Value" : index 
                },
                { 
                    "Name" : "pageSize", 
                    "Value" : pageSize 
                }                               
            ]
        })

    def DeleteBuild(self, record):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteBuild",
            "Arguments" : [
                { 
                    "Name" : "record", 
                    "Value" : record 
                }
            ]
        })

    def GetBuildsWithNoLog(self, job):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildsWithNoLog",
            "Arguments" : [
                { 
                    "Name" : "job", 
                    "Value" : job 
                }
            ]
        })

    def GetBuildsWithLogsAndNoInvolvements(self, job):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildsWithLogsAndNoInvolvements",
            "Arguments" : [
                { 
                    "Name" : "job", 
                    "Value" : job 
                }
            ]
        })

    def GetFailingBuildsWithoutIncident(self, job):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetFailingBuildsWithoutIncident",
            "Arguments" : [
                { 
                    "Name" : "job", 
                    "Value" : job 
                }
            ]
        })

    def GetLatestBuildByJob(self, job):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetLatestBuildByJob",
            "Arguments" : [
                { 
                    "Name" : "job", 
                    "Value" : job 
                }
            ]
        })

    def GetFirstPassingBuildAfterBuild(self, build):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetFirstPassingBuildAfterBuild",
            "Arguments" : [
                { 
                    "Name" : "build", 
                    "Value" : build 
                }
            ]
        })

    def GetPreviousBuild(self, build):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetPreviousBuild",
            "Arguments" : [
                { 
                    "Name" : "build", 
                    "Value" : build 
                }
            ]
        })

    def GetNextBuild(self, build):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetNextBuild",
            "Arguments" : [
                { 
                    "Name" : "build", 
                    "Value" : build 
                }
            ]
        })

    def GetUnparsedBuildLogs(self, job):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetUnparsedBuildLogs",
            "Arguments" : [
                { 
                    "Name" : "job", 
                    "Value" : job 
                }
            ]
        })

    def ResetBuild(self, buildId, hard):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "ResetBuild",
            "Arguments" : [
                { 
                    "Name" : "buildId", 
                    "Value" : buildId 
                },
                { 
                    "Name" : "hard", 
                    "Value" : hard 
                }                
            ]
        })

    def GetBuildsForPostProcessing(self, jobid: str, processorKey: str, limit: int):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildsForPostProcessing",
            "Arguments" : [
                { 
                    "Name" : "jobid", 
                    "Value" : jobid 
                },
                { 
                    "Name" : "processorKey", 
                    "Value" : processorKey 
                },
                { 
                    "Name" : "limit", 
                    "Value" : limit 
                }                                 
            ]
        })

    def SaveBuildFlag(self, flag):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveBuildFlag",
            "Arguments" : [
                { 
                    "Name" : "flag", 
                    "Value" : flag 
                }
            ]
        })

    def IgnoreBuildFlagsForBuild(self, build, flag):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "IgnoreBuildFlagsForBuild",
            "Arguments" : [
                { 
                    "Name" : "build", 
                    "Value" : build 
                },
                { 
                    "Name" : "flag", 
                    "Value" : flag 
                }                
            ]
        })

    def DeleteBuildFlagsForBuild(self, build, flag):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteBuildFlagsForBuild",
            "Arguments" : [
                { 
                    "Name" : "build", 
                    "Value" : build 
                },
                { 
                    "Name" : "flag", 
                    "Value" : flag 
                }                
            ]
        })

    def GetBuildFlagsForBuild(self, build):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildFlagsForBuild",
            "Arguments" : [
                { 
                    "Name" : "build", 
                    "Value" : build 
                }
            ]
        })

    def PageBuildFlags(self, index: int, pageSize: int):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "PageBuildFlags",
            "Arguments" : [
                { 
                    "Name" : "index", 
                    "Value" : index 
                },
                { 
                    "Name" : "pageSize", 
                    "Value" : pageSize 
                }                
            ]
        })

    def GetBuildFlagById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildFlagById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def DeleteBuildFlag(self, record):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteBuildFlag",
            "Arguments" : [
                { 
                    "Name" : "record", 
                    "Value" : record 
                }
            ]
        })

    def SaveBuildLogParseResult(self, buildLog):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveBuildLogParseResult",
            "Arguments" : [
                { 
                    "Name" : "buildLog", 
                    "Value" : buildLog 
                }
            ]
        })

    def GetBuildLogParseResultsByBuildId(self, buildId: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildLogParseResultsByBuildId",
            "Arguments" : [
                { 
                    "Name" : "buildId", 
                    "Value" : buildId 
                }
            ]
        })

    def DeleteBuildLogParseResult(self, record):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteBuildLogParseResult",
            "Arguments" : [
                { 
                    "Name" : "buildId", 
                    "Value" : buildId 
                }
            ]
        })

    def SaveBuildInvolement(self, buildInvolvement):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveBuildInvolement",
            "Arguments" : [
                { 
                    "Name" : "buildInvolvement", 
                    "Value" : buildInvolvement 
                }
            ]
        })

    def GetBuildInvolvementById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildInvolvementById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetBuildInvolvementByRevisionCode(self, jobId: str, revisionCode: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildInvolvementByRevisionCode",
            "Arguments" : [
                { 
                    "Name" : "jobId", 
                    "Value" : jobId 
                },
                { 
                    "Name" : "revisioncode", 
                    "Value" : revisioncode 
                }                
            ]
        })

    def DeleteBuildInvolvement(self, record):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteBuildInvolvement",
            "Arguments" : [
                { 
                    "Name" : "record", 
                    "Value" : record 
                }
            ]
        })

    def GetBuildInvolvementsByBuild(self, buildId: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildInvolvementsByBuild",
            "Arguments" : [
                { 
                    "Name" : "buildId", 
                    "Value" : buildId 
                }
            ]
        })

    def GetBuildInvolvementsWithoutMappedUser(self, jobId: str):
        return invokeMethod(self,
        {
            "FunctionName" : "GetBuildInvolvementsWithoutMappedUser",
            "Arguments" : [
                { 
                    "Name" : "jobId", 
                    "Value" : jobId 
                }
            ]
        })

    def GetBuildInvolvementsWithoutMappedRevisions(self, jobId: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildInvolvementsWithoutMappedRevisions",
            "Arguments" : [
                { 
                    "Name" : "jobId", 
                    "Value" : jobId 
                }
            ]
        })

    def PageBuildInvolvementsByUserAndStatus(self, userid: str, buildStatus, index: int, pageSize: int):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "PageBuildInvolvementsByUserAndStatus",
            "Arguments" : [
                { 
                    "Name" : "userid", 
                    "Value" : userid 
                },
                { 
                    "Name" : "buildStatus", 
                    "Value" : buildStatus 
                },
                { 
                    "Name" : "index", 
                    "Value" : index 
                },
                { 
                    "Name" : "pageSize", 
                    "Value" : pageSize 
                }                                 
            ]
        })

    def GetBuildInvolvementByUserId(self, userId: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildInvolvementByUserId",
            "Arguments" : [
                { 
                    "Name" : "userId", 
                    "Value" : userId 
                }
            ]
        })

    def GetBuildProcessorById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildProcessorById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def SaveBuildProcessor(self, buildProcessor):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveBuildProcessor",
            "Arguments" : [
                { 
                    "Name" : "buildProcessor", 
                    "Value" : buildProcessor 
                }
            ]
        })

    def GetBuildProcessorsByBuildId(self, buildId: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetBuildProcessorsByBuildId",
            "Arguments" : [
                { 
                    "Name" : "buildId", 
                    "Value" : buildId 
                }
            ]
        })

    def SaveRevision(self, revision):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveRevision",
            "Arguments" : [
                { 
                    "Name" : "revision", 
                    "Value" : revision 
                }
            ]
        })

    def GetRevisionById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetRevisionById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetRevisionByKey(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetRevisionByKey",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetNewestRevisionForBuild(self, buildId: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetNewestRevisionForBuild",
            "Arguments" : [
                { 
                    "Name" : "buildId", 
                    "Value" : buildId 
                }
            ]
        })

    def GetRevisionByBuild(self, buildId: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetRevisionByBuild",
            "Arguments" : [
                { 
                    "Name" : "buildId", 
                    "Value" : buildId 
                }
            ]
        })

    def DeleteRevision(self, revision):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteRevision",
            "Arguments" : [
                { 
                    "Name" : "revision", 
                    "Value" : revision 
                }
            ]
        })

    def GetRevisionsBySourceServer(self, sourceServerId: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteRevision",
            "Arguments" : [
                { 
                    "Name" : "revision", 
                    "Value" : revision 
                }
            ]
        })

    def SaveSession(self, session):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveSession",
            "Arguments" : [
                { 
                    "Name" : "session", 
                    "Value" : session 
                }
            ]
        })

    def GetSessionById(self, id: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetSessionById",
            "Arguments" : [
                { 
                    "Name" : "id", 
                    "Value" : id 
                }
            ]
        })

    def GetSessionByUserId(self, userid: str):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetSessionByUserId",
            "Arguments" : [
                { 
                    "Name" : "userid", 
                    "Value" : userid 
                }
            ]
        })

    def DeleteSession(self, session):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteSession",
            "Arguments" : [
                { 
                    "Name" : "session", 
                    "Value" : session 
                }
            ]
        })

    def AddConfigurationState(self, configurationState):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "DeleteSession",
            "Arguments" : [
                { 
                    "Name" : "session", 
                    "Value" : session 
                }
            ]
        })

    def GetLatestConfigurationState(self):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetLatestConfigurationState"
        })

    def GetLastJobDelta(self, jobId):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "GetLastJobDelta",
            "Arguments" : [
                { 
                    "Name" : "jobId", 
                    "Value" : jobId 
                }
            ]
        })

    def SaveJobDelta(self, build):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "SaveJobDelta",
            "Arguments" : [
                { 
                    "Name" : "build", 
                    "Value" : build 
                }
            ]
        })

    def PageConfigurationStates(self, index: int, pageSize: int):
        return invokeMethod(self, self.pluginKey,
        {
            "FunctionName" : "PageConfigurationStates",
            "Arguments" : [
                { 
                    "Name" : "index", 
                    "Value" : index 
                },
                { 
                    "Name" : "pageSize", 
                    "Value" : pageSize 
                }                
            ]
        })
    