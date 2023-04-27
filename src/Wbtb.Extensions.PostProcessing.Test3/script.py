# typical command looks like
# python <thisfile.py> --wbtb-message <message id> --tid <transaction id>
import argparse
import base64
import json
import sys
from urllib.request import urlopen
from importlib.machinery import SourceFileLoader

wbtb = SourceFileLoader('wbtb', './wbtb/wbtb.py').load_module()
DataLayer = wbtb.DataLayer
config = None

# define processor hjere
class PythonProcessor:

    def InitializePlugin(self):
        return { "Success" : True, "SessionId" : "test", "Description" : "123" }

    def InjectConfig(self, config):
        pass

    def Process(self):
        return { "Passed": True, "Result" : f"hello from python"}

parser = argparse.ArgumentParser()
parser.add_argument('--wbtb-message', required=True)
parser.add_argument('--wbtb-tid')
args = parser.parse_args()
messageid = args.wbtb_message

wbtb.logIncoming()
config = wbtb.getConfig()
wbtb.process(PythonProcessor(), messageid)

sys.exit(0)
