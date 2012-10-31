using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UnloadCommand : ConsoleCommand
{
    private string cmdname = "unload";
    public UnloadCommand()
    { }

    public override string run(ArrayList arguments) {
        string message;
        if (arguments.Count == 0) {
            message = "Unload needs avatar name as argument";
            return message;
        }

        string avatarName = (string) arguments[0];
        
        // Get Avatar from OCARepository
        GameObject avatarToDestroy = OCARepository.GetOCA(avatarName);
		if (avatarToDestroy == null) return "No avatar called " + avatarName;

        avatarToDestroy.SendMessage("saveAndExit");
        OCARepository.RemoveOCA(avatarToDestroy);
        message = "Avatar[" + avatarName + "] successfully unloaded.";
        return message;
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

