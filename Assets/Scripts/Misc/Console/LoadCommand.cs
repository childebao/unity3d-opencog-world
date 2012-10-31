using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LoadCommand : ConsoleCommand
{
    private string cmdname = "load";
    public GameObject NPCAvatar;

    public LoadCommand()
    { }

    public void setAvatarGameObject(GameObject avatarPrefab) {
        NPCAvatar = avatarPrefab;
    }

    public override string run(ArrayList arguments) {
        string avatarName = "";
        // Check whether we were given a name to call the avatar
        if (arguments.Count > 0) avatarName = (string) arguments[0];
        
		StartCoroutine(_doLoading(avatarName));
		
        return "Starting OAC named " + avatarName;
    }
    
	private IEnumerator _doLoading(string avatarName) {
		GameObject avatarClone;
		
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) {
            yield return "No object tagged with player.";
        }

        // Record the player's position and make the OCAvatar spawn near it.
		Vector3 playerPos = playerObject.transform.position;

        // Calculate the player's forward direction
        Vector3 eulerAngle = playerObject.transform.rotation.eulerAngles;

        float zFront = 3.0f * (float)Math.Cos((eulerAngle.y / 180) * Math.PI);
        float xFront = 3.0f * (float)Math.Sin((eulerAngle.y / 180) * Math.PI);

        // Instantiate an OCAvatar in front of the player.
        avatarClone = (GameObject) UnityEngine.Object.Instantiate( NPCAvatar,
                new Vector3(playerPos.x + xFront,
		                    playerPos.y + 2,
                            playerPos.z + zFront),
                Quaternion.identity);

        OCConnector connector = avatarClone.GetComponent("OCConnector") as OCConnector;
        
        if (avatarName == "") avatarName = createRandomAvatarName();
        
		avatarClone.name = avatarName;
        
        if (avatarClone != null) {
            if (!OCARepository.AddOCA(avatarClone)) {
				// An avatar with given name is already there.
				yield break;
			}
            Debug.Log("Add avatar[" + avatarName + "] to avatar map.");
        }
		// Get the player id as the master id of the avatar.
		// TODO Currently we use the tag "player". However, when there are multiple 
		// players in the world, we need to figure out a way to identify.
		string masterId = playerObject.GetInstanceID().ToString();
        string masterName = playerObject.name;
		
		// TODO Set agentType and agentTraits in the future.
		// leave agentType and agentTraits to null just for test.
		connector.Init(avatarName, null, null, masterId, masterName);

		yield return StartCoroutine(connector.connectOAC());
		
		if (!connector.IsInit()) {
			// OAC is not loaded normally, destroy the avatar instance.
			Debug.LogError("Cannot connect to the OAC, avatar loading failed.");
            connector.saveAndExit();
			Destroy(avatarClone);
			yield break;
		} 
	}

    private string createRandomAvatarName() {
        int randId = UnityEngine.Random.Range(1, 100);
        string[] baseNames = { "Ryu", "Tribble", "Boondock", "Magneto", "Blanca", "Wumpa" };
        int baseNameIndex = UnityEngine.Random.Range(0, baseNames.Length);
        return baseNames[baseNameIndex] + randId.ToString();
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

