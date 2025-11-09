import json

with open("BingoSyncIntegration/objectives.json") as f:
    text = f.read()
    obj_list = json.loads(text)
    out_list = []
    for obj in obj_list:
        out_list.append({ "name": obj["name"] })
    print(json.dumps(out_list))