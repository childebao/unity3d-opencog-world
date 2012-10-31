using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

/**
 * This console command lists all actions currently accessable by the ActionManager of
 * a given Avatar.
 */
public class ListActionsCommand : ConsoleCommand {

    private string cmdname = "list";

    private GameObject OCObjects;


    public ListActionsCommand()
    { 
    }

    public override string run(ArrayList arguments) {
		OCObjects = GameObject.Find("Objects") as GameObject;
        if (arguments.Count != 1) return "Wrong number of arguments";
        OCObjectRepository OCOR = OCObjectRepository.get();
        string avatarName = (string) arguments[0];
        // Get the appropriate avatar and gameobject
        GameObject avatarObject = OCARepository.GetOCA(avatarName);
        Avatar avatarScript = avatarObject.GetComponent("Avatar") as Avatar;
        ActionManager am = avatarScript.GetComponent("ActionManager") as ActionManager;
        Hashtable currentActions = am.currentActions.Clone() as Hashtable;
        string result = "";
        bool first = true;
        foreach (ActionKey ak in currentActions.Keys) {
            if (!first) {
                result += "\n";
            }
            first = false;
            GameObject OCObject = OCOR.GetOCObject(ak.objectID);
            if (OCObject == null) {
                OCObject = OCARepository.GetOCA(ak.objectID);
            }
            result += OCObject.name + " [" + ak.objectID + "]: " + ak.actionName;
        }
        
        return result;
    }

    public override ArrayList getSignature() {
        // Accepts one string as the NPC name
        KeyValuePair<Type,int> args = new KeyValuePair<Type,int>(Type.GetType("string"),2);
        ArrayList sig = new ArrayList();
        sig.Add(args);
        return sig;
    }
    
    public override string getName() {
        return cmdname;
    }

}
