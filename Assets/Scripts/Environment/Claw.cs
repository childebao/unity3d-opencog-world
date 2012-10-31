using UnityEngine;
using System.Collections;

public class Claw : MonoBehaviour {
	
	
	private float pathPosition = 0.0f;
	
	// If this is non zero length, then the Claw will navigate around these points
	public Vector3[] path;
	
	public bool clawActiveOnStart = false;
	
	// If these are set then the claw will only move between these
	// (NOT IMPLEMENTED YET)
	public bool useTwoDestinations = false;
	public Vector3 destination1;
	public Vector3 destination2;
	
	// If this is set, then the claw will always move this object around
	// otherwise it's random
	public GameObject fixedPayload;
	
	// These are the gameobjects where the payload is stored while in transit.
	// They are different due to the Michael's initial animations using different bone structures
	// If he replaces them (which he should), these should collapse into the same slot.
	public GameObject pickupSlot;
	public GameObject payloadSlot;
		
	// This stores the payload object while it's being moved
	private GameObject payload;
	private GameObject lastPayload = null;
	
	private WorldGameObject theWorld;
	private float payloadHeight;
	
	private bool clawActive = false;
	
	public bool isClawActive {
		get { return clawActive; }
	}
	
	private Bounds clawBounds;
	public float clawHeight;
	
	// Use this for initialization
	void Start () {
		clawHeight = getClawHeightOffset();
		
		theWorld = GameObject.Find("World").GetComponent<WorldGameObject>();
		
		collectAnimations();
		
		if (clawActiveOnStart) StartCoroutine(NextAction(true));
	}
	
	Animation pickupAnim;
	Animation moveAnim;
	private float cfd = 0.1f;
	
	void collectAnimations() {
		// get animation script
		Animation[] anims = gameObject.GetComponentsInChildren<Animation>();
		foreach (Animation a in anims) {
			if (a.gameObject.name == "flyrobot_GrabBox") {
				pickupAnim = a;
				continue;
			} else if (a.gameObject.name == "polySurface1") {
				moveAnim = a;
			}
		}
	}
	
	float getClawHeightOffset() {
		// This calculates the difference between the transform position and the pickup point
		
		// Previous code was attempting to be exact and generic, but wasn't working very well
		// so now we just return an exact value
		return 2.3f;
		/*var meshes = transform.GetComponentsInChildren<MeshRenderer>();
		Bounds b = new Bounds();
		bool initBounds = true;
		foreach (MeshRenderer m in meshes) {
			// don't include the size of the payload slot
			if (m.gameObject == payloadSlot) continue;
			
			Debug.Log("mesh for claw object part " + m.gameObject + ", size is " + m.bounds);
			// the center of the bounds might not be centered on the parents position.
			if (initBounds) {
				initBounds = false;
				b = m.bounds;
			}
			b.Encapsulate(m.bounds);
			//float newY = (m.bounds.center - t.position).y + m.bounds.extents.y;
			//float newY = m.bounds.center.y + m.bounds.size.y;
			//if (newY > maxHeight) maxHeight = newY;
		}
		clawHeight = // transform.GetChild(0).transform.position.y - transform.position.y; //- b.center.y) + b.size.y;
		Debug.LogWarning("claw height was " + clawHeight + " from bounds " + b);
		clawBounds = b;
		Debug.LogWarning("claw transform position " + transform.position);
		Debug.LogWarning("pickup transform position " + pickupSlot.transform.position + " (local " + pickupSlot.transform.localPosition + ")");*/
		
	}
	
	void flyAroundFixedPath() {
		
		if (path == null || path.Length == 0) {
			path = new Vector3[5];
			path[0] = new Vector3(40,110,40);
			path[1] = new Vector3(50,110,40);
			path[2] = new Vector3(50,110,50);
			path[3] = new Vector3(40,110,50);
			path[4] = new Vector3(40,110,40);
		}
		// set initial position
		transform.position = path[0];
		transform.LookAt(path[1]);
		clawActive = true;
		iTween.MoveTo(gameObject,iTween.Hash("path",path,"orienttopath",true,"time",2,
		                                     "movetopath",true,"lookahead",0.02f,
		                                     "easetype","linear","oncomplete", "NextAction", "oncompleteparams", true));
	}
	
	// Update is called once per frame, and is mainly for moving the payload
	// Todo: should we move the payload into the claw transform instead?
	void Update () {
		//pathPosition += (0.1f * Time.deltaTime);
		//if (pathPosition > 1.0f) pathPosition = 0.0f;
		
	}
	
	IEnumerator NextAction(bool wait) {
		Vector3 theDestination;
		clawActive = true;
		if (wait) yield return new WaitForSeconds(2.0F);
		// In the waiting/pause time, the claw may have been disabled.
		if (!clawActive) yield break;
		
		GameObject selectedObject = getNextObject();
		if (selectedObject == null) {
			Debug.LogWarning("No valid objects found for Claw to pickup.");
			clawActive = false;
			yield break;
		}
			
		Vector3 dest = selectedObject.transform.position + new Vector3(0,VerticalSizeCalculator.getHeight(selectedObject.transform, null) - clawHeight,0);
		//Vector3 dest = selectedObject.transform.position + new Vector3(0,VerticalSizeCalculator.getHeight(selectedObject.transform, null) - ,0);
		//  -move to just above it
		
		Debug.Log("Claw destination is " + selectedObject + " position " + dest);
		
		// Play moving start animation
		moveAnim.CrossFade("start_fly_w_box",cfd);
		
		iTween.MoveTo(gameObject,iTween.Hash("position", dest,"speed",10,
		                                     "looktarget", selectedObject.transform, "axis", "y",
		                                     "easetype","linear","oncomplete", "PickupObject", "oncompleteparams", selectedObject));
		// wait for it to complete
		float waitTime = moveAnim["start_fly_w_box"].length - cfd;
		yield return new WaitForSeconds(waitTime);
		
		// Play moving idle animation
		moveAnim.CrossFade("idle_w_box",cfd);
		
	}
	
	GameObject getNextObject() {
		GameObject selectedObject = null;
		if (fixedPayload == null) {
			GameObject theObjects = GameObject.Find("Objects");
			int tries = 0; int max_tries = 10;
		
			while (tries < max_tries && (selectedObject == null || selectedObject == lastPayload)) {
				selectedObject = theObjects.transform.GetChild((int) UnityEngine.Random.Range(0.0f,theObjects.transform.GetChildCount()-0.01f)).gameObject;
				if (selectedObject.GetComponent<Picker>() == null) {
					selectedObject = null;
				}
				tries += 1;
			}
		} else {
			selectedObject = fixedPayload;
		}
		return selectedObject;
	}
	
	IEnumerator PickupObject(GameObject go) {
		// Stop moving animation
		moveAnim.CrossFade("end_fly_w_box",cfd);
		// wait for it to complete
		float waitTime = moveAnim["end_fly_w_box"].length - cfd;
		yield return new WaitForSeconds(waitTime);
		
		float animationSpeed = 1.0f;
		// first check that object isn't in anyone's inventory...
		foreach (Avatar a in GameObject.Find("Avatars").GetComponentsInChildren<Avatar>()) {
			if (a.inventory == go) {
				// Okay, an avatar has already picked up the object we wanted ... so we need to select something else to move...
				NextAction(true);
				yield return null;
			}
		}
		
		// play pick up (down) animation
		pickupAnim["pickup_down"].speed = animationSpeed;
		pickupAnim.Play("pickup_down");
		
		// wait for it to complete
		waitTime = pickupAnim["pickup_down"].length * (1.0f / animationSpeed);
		yield return new WaitForSeconds(waitTime);
		
		// disable physics and gravity
		go.rigidbody.isKinematic = true;
		// transfer object into pickupSlot
		go.transform.parent = pickupSlot.transform;
		go.transform.position = pickupSlot.transform.position;
		
		// play pick up (up) animation
		pickupAnim["pickup_up"].speed = animationSpeed;
		pickupAnim.Play("pickup_up");
		
		// wait for it to complete
		waitTime = pickupAnim["pickup_up"].length * (1.0f / animationSpeed);
		yield return new WaitForSeconds(waitTime);
		
		// transfer object into payloadSlot
		go.transform.parent = payloadSlot.transform;
		go.transform.position = payloadSlot.transform.position;
		
		
		//payloadHeight = VerticalSizeCalculator.getHeight(go.transform, null);
		payload = go;
		
		Vector3 dest = getNextDestination();
		// Make sure there's some ground beneath the destination point
		Vector3 groundDest = theWorld.getGroundBelowPoint(dest);
		if (groundDest == dest) { 
			Debug.LogError("No ground below destination point " + groundDest + " bounds was " + theWorld.bounds);
			yield return null;
			
		}
		
		groundDest += new Vector3(0.0f, 2.0f * clawHeight, 0.0f);
		
		Debug.Log("Moving object " + go + " to " + dest);
		
		// Play moving start animation
		moveAnim.Play("start_fly_w_box");
		
		// if both destinations occupied move to random destination
		// otherwise move to random point in the space
		iTween.MoveTo(gameObject,iTween.Hash("position", groundDest,"speed",10,
		                                     "looktarget", groundDest, "axis", "y",
		                                     "easetype","linear","oncomplete", "DropObject",
		                                     "oncompleteparams", true));
		// wait for it to complete
		waitTime = moveAnim["start_fly_w_box"].length;
		yield return new WaitForSeconds(waitTime);
		
		// Play moving idle animation
		moveAnim.Play("idle_w_box");
	}
	
	Vector3 getNextDestination() {
		if (useTwoDestinations) {
			// go to the furthest away of the two...
			if ( (transform.position - destination1).sqrMagnitude < (transform.position - destination2).sqrMagnitude) {
				return destination2;
			} else {
				return destination1;
			}
			
		} else {
			// Don't go to close to the boundary of the area...
			float boundaryError = 3.0f;
			return new Vector3(UnityEngine.Random.Range(theWorld.bounds.min.x+boundaryError,theWorld.bounds.max.x-boundaryError),
		                           theWorld.bounds.max.y+boundaryError,
		                           UnityEngine.Random.Range(theWorld.bounds.min.z+boundaryError,theWorld.bounds.max.z-boundaryError));
			return Vector3.zero;
		}
	}
	
	IEnumerator DropObject(bool nextAction = true) {
		// Play stop moving animation
		moveAnim.CrossFade("end_fly_w_box",cfd);
		// wait for it to complete
		float waitTime = moveAnim["end_fly_w_box"].length - cfd;
		yield return new WaitForSeconds(waitTime);
		
		// drop object at destination
		pickupAnim.CrossFade("drop",cfd);
		// wait for it to complete
		waitTime = pickupAnim["drop"].length - cfd;
		
		payload.transform.parent = GameObject.Find("Objects").transform;
		payload.rigidbody.isKinematic = false;
		lastPayload = payload;
		payload = null;
		
		yield return new WaitForSeconds(waitTime - cfd);
		
		
		moveAnim.CrossFade("hover_w_box",cfd);

		if (nextAction) {
			StartCoroutine(NextAction(true));
		}
	}
	
	void DropObject2() {
		// called by animation event in "drop" animation.
	}
	
	void OnDrawGizmos() {
		//iTween.DrawPath(path);
		
	}
	
	public void DisableTheClaw() {
		iTween.Stop(gameObject);
		if (payload != null)
			StartCoroutine(DropObject(false));
		clawActive = false;
	}
	
	public void RestartTheClaw() {
		if (!clawActive) {
			// Don't wait before starting up again... 
			StartCoroutine(NextAction(false)); 
		}
	}
}
