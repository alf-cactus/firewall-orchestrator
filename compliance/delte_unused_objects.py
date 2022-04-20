import json

#python_dictionary = json.loads('/home/alf/Downloads/mgm_id_5_config_native.json.anon')

def ruleContainsUID(uid, data):
    for rule in data['rulebases'][0]['layerchunks'][0]['rulebase']:
        for source in rule['source']:
            if source['uid'] == uid:
                return True
        for destination in rule['destination']:
            if destination['uid'] == uid:
                return True
        for service in rule['service']:
            if service['uid'] == uid:
                return True
        for installOn in rule['install-on']:
            if installOn['uid'] == uid:
                return True   

def natRuleContainsUID(uid, data):
    for index in range(5, 7, 1):
        for natrule in data['nat_rulebases'][1]['nat_rule_chunks'][0]['rulebase'][index]['rulebase']:
            if natrule['original-destination']['uid'] == uid:
                return True
            if natrule['translated-destination']['uid'] == uid:
                return True
            if natrule['original-source']['uid'] == uid:
                return True
            if natrule['translated-source']['uid'] == uid:
                return True
            if natrule['original-service']['uid'] == uid:
                return True
            if natrule['translated-service']['uid'] == uid:
                return True
            
with open('/home/alf/Downloads/mgm_id_5_config_native.json.anon') as json_file:
    data = json.load(json_file)
    dataOut = data
    #for headline in data['nat_rulebases'][1]['nat_rule_chunks'][0]['rulebase'][5]['rulebase'][2]:
    #    print(headline)
    #print(("uid", "0f45100c-e4ea-4dc1-bf22-74d9d98a4811") in data['rulebases'][0]['layerchunks'][0].items())
    i = 0
    for object_table in data['object_tables']:
        j = 0
        for object_chunk in object_table['object_chunks']:
            k = 0
            for object in object_chunk['objects']:
                #if ('uid', object['uid']) not in data['rulebase'] and ('uid', object['uid']) not in data['nat_rulebase']
                if not (ruleContainsUID(object['uid'], data) or natRuleContainsUID(object['uid'], data)):
                    #del data['object_tables'][object_table]['object_chunks'][object_chunk]['objects'][object]
                    #print(object)
                    #dataOut['object_tables'][object_table]['object_chunks'][object_chunk]['objects'].remove(object)
                    dataOut['object_tables'][i]['object_chunks'][j]['objects'].remove(object)
                    #dataOut['object_tables'][i]['object_chunks'][j]['objects'].pop(k)
                    #print(dataOut['object_tables'][i]['object_chunks'][j]['objects'])
                    #raise SystemExit(0)
                k = k+1
            j = j+1
        i = i+1
    
#print(dataOut)
with open('json_data.json', 'w') as outfile:
    json.dump(dataOut, outfile, indent=4, ensure_ascii=False)







#data['object_tables'][0]['object_chunks'][0]['objects'][0]
#                    hosts        objects 1-150       first object

#rulebases nur [0]
#nat_rulebases
#object_tables
#users