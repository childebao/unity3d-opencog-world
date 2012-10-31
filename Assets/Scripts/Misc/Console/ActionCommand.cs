using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class ActionCommand : ConsoleCommand
{
    private string cmdname = "do";

    public ActionCommand()
    {
	}

    public override string run(ArrayList arguments) {
        //if (arguments.Count != 2) return "Wrong number of arguments";
        string avatarName = (string) arguments[0];
        string objectName = (string) arguments[1];
        string actionName = (string) arguments[2];

        // Get the appropriate avatar and gameobject
        GameObject avatarObject = OCARepository.GetOCA(avatarName);
        Avatar avatarScript = avatarObject.GetComponent("Avatar") as Avatar;
        if (avatarScript == null)
            return "No Avatar script on avatar \"" + avatarName + "\".";
        if (avatarObject.tag == "Player")
            return "Avatar \"" + avatarName + "\" is a player!";

        // Get the object
        // if objectName is "self" then assume the script is on the avatar
        int actionObjectID;
        if (objectName != "self") {
            GameObject OCObjects = GameObject.Find("Objects") as GameObject;
			GameObject theActionObject = OCObjects.transform.FindChild(objectName).gameObject;
            if (theActionObject == null)
				return "No object called " + objectName;
			actionObjectID = theActionObject.GetInstanceID();
        } else {
            actionObjectID = avatarObject.GetInstanceID();
        }

        // Get the action summary from the Action Manager
        ActionManager AM = avatarScript.GetComponent<ActionManager>() as ActionManager;
        ActionSummary action = AM.getActionSummary(actionObjectID,actionName);
        if (action == null) return "No action called \"" + actionName + "\".";

        ParameterInfo[] pinfo = action.pinfo; //getFreeArguments();
        if (pinfo.Length > 0) {
            ArrayList args = new ArrayList();
            int i=0;
            int jmod=3;
            if (action.componentType == typeof(Avatar)) {
                // Check we don't have too many arguments
                if (pinfo.Length < arguments.Count - 3)
                    return "Expected " + pinfo.Length + " arguments, got " + (arguments.Count - 3) + ".";
            } else {
                // Check we don't have too many arguments, we don't need to
                // provide the avatar
                if (pinfo.Length - 1 < arguments.Count - 3)
                    return "Expected " + (pinfo.Length - 1) + " arguments, got " + (arguments.Count - 3) + ".";
                i=1;
                jmod=2;
            }
            // ignore last parameter if action uses a callback
            int lengthModififer = 0;
            if (action.usesCallback) lengthModififer = 1; 
            for (; i < pinfo.Length-lengthModififer; i++ ) {
                // do type checking and conversion from strings to the expected type
                // ignore 3 console arguments: the action name, avatar name,
                //    and the object with the action (from console arguments)
                int j = i+jmod;
                if (j>=arguments.Count) {
                    // Not enough arguments, so must be a default argument
                    if (!pinfo[i].IsOptional)
                        return "Missing parameter " + pinfo[i].Name + " is not optional.";
                    args.Add(pinfo[i].DefaultValue);
                } else {
                    arguments[j] = ((string)arguments[j]).Replace("\"","");
                    // Depending on the expected type we convert it differently
                    if (pinfo[i].ParameterType == typeof(GameObject)) {
                        // Parameters that are gameobjects... we just search for
                        // the name.
                        args.Add(GameObject.Find((string) arguments[j]));
						if (((GameObject) args[i]) == null) {
						    return "No game object called \"" + (string) arguments[j]  + "\".";
						}
                    } else if (pinfo[i].ParameterType == typeof(Avatar)) {
                        // Parameters that are Avatars... we just search for
                        // the name.
                        args.Add(OCARepository.GetOCA((string)arguments[j]).GetComponent("Avatar") as Avatar);
                        if ((Avatar)args[i] == null) {
                            return "No Avatar called \"" + (string) arguments[j]  + "\".";
                        }
                    } else if (pinfo[i].ParameterType == typeof(int)) {
                        try {
                            args.Add(System.Int32.Parse((string) arguments[j]));
                        } catch (System.FormatException ex) {
                            return "Error parsing string as int32: " + (string) arguments[j];
                        }
                    } else if (pinfo[i].ParameterType == typeof(float)) {
                        try {
                            args.Add(float.Parse((string) arguments[j]));
                        } catch (System.FormatException ex) {
                            return "Error parsing string as float: " + (string) arguments[j];
                        }
                    } else if (pinfo[i].ParameterType == typeof(string)) {
                        args.Add((string) arguments[j]);
                    } else {
                        return "Method " + actionName + " at slot " + i + " has argument of unsupported type \"" + pinfo[i].ParameterType +
                            "\". Ask Joel how to implement support or ask him nicely to do it ;-).";
                    }
                }
            }
			// even if this action supports callbacks, we don't pass our actionComplete callback because
			// it's already called by the global event (which the Console class listens for).
            AM.doAction(actionObjectID,actionName,args);
        }

        return "Told avatar \"" + avatarName + "\" to do action \"" + actionName + "\"";
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

