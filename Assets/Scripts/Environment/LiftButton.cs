using UnityEngine;
using System.Collections;
using Embodiment;

public class LiftButton  : MonoBehaviour  {
	
	// By default the theButton is a child transform with name "TheLiftButton"
	public GameObject theButton = null;
	public Lift theLift = null;
	public bool isActive = false;
	public Material unactiveMaterial;
	public Material activeMaterial;	
	
	// Use this for initialization
	void Start () {
		if (theButton == null)
			theButton = GameObject.Find("TheLiftButton");
		
		if (theLift == null)
			theLift = GameObject.Find("TheLift").GetComponent<Lift>();		
		
		theButton.renderer.material = unactiveMaterial;
		
		StateChangesRegister.RegisterState(gameObject, this,"isActive" );
		
	}
	
    public void OnTriggerEnter(Collider enteredObj)
    {		
        // Objects that is not a avatar can interact with PressableObject
        if(enteredObj.tag == "OCObject" ) {
			if (!isActive)
			{
				isActive = true;
				theButton.renderer.material = activeMaterial;
				theLift.RestartTheLift();
			}
		}

    }

    public void OnTriggerExit (Collider enteredObj)
    {
        // Objects that is not a avatar can interact with PressableObjec
      
            if(enteredObj.tag == "OCObject" ) {
				
				isActive = false;
				theButton.renderer.material = unactiveMaterial;
				theLift.StopTheLift();
                
            }
        
    }

}
