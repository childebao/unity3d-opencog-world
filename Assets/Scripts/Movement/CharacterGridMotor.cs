using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(CharacterController))]

/// <summary>
/// Character grid motor.
/// </summary>
public class CharacterGridMotor : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	public GameObject indicator;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	private Vector3 moveVec = Vector3.forward;
	private float gravityForce = 15.0f;
	// private float jumpForce = 6.3;
	
	public CharacterController controller;
	private Hashtable fwd_HT;
	private Hashtable turnL_HT;
	private Hashtable turnR_HT;
	private Hashtable createBlockHT;
	
	private Hashtable climb_HT;
	private bool isClimbing = false;
	private Block block;
	private string state = "normal";
	private bool m_lock = false;
	
	private RaycastHit Hit;
	private Vector3 frontDir;
	
	private GameObject world;
	//private static int s = 8;
	//private static int[,,] chkArr = new int[s,s,s];
	private static Vector3 sum;
	
	private string currChar = "Player";
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	// ---------------------------------------------------------------------------------------------
	public void playWalk() 
	{
		//Debug.Log("In Play Walk...");
		animation.CrossFade("walk", 0.0f);
		//animation.Play("walk");
	}
	
	public void playJump() 
	{
		animation.CrossFade("jump", 0.1f);
	}
	
	public void playClimb() 
	{
		animation.CrossFade("climb", 0.1f);
	}
	
	public void playTurnR() 
	{
		//Debug.Log("In Turn Right...");
		animation.CrossFade("turnR", 0.1f);
		//animation.Play("turnR");
	}
	
	public void playTurnL() 
	{
		//Debug.Log("In Turn Left...");
		animation.CrossFade("turnL", 0.1f);
		//animation.Play("turnL");
	}
	
	public void playIdle() 
	{
		animation.CrossFade("idle", 0.2f);
	}
	
	public void playIdleState(string s) 
	{
	
		animation.CrossFade("idle_" + s, 0.2f);
		// animation.CrossFade("idle", 0.2);
	}
	
	public void playCreateBlock() 
	{
		animation.CrossFade("createBlock", 0.1f);
	}
	
	public void playComplete() 
	{
	
		isClimbing = false;
		m_lock = false;
	}
	
	// ---------------------------------------------------------------------------------------------
	
	public void Start() {
	
		animation["idle"].wrapMode = WrapMode.Loop;
		animation["idle"].layer = -1;
		
		animation["walk"].wrapMode = WrapMode.Once;
	 	animation["walk"].speed = 2.0f;
		animation["walk"].layer = 0;
	
	 	animation["turnR"].wrapMode = WrapMode.Once;
	 	animation["turnR"].speed = 2.0f;
		animation["turnR"].layer = 1;
	
	 	animation["turnL"].wrapMode = WrapMode.Once;
	 	animation["turnL"].speed = 2.0f;
		animation["turnL"].layer = 1;
	
	//  	animation["jump"].wrapMode = WrapMode.Loop;
	//  	animation["jump"].speed = 2;
	
	 	animation["climb"].wrapMode = WrapMode.Once;
	 	animation["climb"].speed = 1.0f;
	
	 	if (this.gameObject.name == "Robot") {
	
		 	animation["destroyBlockD"].wrapMode = WrapMode.Once;
		 	animation["destroyBlockD"].speed = 1.0f;
		 	animation["destroyBlockD"].layer = 2;
	
		 	animation["destroyBlockM"].wrapMode = WrapMode.Once;
		 	animation["destroyBlockM"].speed = 1.0f;
		 	animation["destroyBlockM"].layer = 2;
	
		 	animation["destroyBlockU"].wrapMode = WrapMode.Once;
		 	animation["destroyBlockU"].speed = 1.0f;
		 	animation["destroyBlockU"].layer = 2;
	
			animation["createBlock"].wrapMode = WrapMode.Once;
		 	animation["createBlock"].speed = 1.0f;
	
			createBlockHT = new Hashtable();
			createBlockHT.Add("y",0);
			createBlockHT.Add("time",animation["createBlock"].length);
			createBlockHT.Add("delay",0);
			createBlockHT.Add("onstart","playCreateBlock");
			createBlockHT.Add("oncomplete", "playComplete");
		}
	
		if (this.gameObject.name == "Girl") {
	
			animation["magicFire"].wrapMode = WrapMode.Once;
		 	animation["magicFire"].speed = 1.0f;
		 	animation["magicFire"].layer = 2;
	
			animation["magicIce"].wrapMode = WrapMode.Once;
		 	animation["magicIce"].speed = 1.0f;
		 	animation["magicIce"].layer = 2;
	
		 }
	
		 if ( (this.gameObject.name == "Girl") || (this.gameObject.name == "Robot") || (this.gameObject.name == "Ghost") ) {
	 	  	animation["idle_happy"].wrapMode = WrapMode.Loop;
		 	animation["idle_angry"].wrapMode = WrapMode.Loop;
		 	animation["idle_frightened"].wrapMode = WrapMode.Loop;
		 	animation["idle_sad"].wrapMode = WrapMode.Loop;
		  	animation["idle_excited"].wrapMode = WrapMode.Loop;
		 	animation["idle_confused"].wrapMode = WrapMode.Loop;
		  	animation["idle_disgusted"].wrapMode = WrapMode.Loop;
	  	}
	
		animation.Stop();
		//controller = (CharacterController)GetComponent (typeof(CharacterController));
	
//		if(controller != null)
//			Debug.Log("In CharacterGridMotor.Start(), controller is: " + controller.ToString());
		
		fwd_HT = new Hashtable();
		fwd_HT.Add("z",1);
		fwd_HT.Add("time",animation["walk"].length / animation["walk"].speed + 0.01);
		fwd_HT.Add("easetype", "linear");
		fwd_HT.Add("delay",0);
		fwd_HT.Add("onstart", "playWalk");
		fwd_HT.Add("oncomplete", "playComplete");
		
//		Debug.Log ("fwd_HT: " + fwd_HT.ToString() + ", time: " + (animation["walk"].length / animation["walk"].speed + 0.01).ToString());
	
		climb_HT = new Hashtable();
		climb_HT.Add("z",1);
		climb_HT.Add("y",1);
		climb_HT.Add("time",animation["climb"].length / animation["climb"].speed + 0.01);
		climb_HT.Add("easetype", "linear");
		climb_HT.Add("delay",0);
		climb_HT.Add("onstart", "playClimb");
		climb_HT.Add("oncomplete", "playComplete");
		
//		Debug.Log ("climb_HT: " + climb_HT.ToString() + ", time: " + (animation["climb"].length / animation["climb"].speed + 0.01).ToString());
	
		turnL_HT = new Hashtable();
		turnL_HT.Add("y",-0.25);	// -90 deg
		turnL_HT.Add("time",animation["turnL"].length / animation["turnL"].speed + 0.02);
		turnL_HT.Add("easetype", "linear");
		turnL_HT.Add("delay",0);
		turnL_HT.Add("onstart","playTurnL");
		turnL_HT.Add("oncomplete", "playComplete");
		
//		Debug.Log ("turnL_HT: " + turnL_HT.ToString() + ", time: " + (animation["turnL"].length / animation["turnL"].speed + 0.02).ToString());
	
		turnR_HT = new Hashtable();
		turnR_HT.Add("y",0.25);		// 90 deg
		turnR_HT.Add("time",animation["turnR"].length / animation["turnR"].speed + 0.02);
		turnR_HT.Add("easetype", "linear");
		turnR_HT.Add("delay",0);
		turnR_HT.Add("onstart","playTurnR");
		turnR_HT.Add("oncomplete", "playComplete");
		
//		Debug.Log ("turnR_HT: " + turnR_HT.ToString() + ", time: " + (animation["turnR"].length / animation["turnR"].speed + 0.02).ToString());
	
		world = GameObject.FindGameObjectWithTag("world");		// Add tag "block" in the scene in Unity !
	}
	
	// ---------------------------------------------------------------------------------------------
	
//	public bool isInValidRange(Vector3 p) 
//	{
//	
//		if (
//			(-1 < p.x) && (p.x <  s) &&
//			(-1 < p.y) && (p.y <= s) &&
//			(-1 < p.z) && (p.z <  s)
//		)
//			return true;
//		else
//			return false;
//	}
	
	// ---------------------------------------------------------------------------------------------
	
	public void attachFireToHand() 
	{
	
		// Attach flame to left hand
		GameObject magic = (GameObject)Instantiate(Resources.Load("Magic/handMagicFlame_fire"));
		GameObject leftHand = GameObject.FindGameObjectWithTag("girlQ_leftHand");
	
		magic.transform.parent = leftHand.transform;
		magic.transform.localPosition = Vector3.zero;
	
		Destroy(magic,2);
	}
	
	public void spawnTestFireAtBlockLocation(Vector3 blockLocation)
	{
		GameObject testFire = (GameObject)Instantiate(Resources.Load("Magic/handMagicFlame_fire"));
		testFire.transform.position = blockLocation;
		
		Destroy (testFire, 10);
	}
	
		public void spawnTestIceAtBlockLocation(Vector3 blockLocation)
	{
		GameObject testIce = (GameObject)Instantiate(Resources.Load("Magic/handMagicFlame_ice"));
		testIce.transform.position = blockLocation;
		
		Destroy (testIce, 2);
	}
	
	// ---------------------------------------------------------------------------------------------
	
	public void attachIceToHand() 
	{
	
		// Attach flame to left hand
		GameObject magic = (GameObject)Instantiate(Resources.Load("Magic/handMagicFlame_ice"));
		GameObject leftHand = GameObject.FindGameObjectWithTag("girlQ_rightHand");
	
		magic.transform.parent = leftHand.transform;
		magic.transform.localPosition = Vector3.zero;
	
		Destroy(magic,2);
	}
	
	// ---------------------------------------------------------------------------------------------
	
	public void Update () {
	
		//Debug.Log("In CharacterGridMotor.Update()... A");
		
		if (currChar == this.gameObject.name) 
		{
	
			if (indicator != null) {
		// 		var root = gameObject.Find(this.gameObject.name + "/RootJ");
		// 		indicator.transform.position = root.transform.position;
				indicator.transform.position = this.gameObject.transform.position;
	
				float thisY = this.gameObject.transform.position.y;
	
				if (this.gameObject.name == "Girl") thisY -= 0.6602f;
				if (this.gameObject.name == "Robot") thisY -= 1.266f;
				if (this.gameObject.name == "Ghost") thisY -= 1.185f;
				if (this.gameObject.name == "Player") thisY -= 0.2834f;
	
				indicator.transform.position.Set(indicator.transform.position.x, Mathf.RoundToInt(thisY) + 0.03f, indicator.transform.position.z);
			}
		}
	
		frontDir = transform.TransformDirection(Vector3.forward);	// process every frame
	
		if (Input.GetKeyDown ("q")) { currChar = "Player"; }
		if (Input.GetKeyDown ("w")) { currChar = "Ghost"; }
		if (Input.GetKeyDown ("e")) { currChar = "Girl"; }
		if (Input.GetKeyDown ("r")) { currChar = "Robot"; }
	
//		if(controller == null)
//			Debug.Log("In CharacterGridMotor: Update, controller is null...");
		
		//Debug.Log("In CharacterGridMotor.Update()... B");
		
		WorldGameObject wgo = (world.GetComponent("WorldGameObject") as WorldGameObject);
		
		if ( controller.isGrounded && (!m_lock) && (currChar == this.gameObject.name) ) {
	
			//transform.position = new Vector3((int)transform.position.x + 0.5f, (int)transform.position.y + 0.5f, (int)transform.position.z + 0.5f);
			Vector3 charPos = transform.position;// - new Vector3(0.5f, 0, 0.5f)
	
			if (this.gameObject.name == "Girl") charPos.y -= 0.6602f;
			if (this.gameObject.name == "Robot") charPos.y -= 1.266f;
			if (this.gameObject.name == "Ghost") charPos.y -= 1.185f;
			if (this.gameObject.name == "Player") charPos.y -= 0.2834f;
	
			// Debug.DrawRay(this.gameObject.transform.position + Vector3(0,0.5,0), frontDir, Color.yellow);
	
//			frontDir.x = Mathf.RoundToInt(frontDir.x);
//			frontDir.y = Mathf.RoundToInt(frontDir.y);
//			frontDir.z = Mathf.RoundToInt(frontDir.z);
//	
//			charPos.x = Mathf.RoundToInt(charPos.x);
//			charPos.y = Mathf.RoundToInt(charPos.y);
//			charPos.z = Mathf.RoundToInt(charPos.z);
			
			Vector3 position_above = charPos + 0.5f*Vector3.up;
			Vector3 ground_below = wgo.getGroundBelowPoint(position_above);
			if(ground_below == position_above)
			{
				//ground_below = Vector3.zero;
			}
			
			sum = charPos + frontDir;
			Vector3 position_above_front = sum + 0.5f*Vector3.up;
			Vector3 ground_front = wgo.getGroundBelowPoint(position_above_front);
			if(ground_front == position_above_front)
			{
				ground_front = Vector3.zero;
			}
			
			Vector3 position_above_above_front = sum + 1.5f*Vector3.up;
			Vector3 ground_above_front = wgo.getGroundBelowPoint(position_above_above_front);
			if(ground_above_front == position_above_above_front)
			{
				ground_above_front = Vector3.zero;
			}
			
			Vector3 climb_test = ground_above_front - ground_below;
			Vector3 walk_test = ground_front - ground_below;
					
			climb_test = Vector3.Project(climb_test, Vector3.up);
			walk_test = Vector3.Project(walk_test, Vector3.up);
			
			climb_test.Normalize();
			//walk_test.Normalize();
	
			if (world == null) 
			{
				//Debug.Log("In CharacterGridMotor.Update()... C");
				if (Input.GetKey ("up")) 
				{
					//Debug.Log("up");
		 			m_lock = true;
		 			iTween.MoveBy(this.gameObject, fwd_HT);
	 			}
	 			else if (Input.GetKey ("left")) 
				{
					//Debug.Log("left");
					m_lock = true;
					iTween.RotateBy(this.gameObject, turnL_HT);
				}
				else if (Input.GetKey ("right")) 
				{
					//Debug.Log("right");
					m_lock = true;
					iTween.RotateBy(this.gameObject, turnR_HT);
				}
			}
			else {
	
				
				
	
				if (Input.GetKey ("up")) {
	
					//if (!isInValidRange(sum)) {
					//	Debug.Log("Out of bound");
					//}
					//else {
						// X - character position
						// A - there is block at lower front
						// B - there is block at middle front
						// C - there is block at upper front
						// D - there is block below character
						// E - there is block below A
						// F - there is block below E
						//
						//        | C |
						//        -----
						//        | B |
						//        -----
						//    | X | A |
						//    ---------
						//    | D | E |
						//        -----
						//        | F |
	
//						int A = 0;
//						int B = 0;
//						int C = 0;
//						int D = 0;
//						int E = 0;
//						int F = 0;
//	
//						A = chkArr[(int)sum.x, (int)charPos.y, (int)sum.z];
//						if (isInValidRange(sum + new Vector3(0.0f, 1.0f,0.0f))) B = chkArr[(int)sum.x, (int)charPos.y + 1, (int)sum.z];
//						if (isInValidRange(sum + new Vector3(0.0f, 2.0f,0.0f))) C = chkArr[(int)sum.x, (int)charPos.y + 2, (int)sum.z];
//						if (isInValidRange(charPos + new Vector3(0.0f,-1.0f,0.0f))) D = chkArr[(int)charPos.x, (int)charPos.y -1, (int)charPos.z];
//						if (isInValidRange(sum + new Vector3(0.0f,-1.0f,0.0f))) E = chkArr[(int)sum.x, (int)charPos.y -1, (int)sum.z];
//						if (isInValidRange(sum + new Vector3(0.0f,-2.0f,0.0f))) F = chkArr[(int)sum.x, (int)charPos.y -2, (int)sum.z];
	
						
						
						//climb_test.Normalize();
						
						if (ground_below == Vector3.zero) {
							Debug.Log("No Ground Below:");
							Debug.Log(climb_test.ToString() + ", " + ground_above_front.ToString() + ", " + ground_below.ToString());
						}
						else if (ground_front == Vector3.zero && ground_above_front == Vector3.zero) {
							Debug.Log("No Ground In Front");
						}
						else if (climb_test.y > 0.0f && climb_test.y <= 1.0f && ground_above_front != ground_front) {
							//Debug.Log("In Climb");
							//if (C == 0) {	// CLIMB
								m_lock = true;
								isClimbing = true;
								//spawnTestIceAtBlockLocation(ground_above_front);
								iTween.MoveBy(this.gameObject, climb_HT);
								//Debug.Log(climb_test.ToString() + ", " + ground_above_front.ToString() + ", " + ground_front.ToString() + ", " + ground_below.ToString());
							//}
							//else {
							//}
						}
						//@TODO: Make real ray-casts to solve find if there's blocks in front which are blocking the character
						else if(ground_below != Vector3.zero && ground_front != Vector3.zero)
						{
							//Debug.Log("In Forward");
							m_lock = true;
							//spawnTestIceAtBlockLocation(ground_front);
							iTween.MoveBy(this.gameObject, fwd_HT);
						}
					//}
				}
				else if (Input.GetKey ("left")) {
					//Debug.Log("In Left");
					m_lock = true;
					iTween.RotateBy(this.gameObject, turnL_HT);
				}
				else if (Input.GetKey ("right")) {
					//Debug.Log ("In Right");
					m_lock = true;
					iTween.RotateBy(this.gameObject, turnR_HT);
				}
			}
	
			if (world != null) {
	
				if(currChar == "Robot")
				{
					// Create block below at B
					if (Input.GetKeyDown("t")) {
		
						//if (isInValidRange(sum)) {
							iTween.MoveBy(this.gameObject, createBlockHT);
							m_lock = true;
		
							//wgo.createBlock(sum.x,sum.y+1,sum.z, 0.6);
							//wgo.world.GenerateBlockAt(new IntVect((int)sum.x, (int)sum.y+1, (int)sum.z), BlockType.Stone);
							spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y+1+0.5f, (int)sum.z+0.5f));
						//}
					}
		
					// Create block below at A
					if (Input.GetKeyDown("g")) {
		
						//if (isInValidRange(sum)) {
							iTween.MoveBy(this.gameObject, createBlockHT);
							m_lock = true;
		
							//wgo.createBlock(sum.x,sum.y,sum.z, 0.6);
							//wgo.world.GenerateBlockAt(new IntVect((int)sum.x, (int)sum.y, (int)sum.z), BlockType.Stone);
							spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y+0.5f, (int)sum.z+0.5f));
						//}
					}
		
					// Create block below at E
					if (Input.GetKeyDown("b")) {
		
						//if (isInValidRange(sum)) {
							if (ground_front == ground_above_front && climb_test.y > 0) {	// !A
		
								iTween.MoveBy(this.gameObject, createBlockHT);
								m_lock = true;
		
								//wgo.createBlock(sum.x,sum.y-1,sum.z, 0.6);
								//wgo.world.GenerateBlockAt(new IntVect((int)sum.x, (int)sum.y-1, (int)sum.z), BlockType.Stone);
								spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y-1+0.5f, (int)sum.z+0.5f));
							}
						//}
					}
				}
	
				if (currChar == "Girl") {
	
					// Change block type of block at B
					if (Input.GetKeyDown("u")) {
						//if (isInValidRange(sum)) {
							animation.CrossFade("magicFire", 0.1f);
							//wgo.changeBlockType(sum.x,sum.y+1,sum.z,3,1.2);
							//wgo.world.RemoveBlockAt(new Vector3i((int)sum.x,(int)sum.y+1,(int)sum.z));
							//wgo.world.GenerateBlockAt(new IntVect((int)sum.x,(int)sum.y+1,(int)sum.z), BlockType.Lava);
							spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y+1+0.5f, (int)sum.z+0.5f));
							attachFireToHand();
						//}
					}
	
					// Change block type of block at A
					if (Input.GetKeyDown("j")) {
						//if (isInValidRange(sum)) {
							animation.CrossFade("magicFire", 0.1f);
							//wgo.changeBlockType(sum.x,sum.y,sum.z,3,1.2);
							//wgo.world.RemoveBlockAt(new Vector3i((int)sum.x,(int)sum.y,(int)sum.z));
							//wgo.world.GenerateBlockAt(new IntVect((int)sum.x,(int)sum.y,(int)sum.z), BlockType.Lava);
							spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y+0.5f, (int)sum.z+0.5f));
							attachFireToHand();
						//}
					}
	
					// Change block type of block at E
					if (Input.GetKeyDown("m")) {
	
						//if (isInValidRange(sum)) {
							if (ground_front == ground_above_front && climb_test.y > 0) {	// !A
								animation.CrossFade("magicFire", 0.1f);
								//wgo.changeBlockType(sum.x,sum.y-1,sum.z,3,1.2);
								//wgo.world.RemoveBlockAt(new Vector3i((int)sum.x,(int)sum.y-1,(int)sum.z));
								//wgo.world.GenerateBlockAt(new IntVect((int)sum.x,(int)sum.y-1,(int)sum.z), BlockType.Lava);
								spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y-1+0.5f, (int)sum.z+0.5f));
								attachFireToHand();
							}
						//}
					}
	
					// Change block type of block at B
					if (Input.GetKeyDown("i")) {
						//if (isInValidRange(sum)) {
							animation.CrossFade("magicIce", 0.1f);
							//wgo.changeBlockType(sum.x,sum.y+1,sum.z,4,1.2);
							//wgo.world.RemoveBlockAt(new Vector3i((int)sum.x,(int)sum.y+1,(int)sum.z));
							//wgo.world.GenerateBlockAt(new IntVect((int)sum.x,(int)sum.y+1,(int)sum.z), BlockType.Ice);
							spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y+1+0.5f, (int)sum.z+0.5f));
							attachIceToHand();
						//}
					}
	
					// Change block type of block at A
					if (Input.GetKeyDown("k")) {
						//if (isInValidRange(sum)) {
							animation.CrossFade("magicIce", 0.1f);
							//wgo.changeBlockType(sum.x,sum.y,sum.z,4,1.2);
							//wgo.world.RemoveBlockAt(new Vector3i((int)sum.x,(int)sum.y,(int)sum.z));
							//wgo.world.GenerateBlockAt(new IntVect((int)sum.x,(int)sum.y,(int)sum.z), BlockType.Ice);
							spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y+0.5f, (int)sum.z+0.5f));
							attachIceToHand();
						//}
					}
	
					// Change block type of block at E
					if (Input.GetKeyDown(",")) {
	
						//if (isInValidRange(sum)) {
							if (ground_front == ground_above_front && climb_test.y > 0) {	// !A
								animation.CrossFade("magicIce", 0.1f);
								//wgo.changeBlockType(sum.x,sum.y-1,sum.z,4,1.2);
								//wgo.world.RemoveBlockAt(new Vector3i((int)sum.x,(int)sum.y-1,(int)sum.z));
								//wgo.world.GenerateBlockAt(new IntVect((int)sum.x,(int)sum.y-1,(int)sum.z), BlockType.Ice);
								spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y-1+0.5f, (int)sum.z+0.5f));
								attachIceToHand();
							}
						//}
					}
	
				}
	
				if (currChar == "Robot") {
	
					// Destroy block at B
					if (Input.GetKeyDown("y")) {
	
						//if (isInValidRange(sum)) {
	
							animation.CrossFade("destroyBlockU", 0.1f);
							
							//wgo.world.RemoveBlockAt(new Vector3i((int)sum.x,(int)sum.y+1,(int)sum.z));
							spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y+1+0.5f, (int)sum.z+0.5f));
							//wgo.destroyBlock(sum.x,sum.y+1,sum.z,0.72);
						//}
					}
	
					// Destroy block at A
					if (Input.GetKeyDown("h")) {
	
						//if (isInValidRange(sum)) {
	
							animation.CrossFade("destroyBlockM", 0.1f);
							//wgo.world.RemoveBlockAt(new Vector3i((int)sum.x,(int)sum.y,(int)sum.z));
							spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y+0.5f, (int)sum.z+0.5f));
							//wgo.destroyBlock(sum.x,sum.y,sum.z,0.52);
						//}
					}
	
					// Destroy block at E
					if (Input.GetKeyDown("n")) {
	
						//if (isInValidRange(sum)) {
							if (ground_front == ground_above_front && climb_test.y > 0) {	// !A
								animation.CrossFade("destroyBlockD", 0.1f);
								//wgo.world.RemoveBlockAt(new Vector3i((int)sum.x,(int)sum.y-1,(int)sum.z));
								spawnTestFireAtBlockLocation(new Vector3((int)sum.x+0.5f, (int)sum.y-1+0.5f, (int)sum.z+0.5f));
								//wgo.destroyBlock(sum.x,sum.y-1,sum.z,0.6);
							}
						//}
					}
				}
			}
		}
	
		// ------------------------------
	 	
		moveVec = Vector3.zero;
		moveVec = transform.TransformDirection(moveVec);
	
		if (controller.isGrounded && (!m_lock) ) 
		{
			// playIdle();
			if (state == "normal") {
				playIdle();
			}
			else {
				playIdleState(state);
			}
		}
	
		if (!isClimbing) {
			moveVec.y = controller.velocity.y - gravityForce * Time.smoothDeltaTime;
		}
	
		controller.Move(moveVec * Time.deltaTime);
	
		if ( (this.gameObject.name == "Girl") || (this.gameObject.name == "Robot") || (this.gameObject.name == "Ghost") ) {
			if (Input.GetKeyDown ("1")) state = "sad";
			if (Input.GetKeyDown ("2")) state = "happy";
			if (Input.GetKeyDown ("3")) state = "excited";
			if (Input.GetKeyDown ("4")) state = "disgusted";
			if (Input.GetKeyDown ("5")) state = "frightened";
			if (Input.GetKeyDown ("6")) state = "confused";
			if (Input.GetKeyDown ("7")) state = "angry";
			if (Input.GetKeyDown ("8")) state = "normal";
		}
	
		// Block Picking
	 	// var b = gameObject.Find("Cube");
	
	// 	if (Input.GetKeyDown (KeyCode.Space)) {
	// 		if (block) {
	// 			block.collider.enabled = true;
	// 			block.transform.parent = null;
	// 			block.transform.position.y = 0;
	// 			block = null;
	// 		}
	// 		else {
	// 			if (lock == 0) {	// Not turning nor walking
	// 				if (Physics.Raycast(transform.position + Vector3(0,0.5,0), frontDir, Hit, 1)) {
	// 					if(Hit.collider.gameObject.tag == "block") {
	// 						block = Hit.collider.gameObject;
	// 						// Get it
	// 						block.collider.enabled = false;
	// 						block.transform.parent = this.transform;
	// 						block.transform.position.y = 0.25;
	// 					}
	// 				}
	// 				else if (Physics.Raycast(transform.position + Vector3(0,-0.5,0), frontDir, Hit, 1)) {
	// 					if(Hit.collider.gameObject.tag == "block") {
	// 						block = Hit.collider.gameObject;
	// 						// Change material
	// 						block.renderer.material = Resources.Load("Materials/waterMAT");
	// 						block = null;
	// 					}
	// 				}
	// 			}
	// 		}
	// 	}
	
		// ------------------------------------
		// 		GirlQ
		// ------------------------------------
		
		if (this.gameObject.name == "Girl") {
	
			GameObject head = GameObject.Find("Girl/girlQG/bodyG/prt_07");
			head.renderer.material.mainTexture = (Texture)Resources.Load("FaceTEX/girl_face_" + state, typeof(Texture2D));
		}
	
		// ------------------------------------
		// 		Ghost
		// ------------------------------------
		
		if (this.gameObject.name == "Ghost") {
	
			GameObject ghostBody = GameObject.Find("Ghost/ghostG/ghost_prt_04");
			GameObject iconPlaneG = GameObject.Find("Ghost/ghostG/icon_planeG");
			GameObject iconPlane1 = GameObject.Find("Ghost/ghostG/icon_planeG/icon_plane1");
			GameObject rootJ = GameObject.Find("Ghost/RootJ");
	
			if (Input.GetKeyDown ("1")) ghostBody.renderer.material.SetColor("_Emission", new Color(0.25f, 0.25f, 0.25f));
			else if (Input.GetKeyDown ("2")) ghostBody.renderer.material.SetColor("_Emission", new Color(0.6f, 0.296f, 0.16f));
			else if (Input.GetKeyDown ("3")) ghostBody.renderer.material.SetColor("_Emission", new Color(0.714f, 0.5f, 0.277f));
			else if (Input.GetKeyDown ("4")) ghostBody.renderer.material.SetColor("_Emission", new Color(0.25f, 0.25f, 0.3f));
			else if (Input.GetKeyDown ("5")) ghostBody.renderer.material.SetColor("_Emission", new Color(0.2f, 0.2f, 0.2f));
			else if (Input.GetKeyDown ("6")) ghostBody.renderer.material.SetColor("_Emission", new Color(0.363f, 0.363f, 0.363f));
			else if (Input.GetKeyDown ("7")) ghostBody.renderer.material.SetColor("_Emission", new Color(0.63f, 0.326f, 0.3f));
			else if (Input.GetKeyDown ("8")) ghostBody.renderer.material.SetColor("_Emission", new Color(0.363f, 0.363f, 0.363f));
	
		 	Vector3 v  = Camera.main.transform.position - iconPlaneG.transform.position;
		  	v.x = v.z = 0.0f;
	
		  	iconPlane1.renderer.material.mainTexture = (Texture)Resources.Load("EmoticonTEX/icon_" + state, typeof(Texture2D));
	  		iconPlaneG.transform.position = rootJ.transform.position + new Vector3(0.0f, 1.1f, 0.0f);
	  		iconPlaneG.transform.LookAt( Camera.main.transform.position - v );
		}
	
		// ------------------------------------
		// 		For all 4
		// ------------------------------------
		
		if (transform.position.y < -2) transform.position = new Vector3(UnityEngine.Random.Range(0,7)+0.5f, 10.0f, UnityEngine.Random.Range(0,7)+0.5f);
	
	}
	
	// ---------------------------------------------------------------------------------------------
	
	public void OnGUI () {
		// GUI.Label (Rect (25, 200, 250, 25), "	state  >>  " + state);
	}
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
}



