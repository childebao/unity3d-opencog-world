using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** Detector
 * A class that handles the interaction.
 */
[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]
public class Detector : MonoBehaviour {

    /// Set what object you want this Detector to detect
    public string[] DetectTags;

    /// A list stored all the Avators who is potentially able to interact
    public List<GameObject> Detection = new List<GameObject>();

    private Transform myParent;
    private Avatar myAvatar;
        
    public void  Start ()
    {
        myParent = transform.parent;
        myAvatar = myParent.GetComponent<Avatar>() as Avatar;
            
        if(collider) {
            if(collider.isTrigger == false) {
                Debug.Log("Please set the isTrigger of the Collider to be true!!");
            }
        } else {
            Debug.Log("Interaction requires a Trigger Collider");
        }
    }

    public void FixedUpdate()
    {
        transform.position = myParent.transform.position;
    }

    public void OnTriggerEnter(Collider enteredObj)
    {		
        // The player or an OpenCog agent ("Avatar") can interact with objects
        foreach(string child in DetectTags) {
            if(enteredObj.tag == child) {
                OCBehaviour OCB = enteredObj.GetComponent<OCBehaviour>();

                if (OCB != null) {
                    OCB.SendMessage("AddAction", myAvatar);
                    Detection.Add(enteredObj.gameObject);
                } else {
                    //Debug.LogWarning("Detector couldn't find OCBehaviour on detected object");
                }
            }
        }
    }

    public void OnTriggerExit (Collider enteredObj)
    {
        // The player or an OpenCog agent ("Avatar") can interact with objects
        foreach(string child in DetectTags) {
            if(enteredObj.tag == child) {
                OCBehaviour[] OCBs = enteredObj.GetComponents<OCBehaviour>();
                // Null checking.
                foreach(OCBehaviour OCB in OCBs)
				{
	                OCB.SendMessage("RemoveAction", myAvatar);
	                Detection.Remove(enteredObj.gameObject);
				}
            }
        }
    }
}
