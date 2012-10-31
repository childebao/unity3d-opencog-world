using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using Embodiment;
using System.Collections;



// We may eventually want to use a FPS controller that has physics:
//http://www.unifycommunity.com/wiki/index.php?title=RigidbodyFPSWalker

[RequireComponent (typeof (CharacterController))]
public class Player : Avatar {
    
    HUD theHUD;
    
    // For things selected with a click
    public Transform selectedIndicator;
    // For things that just get highlighted when we are looking at them
    public Transform highlightIndicator;
    // Is currently the camera view is first person view
    public bool isNowFPview = true; 
	
	public GameObject IceProjectile;
	public GameObject FireProjectile;
	
	public enum CharacterType : byte
	{	none
	,	girl
	,	ghost
	,	robot
	};
	
	public CharacterType characterType;
    
    GameObject lastSelected = null;
    GameObject lastHighlighted = null;
    Transform allObjects = null;
    public GameObject theMainCamera = null;
	public bool isAutoWalking = false;
            
    override public string agentType
    {
        get {
            return "player";
        }
    }
    
    void Awake() {
        Screen.lockCursor = true;
        Screen.showCursor = false;
        theHUD = GameObject.Find("HUD").GetComponent<HUD>();
        //theHUD.player = this;
        allObjects = GameObject.Find("Objects").transform;
		
		characterType = CharacterType.girl;
        
        if (theMainCamera == null)
            theMainCamera = GameObject.Find("Main_Camera");
        
    }
    
    // the skin the interact GUI will use
    public GUISkin mySkin;
    bool showGUI = true;
    public bool interactMenuVisible = false;
    
    // if to show the OCObject info 
    bool showOCObjectInfo = false;
	
	public HUD getTheHUD()
	{
		return theHUD;
	}
	
    void Update() {
        
        // While the interact button is pressed, display the interaction GUI
        // but only if the console isn't enabled
        if(Console.get() != null && !Console.get().isActive() && Input.GetKey(KeyCode.E)) {
            showGUI = true;
        } else {
            showGUI = false;
        } 
		
		// if the forcePanel is shown currently, we get the input to forcePanel,
		// not process others until the left mouse button released 
		if (theHUD.IsShowForcePanel())
		{
			if(Input.GetKeyDown(KeyCode.Mouse0))
			{
				theHUD.hideForcePanel();
			}
			return;
		}
		
        
        // If the console is not active, then a right click selects any object in the center
        // of the screen
        Vector3 thePoint = new Vector3(Camera.main.pixelWidth/2,Camera.main.pixelHeight/2,0);
        if(Input.GetKeyDown(KeyCode.Mouse0)) {
            if(Console.get().isActive()) {
                // If the console is active, then a right click selects an object whereever the cursor is
                thePoint = Input.mousePosition;
            }
        }
        
        // Determine what the character is looking at
        Ray ray = Camera.main.ScreenPointToRay (thePoint);
        // Draw a debug ray in the scene view
        // Increase the ray length from 10 to 40 because when use the Third Person view, 10 is not enough to hit the object looked at 
        Debug.DrawRay (ray.origin, ray.direction * 40, Color.yellow);
        
        // Dray a debug ray to show the player's forward direction
        Debug.DrawRay (gameObject.transform.position , gameObject.transform.forward * 15, Color.green);
        
        // If something gets hit, change the material so it appears selected
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast (ray.origin, ray.direction, out hit, 40)) {
            //Debug.Log("raycast hit: " + hit.transform.gameObject);
            GameObject o = hit.transform.gameObject; 
            // Don't highlight chunks from the gameworld made of cubes
            if (o.tag != "Chunk" && o.transform.IsChildOf(allObjects)) {
                // if renderer is null this is probably an avatar or something else special
                // otherwise we assume it's a proper
                if (lastHighlighted != o) {
                    // only change things if we are looking at something different
                    removeSpinner();
                    //Debug.Log("Changed highlighted object to " + o.name);
                    addSpinner(o);
					
                }
				if(Input.GetKeyDown(KeyCode.Mouse1) && !Console.get().isActive() && lastHighlighted!=null)
				{
					isAutoWalking = true;
					ArrayList args = new ArrayList();
					ActionTarget theObj = new ActionTarget(lastHighlighted.GetInstanceID(), EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
					args.Add(theObj);
					args.Add(1.0f);
					
					AM.doAction(gameObject.GetInstanceID(),"MoveToObject",args,actionComplete);
				}
            } else {
                removeSpinner();
            }
            
            // If the gameobject is an avatar then make it the currently selected avatar
            // for the information shown on the HUD
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                Avatar avatar = o.GetComponent<Avatar>();
                if (avatar != null)
                {
                    // o is Avatar or Player object
                    if (theHUD.selectedAvatar != avatar)
                    {
                        Debug.Log("Selecting avatar " + o.name);
                    }
                    else
                    {
                        // current selected avatar has already been selected at last round.
                        return;
                    }
                    theHUD.selectedAvatar = avatar.gameObject;
                    removeSelectedSpinner();
                    removeSpinner();
                    addSelectedSpinner(o);

                } else {
                    if (theHUD.selectedAvatar != null) {
                        Debug.Log("Selecting none (object was=" + o.name +")");
                        removeSelectedSpinner();
                    }
                    theHUD.selectedAvatar = null;
                    
                }
            }
        } else {
            // Also select no avatar if there was no object
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                removeSelectedSpinner();
            }
            removeSpinner();
        }
		
		WorldGameObject wgo = (WorldGameObject)GameObject.Find("World").GetComponent("WorldGameObject");
		
		//If there's no ground beneath us, move us back to a default position above the world.
		//@TODO: Change this to refer to the starting position somehow...
		if(wgo.getGroundBelowPoint(gameObject.transform.position) == gameObject.transform.position)
		{
			transform.position = new Vector3(wgo.WorldData.WidthInBlocks / 2, 120, wgo.WorldData.HeightInBlocks / 2);
		}
		
		if (Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1))
        {
            //m_World.RemoveBlockAt(blockHitPoint);
            ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 20.0f)) // increase the lenght of ray from 8.0 to 20 for the third person view
            {
				// Don't dig avatars
				if (hit.transform.tag == "Chunk") {
	                Vector3 hitPoint = hit.point + (ray.direction.normalized * 0.01f);
	                IntVect blockHitPoint = new IntVect((int)hitPoint.x, (int)hitPoint.z, (int)hitPoint.y);
					
					if(characterType == CharacterType.robot)
	                	wgo.world.Dig(new Vector3i(blockHitPoint.X, blockHitPoint.Y, blockHitPoint.Z), hitPoint);
					else if(characterType == CharacterType.girl)
					{
						// Create the projectile and start buring the block it should it
						//@TODO: Change this and similar cases so that burning depends the projectile collision...
						Instantiate(FireProjectile, transform.position + transform.forward * 2, Quaternion.identity);
						Fire_Projectile fireProjectile = (Fire_Projectile)FireProjectile.GetComponent("Fire_Projectile");
						fireProjectile.moveVector = ray.direction.normalized;
						wgo.world.Burn(blockHitPoint, hitPoint);
					}
				}
            }
        }
		
		if(Input.GetKey (KeyCode.Mouse1) && !Input.GetKey(KeyCode.Mouse0))
		{
			//m_World.RemoveBlockAt(blockHitPoint);
            ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 20.0f)) // increase the lenght of ray from 8.0 to 20 for the third person view
            {
				// Don't dig avatars
				if (hit.transform.tag == "Chunk") {
	                Vector3 hitPoint = hit.point + (ray.direction.normalized * 0.01f);
	                IntVect blockHitPoint = new IntVect((int)hitPoint.x, (int)hitPoint.z, (int)hitPoint.y);
					
					if(characterType == CharacterType.robot)
					{
						//Lift the block that collided with our ray
						//@NOTE: Should there be graphics connected here?
	                	wgo.world.Lift(blockHitPoint, hitPoint);
					}
					else if(characterType == CharacterType.girl)
					{
						// Create the ice particle and start freezing the block under the ray
						//@TODO: Fix this and similar cases so that they depend on the collision...
						Instantiate(IceProjectile, transform.position + transform.forward * 2, Quaternion.identity);
						Ice_Projectile iceProjectile = (Ice_Projectile)IceProjectile.GetComponent("Ice_Projectile");
						iceProjectile.moveVector = ray.direction.normalized;
						wgo.world.Freeze(blockHitPoint, hitPoint);
					}
				}
            }
		}
		
		if(Input.GetKey (KeyCode.Mouse1) && Input.GetKey(KeyCode.Mouse0))
		{
			//m_World.RemoveBlockAt(blockHitPoint);
            ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 20.0f)) // increase the lenght of ray from 8.0 to 20 for the third person view
            {
				// Don't dig avatars
				if (hit.transform.tag == "Chunk") {
	                Vector3 hitPoint = hit.point + (ray.direction.normalized * 0.01f);
	                IntVect blockHitPoint = new IntVect((int)hitPoint.x, (int)hitPoint.z, (int)hitPoint.y);
					
					if(characterType == CharacterType.girl)
					{
						// Start the steam effect
						//@NOTE: Should there be some graphics connected here?
						wgo.world.Steams(blockHitPoint, hitPoint);
					}
				}
            }
		}
		
    }
	
	public float resetHeight = 1.0f;
    
    void addSelectedSpinner(GameObject o) {
        Transform t = (Transform) Instantiate(selectedIndicator);
        t.parent = o.transform;
        t.name = "SelectionSpinner";
        lastSelected = o;
    }
    
    void removeSelectedSpinner() {
        GameObject o = lastSelected;
        if (o == null) return;
        Debug.Log("Remove spinner from object " + o);
        Transform t = o.transform.FindChild("SelectionSpinner");
        Debug.Log("Found transform t=" + t);
        if (t != null) GameObject.Destroy(t.gameObject);
        lastSelected = null;
    }
    
    void addSpinner(GameObject o) {
        Transform t = (Transform) Instantiate(highlightIndicator);
        t.parent = o.transform;
        t.name = "HighlightSpinner";
        lastHighlighted = o;
        if (o.tag == "OCObject")
        {
            OCBehaviour ocb = o.GetComponent("OCBehaviour") as OCBehaviour;
            showOCObjectInfo = true;
        }
    }
    
    void removeSpinner() {
        GameObject o = lastHighlighted;
        if (o == null) return;
        Debug.Log("Remove spinner from object " + o);
        Transform t = o.transform.FindChild("HighlightSpinner");
        Debug.Log("Found transform t=" + t);
        if (t != null) GameObject.Destroy(t.gameObject);
        if (o.tag == "OCObject")
        {
            OCBehaviour ocb = o.GetComponent("OCBehaviour") as OCBehaviour;
            showOCObjectInfo = false;
        }
        lastHighlighted = null;
    }
    
    public void setCharacterControl(bool t) {
        // Enable/disable the ability
        GameObject playerObject = gameObject;
        bool success = false;
        if (playerObject) {
            CharacterMotor cm = playerObject.GetComponent("CharacterMotor") as CharacterMotor;
            if (cm) {
                cm.canControl = t;
                success = true;
            }
//            MonoBehaviour mouseLookX = playerObject.GetComponent("MouseLook") as MonoBehaviour;
//            MonoBehaviour mouseLookY = playerObject.transform.Find("Main_Camera").GetComponent("MouseLook") as MonoBehaviour;
//            mouseLookX.enabled = t;
//            mouseLookY.enabled = t;

        }
        if (!success) {
            Debug.LogError("No GameObject with 'Player' tag to steal control from");
        }
        Screen.lockCursor = t;
        Screen.showCursor = !t;
    }
    
    void OnGUI() {
        
		if ( theHUD != null && theHUD.IsShowForcePanel())
			return;
		
        if (showGUI) {
            if (!interactMenuVisible) setCharacterControl(false);
            // Show the available actions in a radial menu
            
            // Find some variables of interest so that we don't calculate
            // them for every action.
            // centre of the screen:
            float centre_x = Camera.main.pixelWidth/2;
            float centre_y = Camera.main.pixelHeight/2;

            // filter out actions that shouldn't be player visible
            var playerActions = from x in this.AM.currentActions.Keys.Cast<ActionKey>()
                                where (this.AM.currentActions[x] as ActionSummary).playerVisible
                                select (this.AM.currentActions[x] as ActionSummary);

            // how far to split action buttons
            float degreeSplit = (Mathf.PI / 2.0f) / playerActions.Count();
            
            int i = 0;
            
            // radius from centre
            float radius = 100.0f;
            
            // Hacky copy of the ActionSummary list to avoid out of sync exceptions
            List<ActionSummary> playerActionsCopy = new List<ActionSummary>();
            foreach (ActionSummary action in playerActions) {
                playerActionsCopy.Add(action);
            }
            ///
            foreach (ActionSummary action in playerActionsCopy) {
                //GameObject actionObject = action.actionObject;
                // Start at 45 degrees and rotate
                float theta = (Mathf.PI / 4.0f) + (i * degreeSplit);
                float x = centre_x + (Mathf.Sin(theta) * radius);
                float y = centre_y - (Mathf.Cos(theta) * radius);
                if (GUI.Button (new Rect (x,y,180,20), action.actionObject.name + ":" + action.actionName)) {
                    //Debug.Log("you clicked on " + action.actionName);
                    
                    this.AM.doAction(action.getKey(), new ArrayList(), actionComplete);
                }
                i++;
                
            }

            interactMenuVisible = true;
        } else if (interactMenuVisible) {
            // interactMenuVisible is used to avoid calling this unnecesarily  
            //@TODO:Set new character controller
			setCharacterControl(true);
            interactMenuVisible = false;
        } else if (showOCObjectInfo)
        {
            if (lastHighlighted == null)
                return;
            if (lastHighlighted.tag != "OCObject")
                return;
            
            Color _textColor = GUI.contentColor;
            GUI.contentColor = Color.yellow;
            
            float x = Camera.main.pixelWidth/2 + 20;
            float y = Camera.main.pixelHeight/2 + 20;

            GUI.Label(new Rect(x, y, 200, 20), lastHighlighted.name + " : " + lastHighlighted.GetInstanceID());
            
            OCBehaviour[] ocbs =  lastHighlighted.GetComponents<OCBehaviour>();
            
            if (ocbs.Length != 0)
            {
                y += 5.0f;
                
                GUI.contentColor = Color.grey;
                y += 23.0f;
                GUI.Label(new Rect (x,y,200,20), "Action list:");
                GUI.contentColor = Color.green;
                foreach(OCBehaviour ocb in ocbs)
                {
                    foreach(string actionName in ocb.myActionList)
                    {
                        y += 23.0f;
                        GUI.Label(new Rect (x,y,200,20), actionName);
                        
                    }
                    
                }
            }
            GUI.contentColor = _textColor;
        }
		else
		{
			//@TODO:Set new character controller
			setCharacterControl(true);
		}
        
    
    }
    
    void actionComplete(ActionResult ar) 
    {
        Debug.Log("Action " + ar.action.actionName + " complete");
    }
    

}
