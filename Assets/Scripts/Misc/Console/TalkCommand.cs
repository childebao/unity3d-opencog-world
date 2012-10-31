using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TalkCommand : ConsoleCommand
{
    private string cmdname = "say";
	private GameObject player;
    public void Awake()
    {
		player = GameObject.FindGameObjectWithTag("Player");
	}

    public override string run(ArrayList arguments) {
        string text = string.Join(" ", arguments.ToArray(typeof(string)) as string[]);
        // Send the message to all avatars.
        foreach (GameObject go in OCARepository.GetAllOCA()) {
            // Don't send message to the player
            if (go.tag == "Player") continue;
            // Send message to OpenCog avatars
			OCConnector connection = go.GetComponent<OCConnector>();
			if (connection != null)
                connection.sendSpeechContent(text,player);
            // TODO: send the message to other human players using Unity RPC
        }
        // return Null because sendPredavese updates the log somehow..
        return null;
    }

    public override ArrayList getSignature() {
        // Unlimited strings allowed...
        KeyValuePair<Type,int> kt = new KeyValuePair<Type,int>(Type.GetType("string"),0);
        ArrayList sig = new ArrayList();
        sig.Add(kt);
        return sig;
    }
    
    public override string getName() {
        return cmdname;
    }

}

