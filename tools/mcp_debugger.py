#!/usr/bin/env python3
"""Small stdio MCP bridge to the running app's public command/state API."""
import argparse
import json
import sys
import urllib.request

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--endpoint',default='http://127.0.0.1:8787')
args=parser.parse_args()
TOOLS=[
 {'name':'app_state','description':'Read live frame, image, input, placement and diagnostic state','inputSchema':{'type':'object','properties':{},'additionalProperties':False}},
 {'name':'app_commands','description':'List available public application commands','inputSchema':{'type':'object','properties':{},'additionalProperties':False}},
 {'name':'app_events','description':'Read recent application diagnostics and errors','inputSchema':{'type':'object','properties':{},'additionalProperties':False}},
 {'name':'app_command','description':'Execute a named public command on the application main thread','inputSchema':{'type':'object','properties':{'name':{'type':'string'},'value':{'type':'number'},'flag':{'type':'boolean'},'text':{'type':'string'},'vector':{'type':'object','properties':{'x':{'type':'number'},'y':{'type':'number'},'z':{'type':'number'}}}},'required':['name'],'additionalProperties':False}}
]

def call(name,arguments):
 paths={'app_state':'/api/state','app_commands':'/api/commands','app_events':'/api/events'}
 if name=='app_command':request=urllib.request.Request(args.endpoint+'/api/command',data=json.dumps(arguments,allow_nan=False).encode(),headers={'Content-Type':'application/json'})
 elif name in paths:request=urllib.request.Request(args.endpoint+paths[name])
 else:raise ValueError('Unknown tool '+name)
 with urllib.request.urlopen(request,timeout=5) as response:return json.loads(response.read())

for raw in sys.stdin:
 try:
  message=json.loads(raw);identifier=message.get('id');method=message.get('method');params=message.get('params',{})
  if identifier is None:continue
  if method=='initialize':result={'protocolVersion':params.get('protocolVersion','2025-11-25'),'capabilities':{'tools':{}},'serverInfo':{'name':'soft-strange-debugger','version':'0.2.0'}}
  elif method=='ping':result={}
  elif method=='tools/list':result={'tools':TOOLS}
  elif method=='tools/call':
   try:
    data=call(params['name'],params.get('arguments',{}));result={'content':[{'type':'text','text':json.dumps(data)}],'structuredContent':data,'isError':isinstance(data,dict) and data.get('ok') is False}
   except Exception as error:result={'content':[{'type':'text','text':str(error)}],'isError':True}
  else:
   print(json.dumps({'jsonrpc':'2.0','id':identifier,'error':{'code':-32601,'message':'Method not found'}}),flush=True);continue
  print(json.dumps({'jsonrpc':'2.0','id':identifier,'result':result}),flush=True)
 except Exception as error:
  print(str(error),file=sys.stderr,flush=True)
