using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class SaveMapCommand : ConsoleCommand
{
    private string cmdname = "savemap";
    public SaveMapCommand()
    { }

    public override string run(ArrayList arguments) {
        string mapName = "tempmap.blocks";
        // Check whether we were given a name to call the avatar
        if (arguments.Count > 0) mapName = (string) arguments[0];
		
		if (saveWorld(mapName))
			return "Saved map to " + mapName;
		else
			return "Saving map to " + mapName + " failed";
		
    }

    private bool saveWorld(string filename) {
        // get the world game object
		// get the world data
		// and start the save process
		return GameObject.Find("World").GetComponent<WorldGameObject>().WorldData.saveData(filename);
		return false;
    }

    public override ArrayList getSignature() {
        // Accepts one string as the NPC name
        KeyValuePair<Type,int> kt = new KeyValuePair<Type,int>(Type.GetType("string"),1);
        ArrayList sig = new ArrayList();
        sig.Add(kt);
        return sig;
    }
    
    public override string getName() {
        return cmdname;
    }

}
