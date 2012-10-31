		
using UnityEngine;
using System.Collections;


[AddComponentMenu("Mixamo/Demo/Root Motion Character")]
public class RootMotionCharacterControlDRAGON : MonoBehaviour
{
	public float turningSpeed = 90f;
	public RootMotionComputer computer;
	public CharacterController character;
	
	void Start()
	{
		// validate component references
		if (computer == null) computer = GetComponent(typeof(RootMotionComputer)) as RootMotionComputer;
		if (character == null) character = GetComponent(typeof(CharacterController)) as CharacterController;
		
		// tell the computer to just output values but not apply motion
		computer.applyMotion = false;
		// tell the computer that this script will manage its execution
		computer.isManagedExternally = true;
		// since we are using a character controller, we only want the z translation output
		computer.computationMode = RootMotionComputationMode.ZTranslation;
		// initialize the computer
		computer.Initialize();
		
		// set up properties for the animations
		animation["idle"].layer = 0; animation["idle"].wrapMode = WrapMode.Loop;
		animation["walk"].layer = 1; animation["walk"].wrapMode = WrapMode.Loop;
		animation["run"].layer = 1; animation["run"].wrapMode = WrapMode.Loop;
		animation["fly"].layer = 3; animation["fly"].wrapMode = WrapMode.Loop;
		animation["glide"].layer = 3; animation["glide"].wrapMode = WrapMode.Once;
		animation["takeoff"].layer = 3; animation["takeoff"].wrapMode = WrapMode.Once;
		animation["fireBreath"].layer = 3; animation["fireBreath"].wrapMode = WrapMode.Once;
		
        //animation.Play("idle");
        animation.Play("fly");
		
	}
	
	void Update()
	{
		float targetMovementWeight = 0f;
		float throttle = 0f;
		
		// turning keys
		if (Input.GetKey(KeyCode.Q)) transform.Rotate(Vector3.down, turningSpeed*Time.deltaTime);
		if (Input.GetKey(KeyCode.E)) transform.Rotate(Vector3.up, turningSpeed*Time.deltaTime);
		
		// forward movement keys
		// ensure that the locomotion animations always blend from idle to moving at the beginning of their cycles
		if (Input.GetKeyDown(KeyCode.R) && 
			(animation["walk"].weight == 0f || animation["run"].weight == 0f))
		{
			animation["walk"].normalizedTime = 0f;
			animation["run"].normalizedTime = 0f;
		}
		if (Input.GetKey(KeyCode.R))
		{
			targetMovementWeight = 1f;
		}
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) throttle = 1f;
				
		// blend in the movement

		animation.Blend("run", targetMovementWeight*throttle, 0.5f);
		animation.Blend("walk", targetMovementWeight*(1f-throttle), 0.5f);
		// synchronize timing of the footsteps
		animation.SyncLayer(1);
		
		// all the other animations, such as punch, kick, attach, reaction, etc. go here
		if (Input.GetKeyDown(KeyCode.Alpha1)) animation.CrossFade("fly", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha2)) animation.CrossFade("takeoff", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha3)) animation.CrossFade("glide", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha4)) animation.CrossFade("fireBreath", 0.2f);
		
		
	}
	
	void LateUpdate()
	{
		computer.ComputeRootMotion();
		
		// move the character using the computer's output
		character.SimpleMove(transform.TransformDirection(computer.deltaPosition)/Time.deltaTime);
	}
}